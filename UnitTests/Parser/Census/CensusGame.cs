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
/// <param name="Context">The game the session began with.</param>
/// <param name="Snapshot">The whole game state as the save-game serialiser writes it.</param>
/// <param name="Now">
///     The game as it stands. Not always the one the session began with: a death starts the game over,
///     with a new context and new objects.
/// </param>
public sealed record CensusSession(Func<string?, Task<string?>> Respond, IContext Context, Func<bool> NarratorWasAsked,
    Func<string> Snapshot, Func<IContext> Now);

/// <summary>
///     How to start a hermetic, fresh copy of one game for a single probe: no AI, no AWS, no clock. Each
///     game's test project supplies one, because only it can name the game's types (its starting room,
///     its lamp, its companion's chat seam).
/// </summary>
public sealed class CensusGame
{
    private readonly Func<IIntentParser, IAITakeAndAndDropParser, CensusSession> _start;

    private CensusGame(string name, IGlobalCommandFactory commands,
        Func<IIntentParser, IAITakeAndAndDropParser, CensusSession> start, Action<IContext> light,
        Action<IContext> settle, Action<IContext> afterRestart)
    {
        Name = name;
        Commands = commands;
        _start = start;
        Light = light;
        Settle = settle;
        AfterRestart = afterRestart;
    }

    /// <summary>The game's assembly name, which is also what the vocabulary harvest is keyed by.</summary>
    public string Name { get; }

    public IGlobalCommandFactory Commands { get; }

    /// <summary>Gives the player a light, for rooms that are dark at the start of the game.</summary>
    public Action<IContext> Light { get; }

    /// <summary>
    ///     Fixes whatever the game rolls as its objects are made, once a probe's room and items are
    ///     loaded: a battery's charge, say. Two fresh games must be the same game, or every turn in one
    ///     looks different from every turn in the other.
    /// </summary>
    public Action<IContext> Settle { get; }

    /// <summary>
    ///     Does for a game that has just started over what the boot does for a fresh one. A probe that
    ///     kills the player ends in a restarted game, whose objects were made - and rolled their dice -
    ///     in the middle of the turn. Every fatal verb has to end in the same restarted game, or each
    ///     one looks like a different interaction.
    /// </summary>
    public Action<IContext> AfterRestart { get; }

    public CensusSession Start(IIntentParser parser, IAITakeAndAndDropParser takeAndDrop)
    {
        return _start(parser, takeAndDrop);
    }

    /// <param name="boot">
    ///     What the game's own test base does after building the engine: seed the starting room, stub
    ///     anything that would reach a network.
    /// </param>
    /// <param name="light">How to light a dark room in this game.</param>
    /// <param name="settle">See <see cref="Settle" />. Nothing, for a game that rolls nothing at the start.</param>
    /// <param name="afterRestart">See <see cref="AfterRestart" />. Nothing, likewise.</param>
    public static CensusGame For<TGame, TContext>(Action<TContext> boot, Action<IContext> light,
        Action<IContext>? settle = null, Action<IContext>? afterRestart = null)
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
                engine.SaveGame, () => engine.Context);
        }, light, settle ?? (_ => { }), afterRestart ?? (_ => { }));
    }
}
