using Game.IntentEngine;
using Model.Intent;
using OpenAI.Requests;

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
    public async Task NounOneAndNounTwoDoNotExistAnywhereInTheStore()
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
        location.Setup(s => s.DescriptionForGeneration).Returns("hello!");
        var context = Mock.Of<IContext>(s => s.CurrentLocation == location.Object);
        var generationClient = new Mock<IGenerationClient>();
        generationClient.Setup(s => s.CompleteChat(It.IsAny<CommandHasNoEffectOperationRequest>()))
            .ReturnsAsync("bob");

        // Act
        var result = await engine.Process(intent, context, generationClient.Object);

        // Assert
        result.Should().Contain("bob");
    }
    
    [Test]
    public async Task NounTwoDoesNotExistHereInTheStore()
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
        location.Setup(s => s.DescriptionForGeneration).Returns("troll!");
        var context = Mock.Of<IContext>(s => s.CurrentLocation == location.Object);
        var generationClient = new Mock<IGenerationClient>();
        generationClient.Setup(s => s.CompleteChat(It.IsAny<MissingSecondNounMultiNounOperationRequest>()))
            .ReturnsAsync("bob");

        // Act
        var result = await engine.Process(intent, context, generationClient.Object);

        // Assert
        result.Should().Contain("bob");
    }
    
    [Test]
    public async Task NounOneDoesNotExistHereInTheStore()
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
        location.Setup(s => s.DescriptionForGeneration).Returns("sword!");
        var context = Mock.Of<IContext>(s => s.CurrentLocation == location.Object);
        var generationClient = new Mock<IGenerationClient>();
        generationClient.Setup(s => s.CompleteChat(It.IsAny<MissingFirstNounMultiNounOperationRequest>()))
            .ReturnsAsync("bob");

        // Act
        var result = await engine.Process(intent, context, generationClient.Object);

        // Assert
        result.Should().Contain("bob");
    }
    
    [Test]
    public async Task NeitherNounExistsHereExistHereInTheStore()
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
        location.Setup(s => s.DescriptionForGeneration).Returns("mailbox leaflet!");
        var context = Mock.Of<IContext>(s => s.CurrentLocation == location.Object);
        var generationClient = new Mock<IGenerationClient>();
        generationClient.Setup(s => s.CompleteChat(It.IsAny<VerbHasNoEffectMultiNounOperationRequest>()))
            .ReturnsAsync("bob");

        // Act
        var result = await engine.Process(intent, context, generationClient.Object);

        // Assert
        result.Should().Contain("bob");
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
        location.Setup(s => s.DescriptionForGeneration).Returns("hello");
        var context = Mock.Of<IContext>(s => s.CurrentLocation == location.Object);
        var generationClient = new Mock<IGenerationClient>();
        generationClient.Setup(s => s.CompleteChat(It.IsAny<MissingBothNounsMultiNounOperationRequest>()))
            .ReturnsAsync("bob");

        // Act
        var result = await engine.Process(intent, context, generationClient.Object);

        // Assert
        result.Should().Contain("bob");
    }
}