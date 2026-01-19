using System.Text;
using GameEngine.StaticCommand;
using GameEngine.StaticCommand.Implementation;
using Model.AIGeneration;
using Model.AIGeneration.Requests;
using Model.Interface;
using Model.Location;

namespace UnitTests.GlobalCommands;

[TestFixture]
public class RestoreProcessorTests
{
    [SetUp]
    public void SetUp()
    {
        _mockReader = new Mock<ISaveGameReader>();
        _mockClient = new Mock<IGenerationClient>();
        _mockContext = new Mock<IContext>();
        _mockEngine = new Mock<IGameEngine>();
        _mockLocation = new Mock<ILocation>();

        _mockLocation.Setup(l => l.GetDescriptionForGeneration(It.IsAny<IContext>()))
            .Returns("Test location description");
        _mockContext.Setup(c => c.CurrentLocation).Returns(_mockLocation.Object);
        _mockContext.Setup(c => c.Game.DefaultSaveGameName).Returns("default.sav");
    }

    private Mock<ISaveGameReader> _mockReader = null!;
    private Mock<IGenerationClient> _mockClient = null!;
    private Mock<IContext> _mockContext = null!;
    private Mock<IGameEngine> _mockEngine = null!;
    private Mock<ILocation> _mockLocation = null!;

    [TestFixture]
    public class ConstructorTests : RestoreProcessorTests
    {
        [Test]
        public void Should_CreateInstance_WithDefaultConstructor()
        {
            // Act
            var processor = new RestoreProcessor();

            // Assert
            processor.Should().NotBeNull();
        }

        [Test]
        public void Should_CreateInstance_WithInjectedReader()
        {
            // Act
            var processor = new RestoreProcessor(_mockReader.Object);

            // Assert
            processor.Should().NotBeNull();
        }
    }

    [TestFixture]
    public class ProcessTests : RestoreProcessorTests
    {
        [Test]
        public async Task Should_ReturnRestoreTag_When_RuntimeIsWeb()
        {
            // Arrange
            var processor = new RestoreProcessor(_mockReader.Object);

            // Act
            var result = await processor.Process("restore", _mockContext.Object, _mockClient.Object, Runtime.Web);

            // Assert
            result.Should().Be("<Restore>");
        }

        [Test]
        public async Task Should_PromptForFilename_When_FirstCall()
        {
            // Arrange
            var processor = new RestoreProcessor(_mockReader.Object);
            _mockClient.Setup(c => c.GenerateNarration(It.IsAny<BeforeRestoreGameRequest>(), It.IsAny<string>()))
                .ReturnsAsync("Time flows like water...");

            // Act
            var result = await processor.Process("restore", _mockContext.Object, _mockClient.Object, Runtime.Unknown);

            // Assert
            result.Should().Contain("Time flows like water");
            result.Should().Contain("Enter a file name");
            result.Should().Contain("default.sav");
            processor.Completed.Should().BeFalse();
        }

        [Test]
        public async Task Should_ShowLastSaveGameName_When_Available()
        {
            // Arrange
            var processor = new RestoreProcessor(_mockReader.Object);
            _mockContext.Setup(c => c.LastSaveGameName).Returns("mysave.sav");
            _mockClient.Setup(c => c.GenerateNarration(It.IsAny<BeforeRestoreGameRequest>(), It.IsAny<string>()))
                .ReturnsAsync("Memories return...");

            // Act
            var result = await processor.Process("restore", _mockContext.Object, _mockClient.Object, Runtime.Unknown);

            // Assert
            result.Should().Contain("mysave.sav");
            result.Should().NotContain("default.sav");
        }

        [Test]
        public async Task Should_ReturnEmpty_When_NoEngineAndPrompted()
        {
            // Arrange
            var processor = new RestoreProcessor(_mockReader.Object);
            _mockClient.Setup(c => c.GenerateNarration(It.IsAny<BeforeRestoreGameRequest>(), It.IsAny<string>()))
                .ReturnsAsync("Prompt");
            _mockContext.Setup(c => c.Engine).Returns((IGameEngine?)null);

            // First call to set _havePromptedForFilename
            await processor.Process("restore", _mockContext.Object, _mockClient.Object, Runtime.Unknown);

            // Act
            var result = await processor.Process("filename.sav", _mockContext.Object, _mockClient.Object, Runtime.Unknown);

            // Assert
            result.Should().Be(string.Empty);
        }

