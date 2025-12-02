using CloudWatch;
using CloudWatch.Model;
using GameEngine;
using Microsoft.Extensions.Logging;
using Model.AIParsing;
using Model.Intent;
using Model.Interface;
using Model.Movement;

namespace UnitTests;

[TestFixture]
public class IntentParserTests
{
    [SetUp]
    public void SetUp()
    {
        _mockParser = new Mock<IAIParser>();
        _mockGameSpecificCommandFactory = new Mock<IGlobalCommandFactory>();
        _mockLogger = new Mock<ICloudWatchLogger<GenerationLog>>();

        _intentParser = new IntentParser(_mockParser.Object, _mockGameSpecificCommandFactory.Object);
        _intentParser.Logger = _mockLogger.Object;
        _intentParser.TurnCorrelationId = Guid.NewGuid();
    }

    private Mock<IAIParser> _mockParser;
    private Mock<IGlobalCommandFactory> _mockGameSpecificCommandFactory;
    private Mock<ICloudWatchLogger<GenerationLog>> _mockLogger;
    private IntentParser _intentParser;

    [TestFixture]
    public class DetermineSystemIntentTypeMethod : IntentParserTests
    {
        [Test]
        public void Should_ReturnSystemCommandIntent_When_InputMatchesSystemCommand()
        {
            // Arrange - Test with actual GlobalCommandFactory since GetSystemCommands is not virtual
            var parser = new IntentParser(_mockParser.Object, _mockGameSpecificCommandFactory.Object);

            // Act - "restart" is a known system command in the default factory
            var result = parser.DetermineSystemIntentType("restart");

            // Assert
            result.Should().BeOfType<SystemCommandIntent>();
            ((SystemCommandIntent)result!).Command.Should().NotBeNull();
        }

