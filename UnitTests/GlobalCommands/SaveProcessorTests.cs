using System.Text;
using GameEngine.StaticCommand;
using GameEngine.StaticCommand.Implementation;
using Model.AIGeneration;
using Model.AIGeneration.Requests;
using Model.Interface;
using Model.Location;

namespace UnitTests.GlobalCommands;

[TestFixture]
public class SaveProcessorTests
{
    [SetUp]
    public void SetUp()
    {
        _mockWriter = new Mock<ISaveGameWriter>();
        _mockClient = new Mock<IGenerationClient>();
        _mockContext = new Mock<IContext>();
        _mockEngine = new Mock<IGameEngine>();
        _mockLocation = new Mock<ILocation>();

        _mockLocation.Setup(l => l.GetDescriptionForGeneration(It.IsAny<IContext>()))
            .Returns("Test location");
        _mockContext.Setup(c => c.CurrentLocation).Returns(_mockLocation.Object);
        _mockContext.Setup(c => c.Game.DefaultSaveGameName).Returns("default.sav");
    }

    private Mock<ISaveGameWriter> _mockWriter = null!;
    private Mock<IGenerationClient> _mockClient = null!;
    private Mock<IContext> _mockContext = null!;
    private Mock<IGameEngine> _mockEngine = null!;
    private Mock<ILocation> _mockLocation = null!;

    [TestFixture]
    public class ConstructorTests : SaveProcessorTests
    {
        [Test]
        public void Should_CreateInstance_WithDefaultConstructor()
        {
            new SaveProcessor().Should().NotBeNull();
        }

        [Test]
        public void Should_CreateInstance_WithInjectedWriter()
        {
            new SaveProcessor(_mockWriter.Object).Should().NotBeNull();
        }
    }

    [TestFixture]
    public class WebRuntimeTests : SaveProcessorTests
    {
        [Test]
        public async Task Should_ReturnSaveTag_When_RuntimeIsWeb()
        {
            // Arrange
            var processor = new SaveProcessor(_mockWriter.Object);

            // Act
            var result = await processor.Process("save", _mockContext.Object, _mockClient.Object, Runtime.Web);

            // Assert
            result.Should().Be("<Save>");
        }
    }

    [TestFixture]
    public class ContinueProcessingTests : SaveProcessorTests
    {
        [Test]
        public void Should_ContinueProcessingBeFalse()
        {
            // Arrange
            var processor = new SaveProcessor(_mockWriter.Object);

            // Assert
            ((IStatefulProcessor)processor).ContinueProcessing.Should().BeFalse();
        }
    }

    [TestFixture]
    public class PromptForFilenameTests : SaveProcessorTests
    {
        [Test]
        public async Task Should_ShowDefaultFilename_When_NoLastSaveGameName()
        {
            // Arrange
            var target = new SaveProcessor(Mock.Of<ISaveGameWriter>());
            var client = new Mock<IGenerationClient>();
            client.Setup(s => s.GenerateNarration(It.IsAny<BeforeSaveGameRequest>(), It.IsAny<string>()))
                .ReturnsAsync("shelly");

            var context = Mock.Of<IContext>(c => c.Game.DefaultSaveGameName == "bobby");
            Mock.Get(context).Setup(s => s.CurrentLocation.GetDescriptionForGeneration(Mock.Of<IContext>()))
                .Returns("here");

            // Act
            var response = await target.Process("input", context, client.Object, Runtime.Unknown);

            // Assert
            response.Should().Contain("bobby");
            response.Should().Contain("shelly");
            target.Completed.Should().BeFalse();
            ((IStatefulProcessor)target).ContinueProcessing.Should().BeFalse();
        }

        [Test]
        public async Task Should_ShowLastSaveGameName_When_Available()
        {
            // Arrange
            var target = new SaveProcessor(Mock.Of<ISaveGameWriter>());
            var client = new Mock<IGenerationClient>();
            client.Setup(s => s.GenerateNarration(It.IsAny<BeforeSaveGameRequest>(), It.IsAny<string>()))
                .ReturnsAsync("shelly");

            var context = Mock.Of<IContext>(c => c.Game.DefaultSaveGameName == "bobby");
            Mock.Get(context).Setup(s => s.CurrentLocation.GetDescriptionForGeneration(Mock.Of<IContext>()))
                .Returns("here");
            Mock.Get(context).Setup(s => s.LastSaveGameName).Returns("jake");

            // Act
            var response = await target.Process("input", context, client.Object, Runtime.Unknown);

            // Assert
            response.Should().Contain("jake");
            response.Should().NotContain("bobby");
            response.Should().Contain("shelly");
            ((IStatefulProcessor)target).ContinueProcessing.Should().BeFalse();
        }
    }

