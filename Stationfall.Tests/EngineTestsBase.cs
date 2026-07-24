using ChatLambda;
using CloudWatch;
using CloudWatch.Model;
using GameEngine;
using GameEngine.Item;
using Model;
using Model.AIGeneration;
using Model.AIParsing;
using Model.Interface;
using Model.Item;
using Model.Location;
using Moq;
using Stationfall.GlobalCommand;
using Stationfall.Location;
using UnitTests;

namespace Stationfall.Tests;

/// <summary>
///     Test harness for Stationfall, mirroring <c>Planetfall.Tests.EngineTestsBase</c>: a fully-mocked
///     GameEngine (no real AI, AWS, randomness, or network) that boots the game so tests can drive it
///     through <c>GetResponse</c>. Phase 2's walkthrough tests will build on this.
/// </summary>
public class EngineTestsBase
{
    private Mock<IGenerationClient> _client = new();
    private IIntentParser _parser = Mock.Of<IIntentParser>();
    private GameEngine<StationfallGame, StationfallContext>? _storedEngine;

    protected StationfallContext Context =>
        _storedEngine?.Context ?? throw new InvalidOperationException("Engine not initialized");

    protected Mock<IParseConversation> ParseConversationMock { get; private set; } = null!;

    protected GameEngine<StationfallGame, StationfallContext> GetTarget(IIntentParser? parser = null)
    {
        _client = new Mock<IGenerationClient>();
        _parser = parser ?? new TestParser(new StationfallGlobalCommandFactory(), "Stationfall");

        Repository.Reset();

        var takeAndDropParser = new Mock<IAITakeAndAndDropParser>();
        takeAndDropParser.Setup(s => s.GetListOfItemsToDrop(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((string input, string _) =>
            {
                var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                return words.Length > 1 ? [words[1]] : [];
            });

        takeAndDropParser.Setup(s => s.GetListOfItemsToTake(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((string input, string _) =>
            {
                var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                return words.Length > 1 ? [words[1]] : [];
            });

        // Never treat input as conversational — Stationfall has no talkable NPCs wired up yet.
        ParseConversationMock = new Mock<IParseConversation>();
        ParseConversationMock.Setup(x => x.ParseAsync(It.IsAny<string>()))
            .ReturnsAsync((false, ""));

        var engine = new GameEngine<StationfallGame, StationfallContext>(
            new ItemProcessorFactory(takeAndDropParser.Object), _parser, _client.Object,
            Mock.Of<ISecretsManager>(), Mock.Of<ICloudWatchLogger<TurnLog>>(), ParseConversationMock.Object);
        engine.Context.Verbosity = Verbosity.Verbose;
        Repository.GetLocation<DeckTwelve>().Init();

        _storedEngine = engine;
        engine.Context.LastInput = null;
        engine.Context.LastResponse = null;

        return engine;
    }

    protected T GetItem<T>() where T : IItem, new()
    {
        return Repository.GetItem<T>();
    }

    protected T GetLocation<T>() where T : class, ILocation, new()
    {
        return Repository.GetLocation<T>();
    }
}
