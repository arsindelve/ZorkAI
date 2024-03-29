﻿using Game.StaticCommand;
using Game.StaticCommand.Implementation;
using Model.AIGeneration;
using Model.AIGeneration.Requests;

namespace UnitTests.GlobalCommands;

public class RestoreProcessorTests
{
    [Test]
    public void Constructor()
    {
        new RestoreProcessor().Should().NotBeNull();
    }
    
    [Test]
    public async Task PromptForFilename_GameDefault()
    {
        var target = new RestoreProcessor(Mock.Of<ISaveGameReader>());

        var client = new Mock<IGenerationClient>();
        client.Setup(s => s.CompleteChat(It.IsAny<BeforeRestoreGameRequest>()))
            .ReturnsAsync("shelly");

        var context = Mock.Of<IContext>(c => c.Game.DefaultSaveGameName == "bobby");
        Mock.Get(context).Setup(s => s.CurrentLocation.DescriptionForGeneration).Returns("here");

        // Act
        var response = await target.Process("input", context, client.Object);

        // Assert
        response.Should().Contain("bobby");
        response.Should().Contain("shelly");
        target.Completed.Should().BeFalse();
        ((IStatefulProcessor)target).ContinueProcessing.Should().BeFalse();
    }
    
    [Test]
    public async Task PromptForFilename_LastSaveGameName()
    {
        var target = new RestoreProcessor(Mock.Of<ISaveGameReader>());

        var client = new Mock<IGenerationClient>();
        client.Setup(s => s.CompleteChat(It.IsAny<BeforeRestoreGameRequest>()))
            .ReturnsAsync("shelly");

        var context = Mock.Of<IContext>(c => c.Game.DefaultSaveGameName == "bobby");
        Mock.Get(context).Setup(s => s.CurrentLocation.DescriptionForGeneration).Returns("here");
        Mock.Get(context).Setup(s => s.LastSaveGameName).Returns("jake");

        // Act
        var response = await target.Process("input", context, client.Object);

        // Assert
        response.Should().Contain("jake");
        response.Should().NotContain("bobby");
        response.Should().Contain("shelly");
        ((IStatefulProcessor)target).ContinueProcessing.Should().BeFalse();
    }

}