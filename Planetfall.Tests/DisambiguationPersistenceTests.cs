using FluentAssertions;
using GameEngine;
using NUnit.Framework;
using Planetfall.Item.Kalamontee.Admin;
using Planetfall.Location.Kalamontee;

namespace Planetfall.Tests;

/// <summary>
///     Issue #472, reproduced on the exact families the bug was reported against (the access cards). The
///     deployed game is stateless: the pending "which card do you mean?" prompt lives in an in-memory
///     engine field that is not serialized, so before the fix the player's answer arrived on a rebuilt
///     engine with no pending prompt and was parsed as a fresh command — "no effect", a re-prompt, or the
///     notorious movement side effect where answering "kitchen" walked the player toward the Kitchen.
///
///     Each test serializes + deserializes the game between the prompt and the answer, mirroring the
///     Lambda save/restore round-trip; the single-process CardTests do not exercise this boundary.
/// </summary>
public class DisambiguationPersistenceTests : EngineTestsBase
{
    private GameEngine<PlanetfallGame, PlanetfallContext> RoundTrip(GameEngine<PlanetfallGame, PlanetfallContext> engine)
    {
        var saved = engine.SaveGame();
        var restored = GetTarget();
        restored.RestoreGame(saved);
        return restored;
    }

    [Test]
    [TestCase("kitchen")]
    [TestCase("kitchen access")]
    [TestCase("kitchen card")]
    [TestCase("kitchen access card")]
    public async Task CardDisambiguation_AnswerResolves_AcrossSaveRestore(string reply)
    {
        var engine = GetTarget();
        engine.Context.CurrentLocation = Repository.GetLocation<MessCorridor>();
        engine.Context.ItemPlacedHere(Repository.GetItem<KitchenAccessCard>());
        engine.Context.ItemPlacedHere(Repository.GetItem<ShuttleAccessCard>());

        var prompt = await engine.GetResponse("drop card");
        prompt.Should().Contain("Do you mean the kitchen access card or the shuttle access card?");

        var restored = RoundTrip(engine);

        var response = await restored.GetResponse(reply);

        response.Should().Contain("Dropped");
        Repository.GetItem<KitchenAccessCard>().CurrentLocation.Should().BeOfType<MessCorridor>();
        restored.Context.Items.Should().ContainSingle().Which.Should().Be(Repository.GetItem<ShuttleAccessCard>());
    }

    // The issue's headline movement side effect: after the prompt is lost, answering "kitchen" parses as
    // a movement command ("You can't get there from here."). Once the prompt persists, "kitchen" resolves
    // to the kitchen card and drops it — it must NOT be treated as a move.
    [Test]
    public async Task AnsweringKitchen_ResolvesTheCard_DoesNotWalkTowardTheKitchen()
    {
        var engine = GetTarget();
        engine.Context.CurrentLocation = Repository.GetLocation<MessCorridor>();
        engine.Context.ItemPlacedHere(Repository.GetItem<KitchenAccessCard>());
        engine.Context.ItemPlacedHere(Repository.GetItem<ShuttleAccessCard>());

        await engine.GetResponse("drop card");
        var restored = RoundTrip(engine);

        var response = await restored.GetResponse("kitchen");

        response.Should().Contain("Dropped");
        response.Should().NotContain("get there", "the answer must resolve the prompt, not parse as movement");
        restored.Context.CurrentLocation.Should().BeOfType<MessCorridor>("the player did not move");
    }

    // The issue's other reported phrasing, exercising the multi-noun engine ("slide X through slot")
    // rather than the drop path — same _processorInProgress mechanism, so it must round-trip too.
    [Test]
    [TestCase("kitchen")]
    [TestCase("kitchen access card")]
    public async Task SlideCardThroughSlot_Disambiguation_AnswerResolves_AcrossSaveRestore(string reply)
    {
        var engine = GetTarget();
        engine.Context.CurrentLocation = Repository.GetLocation<MessHall>();
        engine.Context.ItemPlacedHere(Repository.GetItem<KitchenAccessCard>());
        engine.Context.ItemPlacedHere(Repository.GetItem<ShuttleAccessCard>());

        var prompt = await engine.GetResponse("slide card through slot");
        prompt.Should().Contain("Do you mean the kitchen access card or the shuttle access card?");

        var restored = RoundTrip(engine);

        var response = await restored.GetResponse(reply);

        response.Should().Contain("The kitchen door quietly slides open");
    }

    [Test]
    public async Task WrongCard_Disambiguation_AnswerResolves_AcrossSaveRestore()
    {
        var engine = GetTarget();
        engine.Context.CurrentLocation = Repository.GetLocation<MessCorridor>();
        engine.Context.ItemPlacedHere(Repository.GetItem<ShuttleAccessCard>());
        engine.Context.ItemPlacedHere(Repository.GetItem<KitchenAccessCard>());

        await engine.GetResponse("drop card");
        var restored = RoundTrip(engine);

        var response = await restored.GetResponse("shuttle");

        response.Should().Contain("Dropped");
        Repository.GetItem<ShuttleAccessCard>().CurrentLocation.Should().BeOfType<MessCorridor>();
        restored.Context.HasItem<KitchenAccessCard>().Should().BeTrue("only the shuttle card was chosen");
    }
}
