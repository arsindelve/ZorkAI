using Lambda.Controllers;
using Microsoft.Extensions.Logging;
using Model.AIGeneration;
using Model.AIGeneration.Requests;
using Model.Interface;
using Model.Web;

namespace UnitTests.PlanetfallLambda;

[TestFixture]
public class PlanetfallControllerTests
{
    [SetUp]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<PlanetfallController>>();
        _mockEngine = new Mock<IGameEngine>();
        _mockSessionRepository = new Mock<ISessionRepository>();
        _mockSavedGameRepository = new Mock<ISavedGameRepository>();
        _mockGenerationClient = new Mock<IGenerationClient>();

        _mockEngine.Setup(e => e.GenerationClient).Returns(_mockGenerationClient.Object);
        _mockEngine.Setup(e => e.LocationDescription).Returns("Test Location");
        _mockEngine.Setup(e => e.IntroText).Returns("Welcome to Planetfall!");
        _mockEngine.Setup(e => e.Moves).Returns(1);
        _mockEngine.Setup(e => e.SaveGame()).Returns("serialized game state");

        _controller = new PlanetfallController(
            _mockLogger.Object,
            _mockEngine.Object,
            _mockSessionRepository.Object,
            _mockSavedGameRepository.Object,
            new Mock<Model.Hints.IHintLanguageModel>().Object);
    }

    private Mock<ILogger<PlanetfallController>> _mockLogger;
    private Mock<IGameEngine> _mockEngine;
    private Mock<ISessionRepository> _mockSessionRepository;
    private Mock<ISavedGameRepository> _mockSavedGameRepository;
    private Mock<IGenerationClient> _mockGenerationClient;
    private PlanetfallController _controller;

    [TestFixture]
    public class IndexPostMethod : PlanetfallControllerTests
    {
        [Test]
        public async Task Should_InitializeEngine_When_Called()
        {
            // Arrange
            var request = new GameRequest("look", "test-session");
            _mockEngine.Setup(e => e.GetResponse("look")).ReturnsAsync("You are in a room.");
            _mockSessionRepository.Setup(r => r.GetSessionState("test-session", "planetfall_session"))
                .ReturnsAsync((string?)null);
            _mockSessionRepository.Setup(r =>
                    r.WriteSessionState(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            await _controller.Index(request);

            // Assert
            _mockEngine.Verify(e => e.InitializeEngine(), Times.Once);
        }

        [Test]
        public async Task Should_GetResponse_When_ValidInput()
        {
            // Arrange
            var request = new GameRequest("north", "test-session");
            _mockEngine.Setup(e => e.GetResponse("north")).ReturnsAsync("You go north.");
            _mockSessionRepository.Setup(r => r.GetSessionState("test-session", "planetfall_session"))
                .ReturnsAsync((string?)null);
            _mockSessionRepository.Setup(r =>
                    r.WriteSessionState(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Index(request);

            // Assert
            result.Should().NotBeNull();
            _mockEngine.Verify(e => e.GetResponse("north"), Times.Once);
        }

        [Test]
        public async Task Should_RestoreSession_When_SavedSessionExists()
        {
            // Arrange
            var request = new GameRequest("look", "test-session");
            var savedData = "dGVzdCBzYXZlZCBkYXRh"; // Base64 encoded "test saved data"
            _mockEngine.Setup(e => e.GetResponse("look")).ReturnsAsync("You are in a room.");
            _mockSessionRepository.Setup(r => r.GetSessionState("test-session", "planetfall_session"))
                .ReturnsAsync(savedData);
            _mockSessionRepository.Setup(r =>
                    r.WriteSessionState(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            await _controller.Index(request);

            // Assert
            _mockEngine.Verify(e => e.RestoreGame("test saved data"), Times.Once);
        }

        [Test]
        public async Task Should_WriteSession_When_ProcessingComplete()
        {
            // Arrange
            var request = new GameRequest("inventory", "test-session");
            _mockEngine.Setup(e => e.GetResponse("inventory")).ReturnsAsync("You are empty-handed.");
            _mockEngine.Setup(e => e.SaveGame()).Returns("serialized game state");
            _mockSessionRepository.Setup(r => r.GetSessionState("test-session", "planetfall_session"))
                .ReturnsAsync((string?)null);
            _mockSessionRepository.Setup(r =>
                    r.WriteSessionState(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            await _controller.Index(request);

            // Assert
            _mockSessionRepository.Verify(r => r.WriteSessionState(
                "test-session",
                It.IsAny<string>(),
                "planetfall_session"), Times.Once);
        }

        [Test]
        public async Task Should_LogInputAndResponse_When_Processing()
        {
            // Arrange
            var request = new GameRequest("examine floyd", "test-session");
            _mockEngine.Setup(e => e.GetResponse("examine floyd")).ReturnsAsync("Floyd is a lovable robot.");
            _mockSessionRepository.Setup(r => r.GetSessionState("test-session", "planetfall_session"))
                .ReturnsAsync((string?)null);
            _mockSessionRepository.Setup(r =>
                    r.WriteSessionState(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            await _controller.Index(request);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Request: examine floyd")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Response: Floyd is a lovable robot.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }

    [TestFixture]
    public class RestoreGameMethod : PlanetfallControllerTests
    {
        [Test]
        public async Task Should_InitializeEngine_When_Called()
        {
            // Arrange
            var request = new RestoreGameRequest("session-id", "client-id", "save-id");
            _mockSavedGameRepository.Setup(r => r.GetSavedGame("save-id", "client-id", "planetfall_savegame"))
                .ReturnsAsync("dGVzdCBzYXZlZCBkYXRh"); // Base64 encoded data
            _mockEngine.Setup(e => e.GetResponse("look")).ReturnsAsync("You are here.");
            _mockGenerationClient.Setup(g => g.GenerateNarration(It.IsAny<Request>(), It.IsAny<string>()))
                .ReturnsAsync("Game restored successfully.");
            _mockSessionRepository.Setup(r =>
                    r.WriteSessionState(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            await _controller.RestoreGame(request);

            // Assert
            _mockEngine.Verify(e => e.InitializeEngine(), Times.Once);
        }

        [Test]
        public async Task Should_ThrowException_When_SavedGameNotFound()
        {
            // Arrange
            var request = new RestoreGameRequest("session-id", "client-id", "invalid-id");
            _mockSavedGameRepository.Setup(r => r.GetSavedGame("invalid-id", "client-id", "planetfall_savegame"))
                .ReturnsAsync((string?)null);

            // Act & Assert
            var exception = await FluentActions.Invoking(() => _controller.RestoreGame(request))
                .Should().ThrowAsync<ArgumentException>();
            exception.WithMessage("Saved gamed invalid-id had empty game data");
        }

        [Test]
        public async Task Should_RestoreGameAndGenerateResponse_When_ValidSaveData()
        {
            // Arrange
            var request = new RestoreGameRequest("session-id", "client-id", "save-id");
            var savedData = "dGVzdCBzYXZlZCBkYXRh"; // Base64 encoded "test saved data"
            _mockSavedGameRepository.Setup(r => r.GetSavedGame("save-id", "client-id", "planetfall_savegame"))
                .ReturnsAsync(savedData);
            _mockEngine.Setup(e => e.GetResponse("look")).ReturnsAsync("You are in a restored location.");
            _mockGenerationClient.Setup(g => g.GenerateNarration(It.IsAny<Request>(), It.IsAny<string>()))
                .ReturnsAsync("Welcome back to Planetfall!");
            _mockSessionRepository.Setup(r =>
                    r.WriteSessionState(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.RestoreGame(request);

            // Assert
            _mockEngine.Verify(e => e.RestoreGame("test saved data"), Times.Once);
            _mockEngine.Verify(e => e.GetResponse("look"), Times.Once);
            result.Should().NotBeNull();
            result.Response.Should().Contain("Welcome back to Planetfall!");
            result.Response.Should().Contain("You are in a restored location.");
        }
    }

    [TestFixture]
    public class SaveGameMethod : PlanetfallControllerTests
    {
        [Test]
        public async Task Should_InitializeEngine_When_Called()
        {
            // Arrange
            var request = new SaveGameRequest("session-id", "client-id", "My Save", "save-id");
            _mockSessionRepository.Setup(r => r.GetSessionState("session-id", "planetfall_session"))
                .ReturnsAsync("dGVzdCBzYXZlZCBkYXRh");
            _mockEngine.Setup(e => e.SaveGame()).Returns("game state");
            _mockEngine.Setup(e => e.GenerateSaveGameNarration()).ReturnsAsync("Game saved successfully.");
            _mockSavedGameRepository.Setup(r => r.SaveGame(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("saved-game-id");

            // Act
            await _controller.SaveGame(request);

            // Assert
            _mockEngine.Verify(e => e.InitializeEngine(), Times.Once);
        }

        [Test]
        public async Task Should_ThrowException_When_SessionEmpty()
        {
            // Arrange
            var request = new SaveGameRequest("session-id", "client-id", "My Save", "save-id");
            _mockSessionRepository.Setup(r => r.GetSessionState("session-id", "planetfall_session"))
                .ReturnsAsync((string?)null);

            // Act & Assert
            var exception = await FluentActions.Invoking(() => _controller.SaveGame(request))
                .Should().ThrowAsync<ArgumentException>();
            exception.WithMessage("Session had empty game data before attempting save game.");
        }

        [Test]
        public async Task Should_SaveGame_When_ValidSession()
        {
            // Arrange
            var request = new SaveGameRequest("session-id", "client-id", "My Save", "save-id");
            var sessionData = "dGVzdCBzYXZlZCBkYXRh"; // Base64 encoded data
            _mockSessionRepository.Setup(r => r.GetSessionState("session-id", "planetfall_session"))
                .ReturnsAsync(sessionData);
            _mockEngine.Setup(e => e.SaveGame()).Returns("current game state");
            _mockEngine.Setup(e => e.GenerateSaveGameNarration()).ReturnsAsync("Game saved successfully.");
            _mockSavedGameRepository.Setup(r => r.SaveGame(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("saved-game-id");

            // Act
            var result = await _controller.SaveGame(request);

            // Assert
            _mockEngine.Verify(e => e.RestoreGame("test saved data"), Times.Once);
            _mockSavedGameRepository.Verify(r => r.SaveGame(
                "save-id",
                "client-id",
                "My Save",
                It.IsAny<string>(),
                "planetfall_savegame"), Times.Once);
            _mockEngine.Verify(e => e.GenerateSaveGameNarration(), Times.Once);
            result.Should().Be("Game saved successfully.");
        }
    }

    [TestFixture]
    public class GetAllSavedGamesMethod : PlanetfallControllerTests
    {
        [Test]
        [Ignore("GetAllSavedGames method does not initialize engine in Planetfall controller")]
        public async Task Should_InitializeEngine_When_Called()
        {
            // Arrange
            var sessionId = "test-session";
            _mockSavedGameRepository.Setup(r => r.GetSavedGames(sessionId, "planetfall_savegame"))
                .ReturnsAsync(new List<(string Id, string Name, DateTime SavedOn)>());

            // Act
            await _controller.GetAllSavedGames(sessionId);

            // Assert
            _mockEngine.Verify(e => e.InitializeEngine(), Times.Once);
        }

        [Test]
        public async Task Should_ReturnOrderedSavedGames_When_GamesExist()
        {
            // Arrange
            var sessionId = "test-session";
            var savedGames = new List<(string Id, string Name, DateTime SavedOn)>
            {
                ("save1", "First Save", DateTime.Now.AddDays(-1)),
                ("save2", "Second Save", DateTime.Now)
            };
            _mockSavedGameRepository.Setup(r => r.GetSavedGames(sessionId, "planetfall_savegame"))
                .ReturnsAsync(savedGames);

            // Act
            var result = await _controller.GetAllSavedGames(sessionId);

            // Assert
            result.Should().HaveCount(2);
            result[0].Name.Should().Be("Second Save"); // Most recent first
            result[1].Name.Should().Be("First Save");
        }
    }

    [TestFixture]
    public class IndexGetMethod : PlanetfallControllerTests
    {
        [Test]
        public async Task Should_InitializeEngine_When_Called()
        {
            // Arrange
            var sessionId = "test-session";
            _mockSessionRepository.Setup(r => r.GetSessionState(sessionId, "planetfall_session"))
                .ReturnsAsync((string?)null);

            // Act
            await _controller.Index(sessionId);

            // Assert
            _mockEngine.Verify(e => e.InitializeEngine(), Times.Once);
        }

        [Test]
        public async Task Should_ReturnIntroText_When_NoSavedSession()
        {
            // Arrange
            var sessionId = "new-session";
            _mockSessionRepository.Setup(r => r.GetSessionState(sessionId, "planetfall_session"))
                .ReturnsAsync((string?)null);

            // Act
            var result = await _controller.Index(sessionId);

            // Assert
            result.Should().NotBeNull();
            result.Response.Should().Be("Welcome to Planetfall!");
        }

        [Test]
        public async Task Should_RestoreSessionAndReturnLookResponse_When_SessionExists()
        {
            // Arrange
            var sessionId = "existing-session";
            var savedData = "dGVzdCBzYXZlZCBkYXRh"; // Base64 encoded data
            _mockSessionRepository.Setup(r => r.GetSessionState(sessionId, "planetfall_session"))
                .ReturnsAsync(savedData);
            _mockEngine.Setup(e => e.GetResponse("look")).ReturnsAsync("You are in the shuttle bay.");

            // Act
            var result = await _controller.Index(sessionId);

            // Assert
            _mockEngine.Verify(e => e.RestoreGame("test saved data"), Times.Once);
            _mockEngine.Verify(e => e.GetResponse("look"), Times.Once);
            result.Should().NotBeNull();
            result.Response.Should().Be("You are in the shuttle bay.");
        }
    }

    [TestFixture]
    public class HintPostMethod : PlanetfallControllerTests
    {
        [Test]
        public async Task Should_ReturnNoGameMessage_When_SessionMissing_ButHintConversationExists()
        {
            // No saved session, yet the client replays a hint conversation — the player was clearly
            // mid-game, so the session is stale/lost. Refuse honestly instead of giving opening hints.
            _mockSessionRepository.Setup(r => r.GetSessionState("ghost-session", "planetfall_session"))
                .ReturnsAsync((string?)null);

            var result = await _controller.Hint(new HintApiRequest("ghost-session", "what do I do?",
                new List<Model.Hints.HintExchange> {new("earlier q", "earlier a")}));

            // Not an opening-scene hint and not a 500 — a clear "no game yet" message, flagged as a
            // non-hint so the client shows it without recording it into the conversation...
            result.Text.Should().Contain("can't find a game");
            result.IsHint.Should().BeFalse();
            // ...and nothing was persisted.
            _mockSessionRepository.Verify(
                r => r.WriteSessionState(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task Should_HintTheFreshOpening_When_SessionMissing_AndNoHintConversation()
        {
            // Sessions persist only after the first PLAYED turn — a brand-new player at turn zero has
            // no saved session but IS looking at the game intro. With no prior hint conversation this is
            // unambiguous: proceed against the fresh engine state rather than saying "start a game".
            _mockSessionRepository.Setup(r => r.GetSessionState("new-session", "planetfall_session"))
                .ReturnsAsync((string?)null);
            _mockEngine.Setup(e => e.Context).Returns(new Mock<IContext>().Object);

            var result = await _controller.Hint(new HintApiRequest("new-session", "what should I do first?"));

            // It went into the hint flow (the unconfigured mock LLM degrades to the "unavailable"
            // fallback) — the point is it did NOT refuse with the no-game message...
            result.Text.Should().NotContain("can't find a game");
            // ...never tried to restore a nonexistent session, and persisted nothing.
            _mockEngine.Verify(e => e.RestoreGame(It.IsAny<string>()), Times.Never);
            _mockSessionRepository.Verify(
                r => r.WriteSessionState(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task Should_NotWriteSession_When_HintRequested()
        {
            _mockSessionRepository.Setup(r => r.GetSessionState("live-session", "planetfall_session"))
                .ReturnsAsync("c2F2ZWQ="); // a (mock) saved game blob
            _mockEngine.Setup(e => e.Context).Returns(new Mock<IContext>().Object);

            await _controller.Hint(new HintApiRequest("live-session", "what do I do?"));

            // The endpoint's headline guarantee: asking for a hint consumes no turn and persists nothing.
            _mockSessionRepository.Verify(
                r => r.WriteSessionState(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
    }
}