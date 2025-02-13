using GameEngine;
using GameEngine.IntentEngine;
using Model.AIGeneration;
using Model.AIGeneration.Requests;
using Model.Intent;
using Model.Interaction;
using Model.Interface;
using Model.Item;
using Model.Location;

namespace UnitTests;

public class MultiNounEngineTests
{
    [Test]
    public void WrongIntentType()
    {
        var engine = new MultiNounEngine();

        Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            var intent = new SimpleIntent
            {
                OriginalInput = "",
                Verb = "",
                Noun = ""
            };
            var context = new Mock<IContext>().Object;
            var generationClient = new Mock<IGenerationClient>().Object;
            await engine.Process(intent, context, generationClient);
        });
    }

    [Test]
    public async Task NounOneAndNounTwoDoNotExistAnywhereInTheStory()
    {
        var engine = new MultiNounEngine();

        var intent = new MultiNounIntent
        {
            Verb = "kill",
            NounOne = "ogre",
            NounTwo = "halberd",
            Preposition = "with",
            OriginalInput = "kill the ogre with the halberd"
        };

        var location = new Mock<ILocation>();
        location.Setup(s => s.GetDescriptionForGeneration(Mock.Of<IContext>())).Returns("hello!");
        var context = Mock.Of<IContext>(s => s.CurrentLocation == location.Object);
        Mock.Get(context).Setup(s => s.Items).Returns(new List<IItem>());
        var generationClient = new Mock<IGenerationClient>();
        generationClient.Setup(s => s.GenerateNarration(It.IsAny<CommandHasNoEffectOperationRequest>(), It.IsAny<string>()))
            .ReturnsAsync("bob");

        // Act
        var result = await engine.Process(intent, context, generationClient.Object);

        // Assert
        result.ResultMessage.Should().Contain("bob");
    }

    [Test]
    public async Task NounTwoDoesNotExistHereInTheStory()
    {
        // Pre-load
        Repository.Reset();
        Repository.GetItem<Troll>();
        var engine = new MultiNounEngine();

        var intent = new MultiNounIntent
        {
            Verb = "kill",
            NounOne = "troll",
            NounTwo = "halberd",
            Preposition = "with",
            OriginalInput = "kill the troll with the halberd"
        };

        var location = new Mock<ILocation>();
        location.Setup(s => s.GetDescriptionForGeneration(It.IsAny<IContext>())).Returns("troll!");
        var context = Mock.Of<IContext>(s => s.CurrentLocation == location.Object);
        Mock.Get(context).Setup(s => s.Items).Returns(new List<IItem>());
        var generationClient = new Mock<IGenerationClient>();
        generationClient.Setup(s => s.GenerateNarration(It.IsAny<MissingSecondNounMultiNounOperationRequest>(), It.IsAny<string>()))
            .ReturnsAsync("bob");

        // Act
        var result = await engine.Process(intent, context, generationClient.Object);

        // Assert
        result.ResultMessage.Should().Contain("bob");
    }

    [Test]
    public async Task NounOneDoesNotExistHereInTheStory()
    {
        // Pre-load
        Repository.Reset();
        Repository.GetItem<Sword>();
        var engine = new MultiNounEngine();

        var intent = new MultiNounIntent
        {
            Verb = "kill",
            NounOne = "unicorn",
            NounTwo = "sword",
            Preposition = "with",
            OriginalInput = "kill the unicorn with the sword"
        };

        var location = new Mock<ILocation>();
        location.Setup(s => s.GetDescriptionForGeneration(It.IsAny<IContext>())).Returns("sword!");
        var context = Mock.Of<IContext>(s => s.CurrentLocation == location.Object);
        Mock.Get(context).Setup(s => s.Items).Returns(new List<IItem>());
        var generationClient = new Mock<IGenerationClient>();
        generationClient.Setup(s => s.GenerateNarration(It.IsAny<MissingFirstNounMultiNounOperationRequest>(), It.IsAny<string>()))
            .ReturnsAsync("bob");

        // Act
        var result = await engine.Process(intent, context, generationClient.Object);

        // Assert
        result.ResultMessage.Should().Contain("bob");
    }

    [Test]
    public async Task NeitherNounExistsHereExistHereInTheStory()
    {
        Repository.Reset();
        Repository.GetItem<Leaflet>();
        var engine = new MultiNounEngine();

        var intent = new MultiNounIntent
        {
            Verb = "put",
            NounOne = "leaflet",
            NounTwo = "mailbox",
            Preposition = "in",
            OriginalInput = "put leaflet in mailbox"
        };

        var location = new Mock<ILocation>();
        location.Setup(s => s.GetDescriptionForGeneration(It.IsAny<IContext>())).Returns("mailbox leaflet!");
        var context = Mock.Of<IContext>(s => s.CurrentLocation == location.Object);
        Mock.Get(context).Setup(s => s.Items).Returns(new List<IItem>());
        var generationClient = new Mock<IGenerationClient>();
        generationClient.Setup(s => s.GenerateNarration(It.IsAny<VerbHasNoEffectMultiNounOperationRequest>(), It.IsAny<string>()))
            .ReturnsAsync("bob");

        // Act
        var result = await engine.Process(intent, context, generationClient.Object);

        // Assert
        result.ResultMessage.Should().Contain("bob");
    }

    [Test]
    public async Task NeitherNounExistsHereExist()
    {
        Repository.Reset();
        Repository.GetItem<Leaflet>();
        Repository.GetItem<Mailbox>();
        var engine = new MultiNounEngine();

        var intent = new MultiNounIntent
        {
            Verb = "put",
            NounOne = "leaflet",
            NounTwo = "mailbox",
            Preposition = "in",
            OriginalInput = "put leaflet in mailbox"
        };

        var location = new Mock<ILocation>();
        location.Setup(s => s.GetDescriptionForGeneration(It.IsAny<IContext>())).Returns("hello");
        var context = Mock.Of<IContext>(s => s.CurrentLocation == location.Object);
        Mock.Get(context).Setup(s => s.Items).Returns(new List<IItem>());
        var generationClient = new Mock<IGenerationClient>();
        generationClient.Setup(s => s.GenerateNarration(It.IsAny<MissingBothNounsMultiNounOperationRequest>(), It.IsAny<string>()))
            .ReturnsAsync("bob");

        // Act
        (InteractionResult? resultObject, string ResultMessage) result =
            await engine.Process(intent, context, generationClient.Object);

        // Assert
        result.ResultMessage.Should().Contain("bob");
    }

    [Test]
    public async Task TooDark()
    {
        Repository.Reset();
        var engine = new MultiNounEngine();
        var intent = new MultiNounIntent
        {
            Verb = "put",
            NounOne = "leaflet",
            NounTwo = "mailbox",
            Preposition = "in",
            OriginalInput = "put leaflet in mailbox"
        };

        var location = new Mock<ILocation>();
        location.Setup(s => s.GetDescriptionForGeneration(Mock.Of<IContext>())).Returns("hello");
        var context = Mock.Of<IContext>(s => s.CurrentLocation == location.Object);
        Mock.Get(context).Setup(s => s.ItIsDarkHere).Returns(true);
        var generationClient = new Mock<IGenerationClient>();

        // Act
        var result = await engine.Process(intent, context, generationClient.Object);

        // Assert
        result.ResultMessage.Should().Contain("to see!");
    }
}