        [Test]
        public async Task Should_ContinueProcessingBeFalse()
        {
            // Arrange
            var processor = new RestoreProcessor(_mockReader.Object);

            // Assert
            ((IStatefulProcessor)processor).ContinueProcessing.Should().BeFalse();
        }
    }

    [TestFixture]
    public class AttemptRestoreTests : RestoreProcessorTests
    {
        [Test]
        public async Task Should_RestoreGame_With_UserProvidedFilename()
        {
            // Arrange
            var processor = new RestoreProcessor(_mockReader.Object);
            var gameData = "game state data";
            var encodedData = Convert.ToBase64String(Encoding.UTF8.GetBytes(gameData));

            _mockClient.Setup(c => c.GenerateNarration(It.IsAny<BeforeRestoreGameRequest>(), It.IsAny<string>()))
                .ReturnsAsync("Prompt");
            _mockClient.Setup(c => c.GenerateNarration(It.IsAny<AfterRestoreGameRequest>(), It.IsAny<string>()))
                .ReturnsAsync("Welcome back!");

            _mockReader.Setup(r => r.Read("custom.sav")).ReturnsAsync(encodedData);
            _mockContext.Setup(c => c.Engine).Returns(_mockEngine.Object);
            _mockContext.SetupProperty(c => c.LastSaveGameName);
            _mockEngine.Setup(e => e.RestoreGame(gameData)).Returns(_mockContext.Object);

            // First call prompts for filename
            await processor.Process("restore", _mockContext.Object, _mockClient.Object, Runtime.Unknown);

            // Act - second call with filename
            var result = await processor.Process("custom.sav", _mockContext.Object, _mockClient.Object, Runtime.Unknown);

            // Assert
            result.Should().Contain("Welcome back!");
            processor.Completed.Should().BeTrue();
            _mockContext.Object.LastSaveGameName.Should().Be("custom.sav");
        }

        [Test]
        public async Task Should_UseDefaultFilename_When_InputIsEmpty()
        {
            // Arrange
            var processor = new RestoreProcessor(_mockReader.Object);
            var gameData = "saved game";
            var encodedData = Convert.ToBase64String(Encoding.UTF8.GetBytes(gameData));

            _mockClient.Setup(c => c.GenerateNarration(It.IsAny<BeforeRestoreGameRequest>(), It.IsAny<string>()))
                .ReturnsAsync("Prompt");
            _mockClient.Setup(c => c.GenerateNarration(It.IsAny<AfterRestoreGameRequest>(), It.IsAny<string>()))
                .ReturnsAsync("Restored!");

            _mockReader.Setup(r => r.Read("default.sav")).ReturnsAsync(encodedData);
            _mockContext.Setup(c => c.Engine).Returns(_mockEngine.Object);
            _mockContext.SetupProperty(c => c.LastSaveGameName);
            _mockEngine.Setup(e => e.RestoreGame(gameData)).Returns(_mockContext.Object);

            await processor.Process("restore", _mockContext.Object, _mockClient.Object, Runtime.Unknown);

            // Act - empty input uses default
            var result = await processor.Process("", _mockContext.Object, _mockClient.Object, Runtime.Unknown);

            // Assert
            result.Should().Contain("Restored!");
            _mockReader.Verify(r => r.Read("default.sav"), Times.Once);
        }

        [Test]
        public async Task Should_UseLastSaveGameName_When_InputIsEmptyAndLastSaveExists()
        {
            // Arrange
            var processor = new RestoreProcessor(_mockReader.Object);
            var gameData = "saved game";
            var encodedData = Convert.ToBase64String(Encoding.UTF8.GetBytes(gameData));

            _mockClient.Setup(c => c.GenerateNarration(It.IsAny<BeforeRestoreGameRequest>(), It.IsAny<string>()))
                .ReturnsAsync("Prompt");
            _mockClient.Setup(c => c.GenerateNarration(It.IsAny<AfterRestoreGameRequest>(), It.IsAny<string>()))
                .ReturnsAsync("Restored!");

            _mockContext.Setup(c => c.LastSaveGameName).Returns("lastsave.sav");
            _mockReader.Setup(r => r.Read("lastsave.sav")).ReturnsAsync(encodedData);
            _mockContext.Setup(c => c.Engine).Returns(_mockEngine.Object);
            _mockEngine.Setup(e => e.RestoreGame(gameData)).Returns(_mockContext.Object);

            await processor.Process("restore", _mockContext.Object, _mockClient.Object, Runtime.Unknown);

            // Act
            var result = await processor.Process("", _mockContext.Object, _mockClient.Object, Runtime.Unknown);

            // Assert
            _mockReader.Verify(r => r.Read("lastsave.sav"), Times.Once);
        }