    [TestFixture]
    public class CompleteSaveTests : SaveProcessorTests
    {
        [Test]
        public async Task Should_SaveWithDefaultName_When_InputIsEmpty()
        {
            // Arrange
            var writer = Mock.Of<ISaveGameWriter>();
            var target = new SaveProcessor(writer);

            var client = new Mock<IGenerationClient>();
            client.Setup(s => s.GenerateNarration(It.IsAny<BeforeSaveGameRequest>(), It.IsAny<string>()))
                .ReturnsAsync("shelly");

            var engine = new Mock<IGameEngine>();
            engine.Setup(s => s.SaveGame()).Returns("fred");
            engine.Setup(s => s.GenerateSaveGameNarration()).ReturnsAsync("karen");

            var context = Mock.Of<IContext>(c => c.Game.DefaultSaveGameName == "bobby");
            Mock.Get(context).Setup(s => s.CurrentLocation.GetDescriptionForGeneration(Mock.Of<IContext>()))
                .Returns("here");
            Mock.Get(context).Setup(s => s.Engine).Returns(engine.Object);

            // Act
            await target.Process("input", context, client.Object, Runtime.Unknown);
            var response = await target.Process("", context, client.Object, Runtime.Unknown);

            // Assert
            response.Should().Contain("karen");
            target.Completed.Should().BeTrue();
            engine.Verify(e => e.GenerateSaveGameNarration(), Times.Once);
            var bytesToEncode = Encoding.UTF8.GetBytes("fred");
            var encodedText = Convert.ToBase64String(bytesToEncode);
            Mock.Get(writer).Verify(s => s.Write("bobby", encodedText));
        }

        [Test]
        public async Task Should_SaveWithUserProvidedName_When_InputIsProvided()
        {
            // Arrange
            var writer = Mock.Of<ISaveGameWriter>();
            var target = new SaveProcessor(writer);

            var client = new Mock<IGenerationClient>();
            client.Setup(s => s.GenerateNarration(It.IsAny<BeforeSaveGameRequest>(), It.IsAny<string>()))
                .ReturnsAsync("shelly");

            var engine = new Mock<IGameEngine>();
            engine.Setup(s => s.SaveGame()).Returns("fred");
            engine.Setup(s => s.GenerateSaveGameNarration()).ReturnsAsync("karen");

            var context = Mock.Of<IContext>(c => c.Game.DefaultSaveGameName == "bobby");
            Mock.Get(context).Setup(s => s.CurrentLocation.GetDescriptionForGeneration(Mock.Of<IContext>()))
                .Returns("here");
            Mock.Get(context).Setup(s => s.Engine).Returns(engine.Object);

            // Act
            await target.Process("save", context, client.Object, Runtime.Unknown);
            var response = await target.Process("danny", context, client.Object, Runtime.Unknown);

            // Assert
            response.Should().Contain("karen");
            target.Completed.Should().BeTrue();
            engine.Verify(e => e.GenerateSaveGameNarration(), Times.Once);
            var bytesToEncode = Encoding.UTF8.GetBytes("fred");
            var encodedText = Convert.ToBase64String(bytesToEncode);
            Mock.Get(writer).Verify(s => s.Write("danny", encodedText));
        }

        [Test]
        public async Task Should_UseLastSaveGameName_When_InputIsEmptyAndLastSaveExists()
        {
            // Arrange
            var writer = Mock.Of<ISaveGameWriter>();
            var target = new SaveProcessor(writer);

            var client = new Mock<IGenerationClient>();
            client.Setup(s => s.GenerateNarration(It.IsAny<BeforeSaveGameRequest>(), It.IsAny<string>()))
                .ReturnsAsync("prompt");

            var engine = new Mock<IGameEngine>();
            engine.Setup(s => s.SaveGame()).Returns("game data");
            engine.Setup(s => s.GenerateSaveGameNarration()).ReturnsAsync("saved!");

            var context = Mock.Of<IContext>(c => c.Game.DefaultSaveGameName == "default.sav");
            Mock.Get(context).Setup(s => s.CurrentLocation.GetDescriptionForGeneration(Mock.Of<IContext>()))
                .Returns("here");
            Mock.Get(context).Setup(s => s.Engine).Returns(engine.Object);
            Mock.Get(context).Setup(s => s.LastSaveGameName).Returns("lastsave.sav");

            // Prompt for filename
            await target.Process("save", context, client.Object, Runtime.Unknown);

            // Act - empty input should use LastSaveGameName
            await target.Process("", context, client.Object, Runtime.Unknown);

            // Assert
            var bytesToEncode = Encoding.UTF8.GetBytes("game data");
            var encodedText = Convert.ToBase64String(bytesToEncode);
            Mock.Get(writer).Verify(s => s.Write("lastsave.sav", encodedText));
        }

