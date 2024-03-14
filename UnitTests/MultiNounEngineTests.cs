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
}