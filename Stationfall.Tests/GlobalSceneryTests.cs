using FluentAssertions;
using GameEngine;
using GameEngine.Item;
using GameEngine.Item.ItemProcessor;
using Model.AIGeneration;
using Model.AIParsing;
using Model.Intent;
using Model.Interaction;
using Moq;
using Stationfall.Location.Duffy;

namespace Stationfall.Tests;

/// <summary>
///     The nouns <see cref="StationfallGame.GlobalScenery" /> answers for in every room — walls, floor,
///     ceiling, the air, the player's own body. They exist because an improvised answer is
///     indistinguishable from a real one and therefore worse than nothing (issue #563), so what matters
///     is not that the strings exist but that a player reaches them by typing the obvious thing.
///     <para>
///         Issue #577: they were reachable only by phrasing luck. Which intent shape the live parser
///         produces for a take varies by noun, and only one of the two shapes ever consulted scenery —
///         "get wall" answered, "take wall" got improvised prose that contradicted the game's own facts
///         about the player. These pin both shapes to the same authored answer.
///     </para>
/// </summary>
[TestFixture]
public class GlobalSceneryTests : EngineTestsBase
{
    private static Mock<IAITakeAndAndDropParser> ParserThatFindsNothingInTheRoom()
    {
        // What the real take/drop list-parser does with a scenery noun: it is shown the room
        // description and asked which of those things the player means, and a wall is not one of them.
        var parser = new Mock<IAITakeAndAndDropParser>();
        parser.Setup(s => s.GetListOfItemsToTake(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync([]);
        return parser;
    }

    [Test]
    public async Task TakeWall_ViaTakeIntent_GivesTheAuthoredRefusal()
    {
        // The reported repro, verbatim: on prod, "get wall" on Deck Twelve answered with the bulkhead
        // line while "take wall" — the canonical verb the line was written for — reached no handler at
        // all. Production's real AI parser tags it as a TakeIntent, which GameEngine dispatches
        // straight to TakeOrDropInteractionProcessor.Process(TakeIntent, ...); TestParser can only
        // ever produce a SimpleIntent, so the overload has to be invoked directly as GameEngine does.
        var engine = GetTarget();
        engine.Context.CurrentLocation = Repository.GetLocation<DeckTwelve>();

        var processor = new TakeOrDropInteractionProcessor(ParserThatFindsNothingInTheRoom().Object);
        var (_, message) = await processor.Process(
            new TakeIntent { Noun = "wall", OriginalInput = "take wall" },
            engine.Context, Mock.Of<IGenerationClient>());

        message.Should().Contain("The bulkheads are structural, and you are not.");
    }

    [Test]
    public async Task EveryGlobalSceneryNoun_RefusesTheSameWay_ThroughBothIntentShapes()
    {
        // Game-wide rather than per-noun: a new entry in GlobalScenery is covered the day it is added.
        var engine = GetTarget();
        var location = Repository.GetLocation<DeckTwelve>();
        engine.Context.CurrentLocation = location;

        var client = Mock.Of<IGenerationClient>();
        var takeAndDropParser = ParserThatFindsNothingInTheRoom();
        var processor = new TakeOrDropInteractionProcessor(takeAndDropParser.Object);
        var factory = new ItemProcessorFactory(takeAndDropParser.Object);

        foreach (var scenery in new StationfallGame().GlobalScenery)
        foreach (var noun in scenery.Nouns)
        {
            var expected = scenery.CannotBeTakenReason!;
            expected.Should().NotBeNull($"'{noun}' needs an authored reason it can't be taken");

            var viaSimpleIntent = await location.RespondToSimpleInteraction(
                new SimpleIntent { Verb = "get", Noun = noun, OriginalInput = $"get {noun}" },
                engine.Context, client, factory);

            var (_, viaTakeIntent) = await processor.Process(
                new TakeIntent { Noun = noun, OriginalInput = $"take {noun}" }, engine.Context, client);

            viaSimpleIntent.InteractionMessage.Should().Be(expected, $"get {noun}");
            viaTakeIntent.Should().Be(expected, $"take {noun}");
        }
    }
}