        [Test]
        public async Task Should_HandleFileNotFoundException()
        {
            // Arrange
            var processor = new RestoreProcessor(_mockReader.Object);

            _mockClient.Setup(c => c.GenerateNarration(It.IsAny<BeforeRestoreGameRequest>(), It.IsAny<string>()))
                .ReturnsAsync("Prompt");
            _mockClient.Setup(c => c.GenerateNarration(It.IsAny<RestoreFailedFileNotFoundGameRequest>(), It.IsAny<string>()))
                .ReturnsAsync("File not found!");

            _mockReader.Setup(r => r.Read(It.IsAny<string>())).ThrowsAsync(new FileNotFoundException());
            _mockContext.Setup(c => c.Engine).Returns(_mockEngine.Object);
            _mockContext.SetupProperty(c => c.LastSaveGameName);

            await processor.Process("restore", _mockContext.Object, _mockClient.Object, Runtime.Unknown);

            // Act
            var result = await processor.Process("nonexistent.sav", _mockContext.Object, _mockClient.Object, Runtime.Unknown);

            // Assert
            result.Should().Contain("File not found!");
            processor.Completed.Should().BeTrue();
        }

        [Test]
        public async Task Should_HandleGenericException()
        {
            // Arrange
            var processor = new RestoreProcessor(_mockReader.Object);

            _mockClient.Setup(c => c.GenerateNarration(It.IsAny<BeforeRestoreGameRequest>(), It.IsAny<string>()))
                .ReturnsAsync("Prompt");
            _mockClient.Setup(c => c.GenerateNarration(It.IsAny<RestoreFailedUnknownReasonGameRequest>(), It.IsAny<string>()))
                .ReturnsAsync("Unknown error!");

            _mockReader.Setup(r => r.Read(It.IsAny<string>())).ThrowsAsync(new InvalidOperationException("Corrupted"));
            _mockContext.Setup(c => c.Engine).Returns(_mockEngine.Object);
            _mockContext.SetupProperty(c => c.LastSaveGameName);

            await processor.Process("restore", _mockContext.Object, _mockClient.Object, Runtime.Unknown);

            // Act
            var result = await processor.Process("corrupt.sav", _mockContext.Object, _mockClient.Object, Runtime.Unknown);

            // Assert
            result.Should().Contain("Unknown error!");
            processor.Completed.Should().BeTrue();
        }

        [Test]
        public async Task Should_IncludeLocationDescription_InResponse()
        {
            // Arrange
            var processor = new RestoreProcessor(_mockReader.Object);
            var gameData = "saved game";
            var encodedData = Convert.ToBase64String(Encoding.UTF8.GetBytes(gameData));

            _mockClient.Setup(c => c.GenerateNarration(It.IsAny<BeforeRestoreGameRequest>(), It.IsAny<string>()))
                .ReturnsAsync("Prompt");
            _mockClient.Setup(c => c.GenerateNarration(It.IsAny<AfterRestoreGameRequest>(), It.IsAny<string>()))
                .ReturnsAsync("You remember...");

            _mockReader.Setup(r => r.Read(It.IsAny<string>())).ReturnsAsync(encodedData);
            _mockContext.Setup(c => c.Engine).Returns(_mockEngine.Object);
            _mockContext.SetupProperty(c => c.LastSaveGameName);
            _mockEngine.Setup(e => e.RestoreGame(gameData)).Returns(_mockContext.Object);

            await processor.Process("restore", _mockContext.Object, _mockClient.Object, Runtime.Unknown);

            // Act
            var result = await processor.Process("", _mockContext.Object, _mockClient.Object, Runtime.Unknown);

            // Assert
            result.Should().Contain("You remember...");
            result.Should().Contain("Test location description");
        }
    }
}