        [Test]
        public void Should_ReturnNull_When_InputDoesNotMatchSystemCommand()
        {
            // Act
            var result = _intentParser.DetermineSystemIntentType("unknown command");

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public void Should_ReturnNull_When_InputIsNull()
        {
            // Act
            var result = _intentParser.DetermineSystemIntentType(null);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public void Should_ReturnNull_When_InputIsEmpty()
        {
            // Act
            var result = _intentParser.DetermineSystemIntentType("");

            // Assert
            result.Should().BeNull();
        }
    }

    [TestFixture]
    public class DetermineGlobalIntentTypeMethod : IntentParserTests
    {
        [Test]
        public void Should_ReturnMoveIntent_When_InputIsDirection()
        {
            // Act
            var result = _intentParser.DetermineGlobalIntentType("north");

            // Assert
            result.Should().BeOfType<MoveIntent>();
            ((MoveIntent)result!).Direction.Should().Be(Direction.N);
        }

        [Test]
        public void Should_ReturnMoveIntent_When_InputIsDirectionAbbreviation()
        {
            // Act
            var result = _intentParser.DetermineGlobalIntentType("n");

            // Assert
            result.Should().BeOfType<MoveIntent>();
            ((MoveIntent)result!).Direction.Should().Be(Direction.N);
        }

        [Test]
        public void Should_ReturnGlobalCommandIntent_When_InputMatchesDefaultGlobalCommand()
        {
            // Act
            var result = _intentParser.DetermineGlobalIntentType("inventory");

            // Assert
            result.Should().BeOfType<GlobalCommandIntent>();
            ((GlobalCommandIntent)result!).Command.Should().NotBeNull();
        }

        [Test]
        public void Should_ReturnGlobalCommandIntent_When_InputMatchesGameSpecificCommand()
        {
            // Arrange
            var gameSpecificCommand = Mock.Of<IGlobalCommand>();
            _mockGameSpecificCommandFactory.Setup(f => f.GetGlobalCommands("diagnose"))
                .Returns(gameSpecificCommand);

            // Act
            var result = _intentParser.DetermineGlobalIntentType("diagnose");

            // Assert
            result.Should().BeOfType<GlobalCommandIntent>();
            ((GlobalCommandIntent)result!).Command.Should().Be(gameSpecificCommand);
        }

        [Test]
        public void Should_PrioritizeDirection_Over_GlobalCommands()
        {
            // Arrange - Even if "north" could be a global command, direction should win
            var globalCommand = Mock.Of<IGlobalCommand>();
            _mockGameSpecificCommandFactory.Setup(f => f.GetGlobalCommands("north"))
                .Returns(globalCommand);

            // Act
            var result = _intentParser.DetermineGlobalIntentType("north");

            // Assert
            result.Should().BeOfType<MoveIntent>();
            ((MoveIntent)result!).Direction.Should().Be(Direction.N);
        }

        [Test]
        public void Should_PrioritizeDefaultGlobalCommand_Over_GameSpecificCommand()
        {
            // Arrange
            var gameSpecificCommand = Mock.Of<IGlobalCommand>();
            _mockGameSpecificCommandFactory.Setup(f => f.GetGlobalCommands("inventory"))
                .Returns(gameSpecificCommand);

            // Act
            var result = _intentParser.DetermineGlobalIntentType("inventory");

            // Assert
            result.Should().BeOfType<GlobalCommandIntent>();
            // Should be the default inventory processor, not the game-specific one
            ((GlobalCommandIntent)result!).Command.Should().NotBe(gameSpecificCommand);
        }

        [Test]
        public void Should_ReturnNull_When_InputMatchesNoCommands()
        {
            // Act
            var result = _intentParser.DetermineGlobalIntentType("unknown command");

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public void Should_ReturnNull_When_InputIsNull()
        {
            // Act
            var result = _intentParser.DetermineGlobalIntentType(null);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public void Should_ReturnNull_When_InputIsEmpty()
        {
            // Act
            var result = _intentParser.DetermineGlobalIntentType("");

            // Assert
            result.Should().BeNull();
        }
    }

    [TestFixture]
    public class DetermineComplexIntentTypeMethod : IntentParserTests
    {
        [Test]
        public async Task Should_CallAIParser_And_ReturnResponse()
        {
            // Arrange
            var expectedIntent = new SimpleIntent { Verb = "take", Noun = "sword", Message = "Test intent" };
            _mockParser.Setup(p => p.AskTheAIParser("take sword", "You are in a room", "session123"))
                .ReturnsAsync(expectedIntent);

            // Act
            var result =
                await _intentParser.DetermineComplexIntentType("take sword", "You are in a room", "session123");

            // Assert
            result.Should().Be(expectedIntent);
            _mockParser.Verify(p => p.AskTheAIParser("take sword", "You are in a room", "session123"), Times.Once);
        }

        [Test]
        public async Task Should_LogGenerationEvent_When_InputIsNotEmpty()
        {
            // Arrange
            var intent = new SimpleIntent { Verb = "take", Noun = "sword", Message = "Test intent" };
            _mockParser.Setup(p => p.AskTheAIParser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(intent);
            _mockParser.Setup(p => p.LanguageModel).Returns("TestModel");

            // Act
            await _intentParser.DetermineComplexIntentType("take sword", "You are in a room", "session123");

            // Assert
            _mockLogger.Verify(l => l.WriteLogEvents(It.Is<GenerationLog>(log =>
                log.UserPrompt == "take sword" &&
                log.LanguageModel == "TestModel" &&
                log.Temperature == 0 &&
                log.SystemPrompt == string.Empty &&
                log.TurnCorrelationId == _intentParser.TurnCorrelationId.ToString()
            )), Times.Once);
        }

        [Test]
        public async Task Should_NotLog_When_InputIsEmpty()
        {
            // Arrange
            var intent = new SimpleIntent { Verb = "take", Noun = "sword", Message = "Test intent" };
            _mockParser.Setup(p => p.AskTheAIParser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(intent);

            // Act
            await _intentParser.DetermineComplexIntentType("", "You are in a room", "session123");

            // Assert
            _mockLogger.Verify(l => l.WriteLogEvents(It.IsAny<GenerationLog>()), Times.Never);
        }

        [Test]
        public async Task Should_NotLog_When_InputIsNull()
        {
            // Arrange
            var intent = new SimpleIntent { Verb = "take", Noun = "sword", Message = "Test intent" };
            _mockParser.Setup(p => p.AskTheAIParser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(intent);

            // Act
            await _intentParser.DetermineComplexIntentType(null, "You are in a room", "session123");

            // Assert
            _mockLogger.Verify(l => l.WriteLogEvents(It.IsAny<GenerationLog>()), Times.Never);
        }

    }

    [TestFixture]
    public class ConstructorTests : IntentParserTests
    {
        [Test]
        public void Should_InitializeWithProvidedDependencies()
        {
            // Arrange & Act
            var parser = new IntentParser(_mockParser.Object, _mockGameSpecificCommandFactory.Object);

            // Assert
            parser.Should().NotBeNull();
            parser.TurnCorrelationId.Should().BeNull();
            parser.Logger.Should().BeNull();
        }

        [Test]
        public void Should_InitializeWithDefaultParser_When_UsingGameSpecificConstructor()
        {
            // Arrange
            Environment.SetEnvironmentVariable("OPENAI_API_KEY", "test-key");

            // Act
            var parser = new IntentParser(_mockGameSpecificCommandFactory.Object);

            // Assert
            parser.Should().NotBeNull();
        }

        [Test]
        public void Should_InitializeWithDefaultParser_And_Logger_When_UsingGameSpecificConstructor()
        {
            // Arrange
            Environment.SetEnvironmentVariable("OPENAI_API_KEY", "test-key");
            var mockLogger = Mock.Of<ILogger>();

            // Act
            var parser = new IntentParser(_mockGameSpecificCommandFactory.Object, mockLogger);

            // Assert
            parser.Should().NotBeNull();
        }
    }

    [TestFixture]
    public class PropertiesTests : IntentParserTests
    {
        [Test]
        public void TurnCorrelationId_Should_BeSettableAndGettable()
        {
            // Arrange
            var expectedId = Guid.NewGuid();

            // Act
            _intentParser.TurnCorrelationId = expectedId;

            // Assert
            _intentParser.TurnCorrelationId.Should().Be(expectedId);
        }

        [Test]
        public void Logger_Should_BeSettableAndGettable()
        {
            // Arrange
            var logger = Mock.Of<ICloudWatchLogger<GenerationLog>>();

            // Act
            _intentParser.Logger = logger;

            // Assert
            _intentParser.Logger.Should().Be(logger);
        }
    }
}