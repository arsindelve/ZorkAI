using ChatLambda;
using CloudWatch;
using CloudWatch.Model;
using GameEngine;
using GameEngine.Diagnostics;
using GameEngine.Item;
using Model.AIGeneration;
using Model.AIParsing;
using Model.Interface;
using SecretsManager;

namespace UnitTests.Parser.Census;

/// <summary>One freshly started game: how to send it a line of input, and its state.</summary>
/// <param name="Snapshot">The whole game state as the save-game serialiser writes it.</param>
public sealed record CensusSession(Func<string?, Task<string?>> Respond, IContext Context, Func<bool> NarratorWasAsked,
    Func<string> Snapshot);

/// <summary>
///     How to start a hermetic, fresh copy of one game for a single probe: no AI, no AWS, no clock. Each
///     game's test project supplies one, because only it can name the game's types (its starting room,
///     its lamp, its companion's chat seam).
/// </summary>
public sealed class CensusGame
{
    private readonly Func<IIntentParser, IAITakeAndAndDropParser, CensusSession> _start;

    private CensusGame(string name, IGlobalCommandFactory commands,
        Func<IIntentParser, IAITakeAndAndDropParser, CensusSession> start, Action<IContext> light)
    {
        Name = name;
        Commands = commands;
        _start = start;
        Light = light;
    }

    /// <summary>The game's assembly name, which is also what the vocabulary harvest is keyed by.</summary>
    public string Name { get; }

    public IGlobalCommandFactory Commands { get; }

    /// <summary>Gives the player a light, for rooms that are dark at the start of the game.</summary>
    public Action<IContext> Light { get; }

    public CensusSession Start(IIntentParser parser, IAITakeAndAndDropParser takeAndDrop)
    {
        return _start(parser, takeAndDrop);
    }

    /// <param name="boot">
    ///     What the game's own test base does after building the engine: seed the starting room, stub
    ///     anything that would reach a network.
    /// </param>
    /// <param name="light">How to light a dark room in this game.</param>
    public static CensusGame For<TGame, TContext>(Action<TContext> boot, Action<IContext> light)
        where TGame : IInfocomGame, new()
        where TContext : class, IContext, new()
    {
        var game = new TGame();

        return new CensusGame(game.GameName, game.GetGlobalCommandFactory(), (parser, takeAndDrop) =>
        {
            var conversation = new Mock<IParseConversation>();
            conversation.Setup(parse => parse.ParseAsync(It.IsAny<string>())).ReturnsAsync((false, ""));

            // Every fall-through to the narrator is recorded: that is the line between an interaction the
            // game authors an answer for and one it leaves the AI to improvise.
            var narrator = new LeakRecordingGenerationClient(new Mock<IGenerationClient>().Object, () => ("", ""));

            var engine = new GameEngine<TGame, TContext>(new ItemProcessorFactory(takeAndDrop), parser, narrator,
                Mock.Of<ISecretsManager>(), Mock.Of<ICloudWatchLogger<TurnLog>>(), conversation.Object);

            engine.Context.Verbosity = Verbosity.Verbose;
            boot(engine.Context);
            engine.Context.LastInput = null;
            engine.Context.LastResponse = null;

            return new CensusSession(engine.GetResponse, engine.Context, () => narrator.CompletenessLeaks.Count > 0,
                engine.SaveGame);
        }, light);
    }
}