        [Test]
        public async Task Should_HandleException_When_WriteFails()
        {
            // Arrange
            var writer = Mock.Of<ISaveGameWriter>();
            Mock.Get(writer).Setup(s => s.Write(It.IsAny<string>(), It.IsAny<string>())).Throws<Exception>();
            var target = new SaveProcessor(writer);

            var client = new Mock<IGenerationClient>();
            client.Setup(s => s.GenerateNarration(It.IsAny<BeforeSaveGameRequest>(), It.IsAny<string>()))
                .ReturnsAsync("shelly");
            client.Setup(s => s.GenerateNarration(It.IsAny<AfterSaveGameRequest>(), It.IsAny<string>()))
                .ReturnsAsync("karen");
            client.Setup(s => s.GenerateNarration(It.IsAny<SaveFailedUnknownReasonGameRequest>(), It.IsAny<string>()))
                .ReturnsAsync("frank");

            var engine = Mock.Of<IGameEngine>(s => s.SaveGame() == "fred");
            var context = Mock.Of<IContext>(c => c.Game.DefaultSaveGameName == "bobby");
            Mock.Get(context).Setup(s => s.CurrentLocation.GetDescriptionForGeneration(Mock.Of<IContext>()))
                .Returns("here");
            Mock.Get(context).Setup(s => s.Engine).Returns(engine);

            // Act
            await target.Process("save", context, client.Object, Runtime.Unknown);
            var response = await target.Process("danny", context, client.Object, Runtime.Unknown);

            // Assert
            response.Should().Contain("frank");
            target.Completed.Should().BeTrue();
        }

        [Test]
        public async Task Should_ReturnEmpty_When_NoEngineAndPrompted()
        {
            // Arrange
            var target = new SaveProcessor(_mockWriter.Object);
            _mockClient.Setup(c => c.GenerateNarration(It.IsAny<BeforeSaveGameRequest>(), It.IsAny<string>()))
                .ReturnsAsync("Prompt");
            _mockContext.Setup(c => c.Engine).Returns((IGameEngine?)null);

            // First call to set _havePromptedForFilename
            await target.Process("save", _mockContext.Object, _mockClient.Object, Runtime.Unknown);

            // Act
            var result = await target.Process("filename.sav", _mockContext.Object, _mockClient.Object, Runtime.Unknown);

            // Assert
            result.Should().Be(string.Empty);
        }

        [Test]
        public async Task Should_SetLastSaveGameName_AfterSave()
        {
            // Arrange
            var writer = Mock.Of<ISaveGameWriter>();
            var target = new SaveProcessor(writer);

            var client = new Mock<IGenerationClient>();
            client.Setup(s => s.GenerateNarration(It.IsAny<BeforeSaveGameRequest>(), It.IsAny<string>()))
                .ReturnsAsync("prompt");

            var engine = new Mock<IGameEngine>();
            engine.Setup(s => s.SaveGame()).Returns("data");
            engine.Setup(s => s.GenerateSaveGameNarration()).ReturnsAsync("saved!");

            var context = new Mock<IContext>();
            context.Setup(c => c.Game.DefaultSaveGameName).Returns("default.sav");
            context.Setup(c => c.CurrentLocation.GetDescriptionForGeneration(It.IsAny<IContext>()))
                .Returns("here");
            context.Setup(c => c.Engine).Returns(engine.Object);
            context.SetupProperty(c => c.LastSaveGameName);

            // Prompt for filename
            await target.Process("save", context.Object, client.Object, Runtime.Unknown);

            // Act
            await target.Process("newsave.sav", context.Object, client.Object, Runtime.Unknown);

            // Assert
            context.Object.LastSaveGameName.Should().Be("newsave.sav");
        }
    }
}
