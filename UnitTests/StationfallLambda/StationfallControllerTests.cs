using Stationfall_Lambda.Controllers;
using Microsoft.Extensions.Logging;
using Model.AIGeneration;
using Model.AIGeneration.Requests;
using Model.Interface;
using Model.Web;

namespace UnitTests.StationfallLambda;

[TestFixture]
public class StationfallControllerTests
{
    [SetUp]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<StationfallController>>();
        _mockEngine = new Mock<IGameEngine>();
        _mockSessionRepository = new Mock<ISessionRepository>();
        _mockSavedGameRepository = new Mock<ISavedGameRepository>();
        _mockGenerationClient = new Mock<IGenerationClient>();

        _mockEngine.Setup(e => e.GenerationClient).Returns(_mockGenerationClient.Object);
        _mockEngine.Setup(e => e.LocationDescription).Returns("Test Location");
        _mockEngine.Setup(e => e.IntroText).Returns("Welcome to Stationfall!");
        _mockEngine.Setup(e => e.Moves).Returns(1);
        _mockEngine.Setup(e => e.SaveGame()).Returns("serialized game state");

        _controller = new StationfallController(
            _mockLogger.Object,
            _mockEngine.Object,
            _mockSessionRepository.Object,
            _mockSavedGameRepository.Object);
    }

    private Mock<ILogger<StationfallController>> _mockLogger;
    private Mock<IGameEngine> _mockEngine;
    private Mock<ISessionRepository> _mockSessionRepository;
    private Mock<ISavedGameRepository> _mockSavedGameRepository;
    private Mock<IGenerationClient> _mockGenerationClient;
    private StationfallController _controller;

    [TestFixture]
    public class IndexPostMethod : StationfallControllerTests
    {
        [Test]
        public async Task Should_InitializeEngine_When_Called()
        {
            // Arrange
            var request = new GameRequest("look", "test-session");
            _mockEngine.Setup(e => e.GetResponse("look")).ReturnsAsync("You are in a room.");
            _mockSessionRepository.Setup(r => r.GetSessionState("test-session", "stationfall_session"))
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
            _mockSessionRepository.Setup(r => r.GetSessionState("test-session", "stationfall_session"))
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
            _mockSessionRepository.Setup(r => r.GetSessionState("test-session", "stationfall_session"))
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
            _mockSessionRepository.Setup(r => r.GetSessionState("test-session", "stationfall_session"))
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
                "stationfall_session"), Times.Once);
        }

        [Test]
        public async Task Should_LogInputAndResponse_When_Processing()
        {
            // Arrange
            var request = new GameRequest("examine chronometer", "test-session");
            _mockEngine.Setup(e => e.GetResponse("examine chronometer")).ReturnsAsync("The chronometer reads 9:42.");
            _mockSessionRepository.Setup(r => r.GetSessionState("test-session", "stationfall_session"))
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
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Request: examine chronometer")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Response: The chronometer reads 9:42.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }

    [TestFixture]
    public class RestoreGameMethod : StationfallControllerTests
    {
        [Test]
        public async Task Should_InitializeEngine_When_Called()
        {
            // Arrange
            var request = new RestoreGameRequest("session-id", "client-id", "save-id");
            _mockSavedGameRepository.Setup(r => r.GetSavedGame("save-id", "client-id", "stationfall_savegame"))
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
            _mockSavedGameRepository.Setup(r => r.GetSavedGame("invalid-id", "client-id", "stationfall_savegame"))
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
            _mockSavedGameRepository.Setup(r => r.GetSavedGame("save-id", "client-id", "stationfall_savegame"))
                .ReturnsAsync(savedData);
            _mockEngine.Setup(e => e.GetResponse("look")).ReturnsAsync("You are in a restored location.");
            _mockGenerationClient.Setup(g => g.GenerateNarration(It.IsAny<Request>(), It.IsAny<string>()))
                .ReturnsAsync("Welcome back to Stationfall!");
            _mockSessionRepository.Setup(r =>
                    r.WriteSessionState(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.RestoreGame(request);

            // Assert
            _mockEngine.Verify(e => e.RestoreGame("test saved data"), Times.Once);
            _mockEngine.Verify(e => e.GetResponse("look"), Times.Once);
            result.Should().NotBeNull();
            result.Response.Should().Contain("Welcome back to Stationfall!");
            result.Response.Should().Contain("You are in a restored location.");
        }
    }

    [TestFixture]
    public class SaveGameMethod : StationfallControllerTests
    {
        [Test]
        public async Task Should_InitializeEngine_When_Called()
        {
            // Arrange
            var request = new SaveGameRequest("session-id", "client-id", "My Save", "save-id");
            _mockSessionRepository.Setup(r => r.GetSessionState("session-id", "stationfall_session"))
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
            _mockSessionRepository.Setup(r => r.GetSessionState("session-id", "stationfall_session"))
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
            _mockSessionRepository.Setup(r => r.GetSessionState("session-id", "stationfall_session"))
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
                "stationfall_savegame"), Times.Once);
            _mockEngine.Verify(e => e.GenerateSaveGameNarration(), Times.Once);
            result.Should().Be("Game saved successfully.");
        }
    }

    [TestFixture]
    public class GetAllSavedGamesMethod : StationfallControllerTests
    {
        [Test]
        [Ignore("GetAllSavedGames method does not initialize engine in Stationfall controller")]
        public async Task Should_InitializeEngine_When_Called()
        {
            // Arrange
            var sessionId = "test-session";
            _mockSavedGameRepository.Setup(r => r.GetSavedGames(sessionId, "stationfall_savegame"))
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
            _mockSavedGameRepository.Setup(r => r.GetSavedGames(sessionId, "stationfall_savegame"))
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
    public class IndexGetMethod : StationfallControllerTests
    {
        [Test]
        public async Task Should_InitializeEngine_When_Called()
        {
            // Arrange
            var sessionId = "test-session";
            _mockSessionRepository.Setup(r => r.GetSessionState(sessionId, "stationfall_session"))
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
            _mockSessionRepository.Setup(r => r.GetSessionState(sessionId, "stationfall_session"))
                .ReturnsAsync((string?)null);

            // Act
            var result = await _controller.Index(sessionId);

            // Assert
            result.Should().NotBeNull();
            result.Response.Should().Be("Welcome to Stationfall!");
        }

        [Test]
        public async Task Should_RestoreSessionAndReturnLookResponse_When_SessionExists()
        {
            // Arrange
            var sessionId = "existing-session";
            var savedData = "dGVzdCBzYXZlZCBkYXRh"; // Base64 encoded data
            _mockSessionRepository.Setup(r => r.GetSessionState(sessionId, "stationfall_session"))
                .ReturnsAsync(savedData);
            _mockEngine.Setup(e => e.GetResponse("look")).ReturnsAsync("You are on Deck Twelve.");

            // Act
            var result = await _controller.Index(sessionId);

            // Assert
            _mockEngine.Verify(e => e.RestoreGame("test saved data"), Times.Once);
            _mockEngine.Verify(e => e.GetResponse("look"), Times.Once);
            result.Should().NotBeNull();
            result.Response.Should().Be("You are on Deck Twelve.");
        }
    }

    [TestFixture]
    public class SessionTableName : StationfallControllerTests
    {
        [Test]
        public async Task Should_MatchTheTableNameTheGameDeclares()
        {
            // The controller hard-codes the session table (as every game's controller does) while the
            // engine reads StationfallGame.SessionTableName. If the two ever drift, sessions are written
            // to one table and looked up in another - the game silently restarts on every turn in prod
            // while every mock-based test still passes. Pin them together.
            var declared = new Stationfall.StationfallGame().SessionTableName;
            _mockEngine.Setup(e => e.GetResponse("look")).ReturnsAsync("You are on Deck Twelve.");
            _mockSessionRepository.Setup(r => r.GetSessionState("test-session", declared))
                .ReturnsAsync((string?)null);
            _mockSessionRepository.Setup(r =>
                    r.WriteSessionState(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            await _controller.Index(new GameRequest("look", "test-session"));

            _mockSessionRepository.Verify(
                r => r.WriteSessionState("test-session", It.IsAny<string>(), declared), Times.Once);
        }
    }
}
