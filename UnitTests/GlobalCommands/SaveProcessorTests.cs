using System.Text;
using GameEngine.StaticCommand;
using GameEngine.StaticCommand.Implementation;
using Model.AIGeneration;
using Model.AIGeneration.Requests;
using Model.Interface;

namespace UnitTests.GlobalCommands;

public class SaveProcessorTests
{
    [Test]
    public void Constructor()
    {
        new SaveProcessor().Should().NotBeNull();
    }

    [Test]
    public async Task PromptForFilename_GameDefault()
    {
        var target = new SaveProcessor(Mock.Of<ISaveGameWriter>());

        var client = new Mock<IGenerationClient>();
        client.Setup(s => s.GenerateNarration(It.IsAny<BeforeSaveGameRequest>(), It.IsAny<string>()))
            .ReturnsAsync("shelly");

        var context = Mock.Of<IContext>(c => c.Game.DefaultSaveGameName == "bobby");
        Mock.Get(context).Setup(s => s.CurrentLocation.DescriptionForGeneration).Returns("here");

        // Act
        var response = await target.Process("input", context, client.Object, Runtime.Unknown);

        // Assert
        response.Should().Contain("bobby");
        response.Should().Contain("shelly");
        target.Completed.Should().BeFalse();
        ((IStatefulProcessor)target).ContinueProcessing.Should().BeFalse();
    }

    [Test]
    public async Task PromptForFilename_LastSaveGameName()
    {
        var target = new SaveProcessor(Mock.Of<ISaveGameWriter>());

        var client = new Mock<IGenerationClient>();
        client.Setup(s => s.GenerateNarration(It.IsAny<BeforeSaveGameRequest>(), It.IsAny<string>()))
            .ReturnsAsync("shelly");

        var context = Mock.Of<IContext>(c => c.Game.DefaultSaveGameName == "bobby");
        Mock.Get(context).Setup(s => s.CurrentLocation.DescriptionForGeneration).Returns("here");
        Mock.Get(context).Setup(s => s.LastSaveGameName).Returns("jake");

        // Act
        var response = await target.Process("input", context, client.Object, Runtime.Unknown);

        // Assert
        response.Should().Contain("jake");
        response.Should().NotContain("bobby");
        response.Should().Contain("shelly");
        ((IStatefulProcessor)target).ContinueProcessing.Should().BeFalse();
    }

    [Test]
    public async Task CompleteSave_DefaultName_Success()
    {
        var writer = Mock.Of<ISaveGameWriter>();
        var target = new SaveProcessor(writer);

        var client = new Mock<IGenerationClient>();
        client.Setup(s => s.GenerateNarration(It.IsAny<BeforeSaveGameRequest>(), It.IsAny<string>()))
            .ReturnsAsync("shelly");
        client.Setup(s => s.GenerateNarration(It.IsAny<AfterSaveGameRequest>(), It.IsAny<string>()))
            .ReturnsAsync("karen");

        var engine = Mock.Of<IGameEngine>(s => s.SaveGame() == "fred");
        var context = Mock.Of<IContext>(c => c.Game.DefaultSaveGameName == "bobby");
        Mock.Get(context).Setup(s => s.CurrentLocation.DescriptionForGeneration).Returns("here");
        Mock.Get(context).Setup(s => s.Engine).Returns(engine);

        // Act
        await target.Process("input", context, client.Object, Runtime.Unknown);
        var response = await target.Process("", context, client.Object, Runtime.Unknown);

        // Assert
        response.Should().Contain("karen");
        target.Completed.Should().BeTrue();
        var bytesToEncode = Encoding.UTF8.GetBytes("fred");
        var encodedText = Convert.ToBase64String(bytesToEncode);
        Mock.Get(writer).Verify(s => s.Write("bobby", encodedText));
    }

    [Test]
    public async Task CompleteSave_InputName_Success()
    {
        var writer = Mock.Of<ISaveGameWriter>();
        var target = new SaveProcessor(writer);

        var client = new Mock<IGenerationClient>();
        client.Setup(s => s.GenerateNarration(It.IsAny<BeforeSaveGameRequest>(), It.IsAny<string>()))
            .ReturnsAsync("shelly");
        client.Setup(s => s.GenerateNarration(It.IsAny<AfterSaveGameRequest>(), It.IsAny<string>()))
            .ReturnsAsync("karen");

        var engine = Mock.Of<IGameEngine>(s => s.SaveGame() == "fred");
        var context = Mock.Of<IContext>(c => c.Game.DefaultSaveGameName == "bobby");
        Mock.Get(context).Setup(s => s.CurrentLocation.DescriptionForGeneration).Returns("here");
        Mock.Get(context).Setup(s => s.Engine).Returns(engine);

        // Act
        await target.Process("save", context, client.Object, Runtime.Unknown);
        var response = await target.Process("danny", context, client.Object, Runtime.Unknown);

        // Assert
        response.Should().Contain("karen");
        target.Completed.Should().BeTrue();
        var bytesToEncode = Encoding.UTF8.GetBytes("fred");
        var encodedText = Convert.ToBase64String(bytesToEncode);
        Mock.Get(writer).Verify(s => s.Write("danny", encodedText));
    }

    [Test]
    public async Task CompleteSave_Exception()
    {
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
        Mock.Get(context).Setup(s => s.CurrentLocation.DescriptionForGeneration).Returns("here");
        Mock.Get(context).Setup(s => s.Engine).Returns(engine);

        // Act
        await target.Process("save", context, client.Object, Runtime.Unknown);
        var response = await target.Process("danny", context, client.Object, Runtime.Unknown);

        // Assert
        response.Should().Contain("frank");
        target.Completed.Should().BeTrue();
    }
}