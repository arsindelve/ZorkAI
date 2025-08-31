using Lambda.Controllers;
using Microsoft.Extensions.Logging;
using Model.AIGeneration;
using Model.AIGeneration.Requests;
using Model.Interface;
using Model.Web;

namespace UnitTests.Lambda;

[TestFixture]
public class ZorkOneControllerTests
{
    [SetUp]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<ZorkOneController>>();
        _mockEngine = new Mock<IGameEngine>();
        _mockSessionRepository = new Mock<ISessionRepository>();
        _mockSavedGameRepository = new Mock<ISavedGameRepository>();
        _mockGenerationClient = new Mock<IGenerationClient>();

        _mockEngine.Setup(e => e.GenerationClient).Returns(_mockGenerationClient.Object);
        _mockEngine.Setup(e => e.LocationDescription).Returns("Test Location");
        _mockEngine.Setup(e => e.IntroText).Returns("Welcome to Zork!");
        _mockEngine.Setup(e => e.Moves).Returns(1);
        _mockEngine.Setup(e => e.SaveGame()).Returns("serialized game state");

        _controller = new ZorkOneController(
            _mockLogger.Object,
            _mockEngine.Object,
            _mockSessionRepository.Object,
            _mockSavedGameRepository.Object);
    }

    private Mock<ILogger<ZorkOneController>> _mockLogger;
    private Mock<IGameEngine> _mockEngine;
    private Mock<ISessionRepository> _mockSessionRepository;
    private Mock<ISavedGameRepository> _mockSavedGameRepository;
    private Mock<IGenerationClient> _mockGenerationClient;
    private ZorkOneController _controller;

    [TestFixture]
    public class IndexPostMethod : ZorkOneControllerTests
    {
        [Test]
        public async Task Should_InitializeEngine_When_Called()
        {
            // Arrange
            var request = new GameRequest("look", "test-session");
            _mockEngine.Setup(e => e.GetResponse("look")).ReturnsAsync("You are in a room.");
            _mockSessionRepository.Setup(r => r.GetSessionState("test-session", "zork1_session"))
                .ReturnsAsync((string?)null);
            _mockSessionRepository.Setup(r =>
                    r.WriteSessionState(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            _mockSessionRepository.Setup(r => r.WriteSessionStep(It.IsAny<string>(), It.IsAny<int>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
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
            _mockSessionRepository.Setup(r => r.GetSessionState("test-session", "zork1_session"))
                .ReturnsAsync((string?)null);
            _mockSessionRepository.Setup(r =>
                    r.WriteSessionState(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            _mockSessionRepository.Setup(r => r.WriteSessionStep(It.IsAny<string>(), It.IsAny<int>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
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
            _mockSessionRepository.Setup(r => r.GetSessionState("test-session", "zork1_session"))
                .ReturnsAsync(savedData);

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
            _mockSessionRepository.Setup(r => r.GetSessionState("test-session", "zork1_session"))
                .ReturnsAsync((string?)null);
            _mockSessionRepository.Setup(r =>
                    r.WriteSessionState(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            _mockSessionRepository.Setup(r => r.WriteSessionStep(It.IsAny<string>(), It.IsAny<int>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            await _controller.Index(request);

            // Assert
            _mockSessionRepository.Verify(r => r.WriteSessionState(
                "test-session",
                It.IsAny<string>(),
                "zork1_session"), Times.Once);
            _mockSessionRepository.Verify(r => r.WriteSessionStep(
                "test-session",
                1,
                "inventory",
                "You are empty-handed.",
                "zork1_session_steps"), Times.Once);
        }

        [Test]
        public async Task Should_LogInputAndResponse_When_Processing()
        {
            // Arrange
            var request = new GameRequest("examine lamp", "test-session");
            _mockEngine.Setup(e => e.GetResponse("examine lamp")).ReturnsAsync("It's a brass lantern.");
            _mockSessionRepository.Setup(r => r.GetSessionState("test-session", "zork1_session"))
                .ReturnsAsync((string?)null);
            _mockSessionRepository.Setup(r =>
                    r.WriteSessionState(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            _mockSessionRepository.Setup(r => r.WriteSessionStep(It.IsAny<string>(), It.IsAny<int>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            await _controller.Index(request);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Request: examine lamp")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Response: It's a brass lantern.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }

    [TestFixture]
    public class RestoreGameMethod : ZorkOneControllerTests
    {
        [Test]
        public async Task Should_InitializeEngine_When_Called()
        {
            // Arrange
            var request = new RestoreGameRequest("session-id", "client-id", "save-id");
            _mockSavedGameRepository.Setup(r => r.GetSavedGame("save-id", "client-id", "zork1_savegame"))
                .ReturnsAsync("dGVzdCBzYXZlZCBkYXRh"); // Base64 encoded data
            _mockEngine.Setup(e => e.GetResponse("look")).ReturnsAsync("You are here.");
            _mockGenerationClient.Setup(g => g.GenerateNarration(It.IsAny<Request>(), It.IsAny<string>()))
                .ReturnsAsync("Game restored successfully.");
            _mockSessionRepository.Setup(r =>
                    r.WriteSessionState(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            _mockSessionRepository.Setup(r => r.WriteSessionStep(It.IsAny<string>(), It.IsAny<int>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
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
            _mockSavedGameRepository.Setup(r => r.GetSavedGame("invalid-id", "client-id", "zork1_savegame"))
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
            _mockSavedGameRepository.Setup(r => r.GetSavedGame("save-id", "client-id", "zork1_savegame"))
                .ReturnsAsync(savedData);
            _mockEngine.Setup(e => e.GetResponse("look")).ReturnsAsync("You are in a restored location.");
            _mockGenerationClient.Setup(g => g.GenerateNarration(It.IsAny<Request>(), It.IsAny<string>()))
                .ReturnsAsync("Welcome back!");
            _mockSessionRepository.Setup(r =>
                    r.WriteSessionState(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            _mockSessionRepository.Setup(r => r.WriteSessionStep(It.IsAny<string>(), It.IsAny<int>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.RestoreGame(request);

            // Assert
            _mockEngine.Verify(e => e.RestoreGame("test saved data"), Times.Once);
            _mockEngine.Verify(e => e.GetResponse("look"), Times.Once);
            result.Should().NotBeNull();
            result.Response.Should().Contain("Welcome back!");
            result.Response.Should().Contain("You are in a restored location.");
        }
    }

    [TestFixture]
    public class SaveGameMethod : ZorkOneControllerTests
    {
        [Test]
        public async Task Should_InitializeEngine_When_Called()
        {
            // Arrange
            var request = new SaveGameRequest("session-id", "client-id", "My Save", "save-id");
            _mockSessionRepository.Setup(r => r.GetSessionState("session-id", "zork1_session"))
                .ReturnsAsync("dGVzdCBzYXZlZCBkYXRh");
            _mockSessionRepository.Setup(r => r.GetSessionStepsAsText("session-id", "zork1_session_steps"))
                .ReturnsAsync("game history");
            _mockEngine.Setup(e => e.SaveGame()).Returns("game state");
            _mockGenerationClient.Setup(g => g.GenerateNarration(It.IsAny<Request>(), It.IsAny<string>()))
                .ReturnsAsync("Game saved successfully.");
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
            var request = new SaveGameRequest("save-id", "client-id", "My Save", "session-id");
            _mockSessionRepository.Setup(r => r.GetSessionState("session-id", "zork1_session"))
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
            _mockSessionRepository.Setup(r => r.GetSessionState("session-id", "zork1_session"))
                .ReturnsAsync(sessionData);
            _mockSessionRepository.Setup(r => r.GetSessionStepsAsText("session-id", "zork1_session_steps"))
                .ReturnsAsync("game history");
            _mockEngine.Setup(e => e.SaveGame()).Returns("current game state");
            _mockGenerationClient.Setup(g => g.GenerateNarration(It.IsAny<Request>(), It.IsAny<string>()))
                .ReturnsAsync("Game saved successfully.");
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
                "zork1_savegame"), Times.Once);
            result.Should().Be("Game saved successfully.");
        }
    }

    [TestFixture]
    public class GetAllSavedGamesMethod : ZorkOneControllerTests
    {
        [Test]
        public async Task Should_InitializeEngine_When_Called()
        {
            // Arrange
            var sessionId = "test-session";
            _mockSavedGameRepository.Setup(r => r.GetSavedGames(sessionId, "zork1_savegame"))
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
            _mockSavedGameRepository.Setup(r => r.GetSavedGames(sessionId, "zork1_savegame"))
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
    public class IndexGetMethod : ZorkOneControllerTests
    {
        [Test]
        public async Task Should_InitializeEngine_When_Called()
        {
            // Arrange
            var sessionId = "test-session";
            _mockSessionRepository.Setup(r => r.GetSessionState(sessionId, "zork1_session"))
                .ReturnsAsync((string?)null);
            _mockSessionRepository.Setup(r => r.GetSessionStepsAsText(sessionId, "zork1_session_steps"))
                .ReturnsAsync("");

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
            _mockSessionRepository.Setup(r => r.GetSessionState(sessionId, "zork1_session"))
                .ReturnsAsync((string?)null);
            _mockSessionRepository.Setup(r => r.GetSessionStepsAsText(sessionId, "zork1_session_steps"))
                .ReturnsAsync("");

            // Act
            var result = await _controller.Index(sessionId);

            // Assert
            result.Should().NotBeNull();
            result.Response.Should().Be("Welcome to Zork!");
        }

        [Test]
        public async Task Should_RestoreSessionAndReturnHistory_When_SessionExists()
        {
            // Arrange
            var sessionId = "existing-session";
            var savedData = "dGVzdCBzYXZlZCBkYXRh"; // Base64 encoded data
            var history = "Previous game history";
            _mockSessionRepository.Setup(r => r.GetSessionState(sessionId, "zork1_session"))
                .ReturnsAsync(savedData);
            _mockSessionRepository.Setup(r => r.GetSessionStepsAsText(sessionId, "zork1_session_steps"))
                .ReturnsAsync(history);

            // Act
            var result = await _controller.Index(sessionId);

            // Assert
            _mockEngine.Verify(e => e.RestoreGame("test saved data"), Times.Once);
            result.Should().NotBeNull();
            result.Response.Should().Be(history);
        }

        [Test]
        public async Task Should_ReturnLookResponse_When_SessionExistsButNoHistory()
        {
            // Arrange
            var sessionId = "existing-session";
            var savedData = "dGVzdCBzYXZlZCBkYXRh"; // Base64 encoded data
            _mockSessionRepository.Setup(r => r.GetSessionState(sessionId, "zork1_session"))
                .ReturnsAsync(savedData);
            _mockSessionRepository.Setup(r => r.GetSessionStepsAsText(sessionId, "zork1_session_steps"))
                .ReturnsAsync("");
            _mockEngine.Setup(e => e.GetResponse("look")).ReturnsAsync("You are in a room.");

            // Act
            var result = await _controller.Index(sessionId);

            // Assert
            _mockEngine.Verify(e => e.RestoreGame("test saved data"), Times.Once);
            _mockEngine.Verify(e => e.GetResponse("look"), Times.Once);
            result.Should().NotBeNull();
            result.Response.Should().Be("You are in a room.");
        }
    }
}