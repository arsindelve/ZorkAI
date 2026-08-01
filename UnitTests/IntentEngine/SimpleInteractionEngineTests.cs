using GameEngine;
using GameEngine.IntentEngine;
using Model.AIGeneration;
using Model.AIGeneration.Requests;
using Model.Intent;
using Model.Interface;
using Model.Interaction;
using Model.Item;
using Model.Location;
using Model.Movement;
using Utilities;

namespace UnitTests.IntentEngine;

// Note: Using real Repository for integration tests since it's lightweight and in-memory

[TestFixture]
public class SimpleInteractionEngineTests
{
    private SimpleInteractionEngine _engine;
    private Mock<IItemProcessorFactory> _mockItemProcessorFactory;
    private Mock<IContext> _mockContext;
    private Mock<IGenerationClient> _mockGenerationClient;
    private Mock<ILocation> _mockLocation;

    [SetUp]
    public void SetUp()
    {
        // Reset Repository for clean test state
        Repository.Reset();
        
        _mockItemProcessorFactory = new Mock<IItemProcessorFactory>();
        _mockContext = new Mock<IContext>();
        _mockGenerationClient = new Mock<IGenerationClient>();
        
        // Create location mock that implements both ILocation and ICanContainItems
        _mockLocation = new Mock<ILocation>();
        _mockLocation.As<ICanContainItems>(); // Setup interface before accessing Object

        _engine = new SimpleInteractionEngine(_mockItemProcessorFactory.Object);

        _mockContext.Setup(c => c.CurrentLocation).Returns(_mockLocation.Object);
        _mockContext.Setup(c => c.ItIsDarkHere).Returns(false);
        _mockContext.Setup(c => c.GetAllItemsRecursively).Returns(new List<IItem>());
        _mockContext.Setup(c => c.SystemPromptAddendum).Returns("");

        _mockLocation.Setup(l => l.GetDescriptionForGeneration(It.IsAny<IContext>())).Returns("Test location");
        _mockLocation.As<ICanContainItems>().Setup(l => l.GetAllItemsRecursively).Returns(new List<IItem>());
    }

    [TestFixture]
    public class ProcessMethod : SimpleInteractionEngineTests
    {
        [Test]
        public async Task Should_ThrowArgumentException_When_IntentIsNotSimpleIntent()
        {
            // Arrange
            var wrongIntent = new MoveIntent { Direction = Direction.N };

            // Act & Assert
            await FluentActions.Invoking(async () => 
                await _engine.Process(wrongIntent, _mockContext.Object, _mockGenerationClient.Object))
                .Should().ThrowAsync<ArgumentException>();
        }

        [Test]
        public async Task Should_SetLastNoun_When_Processing()
        {
            // Arrange
            var intent = new SimpleIntent { Verb = "take", Noun = "sword" };
            var interaction = new PositiveInteractionResult("Done");
            
            _mockLocation.Setup(l => l.RespondToSimpleInteraction(intent, _mockContext.Object, _mockGenerationClient.Object, _mockItemProcessorFactory.Object))
                .ReturnsAsync(interaction);

            // Act
            await _engine.Process(intent, _mockContext.Object, _mockGenerationClient.Object);

            // Assert
            _mockContext.VerifySet(c => c.LastNoun = "sword", Times.Once);
        }

        [Test]
        public async Task Should_ReturnDisambiguation_When_MultipleItemsMatch()
        {
            // Arrange
            var intent = new SimpleIntent { Verb = "take", Noun = "box" };
            
            var redBox = new Mock<IItem>();
            redBox.Setup(i => i.NounsForMatching).Returns(new[] { "red box", "box" });
            redBox.Setup(i => i.NounsForPreciseMatching).Returns(new[] { "red box", "box" });
            
            var blueBox = new Mock<IItem>();
            blueBox.Setup(i => i.NounsForMatching).Returns(new[] { "blue box", "box" });
            blueBox.Setup(i => i.NounsForPreciseMatching).Returns(new[] { "blue box", "box" });

            var items = new List<IItem> { redBox.Object, blueBox.Object };
            _mockContext.Setup(c => c.GetAllItemsRecursively).Returns(items);

            // Act
            var result = await _engine.Process(intent, _mockContext.Object, _mockGenerationClient.Object);

            // Assert
            result.resultObject.Should().BeOfType<DisambiguationInteractionResult>();
            result.ResultMessage.Should().Contain("Do you mean");
            result.ResultMessage.Should().Contain("red box");
            result.ResultMessage.Should().Contain("blue box");
        }

        [Test]
        public async Task Should_ReturnDarkMessage_When_ItIsDarkAndNotLightSource()
        {
            // Arrange
            var intent = new SimpleIntent { Verb = "take", Noun = "nonexistent" };
            _mockContext.Setup(c => c.ItIsDarkHere).Returns(true);

            // Repository.GetItem("nonexistent") will return null (not a light source)
            
            // Act
            var result = await _engine.Process(intent, _mockContext.Object, _mockGenerationClient.Object);

            // Assert
            result.resultObject.Should().BeNull();
            result.ResultMessage.Should().Be("It's too dark to see! ");
        }

        [Test]
        public async Task Should_ReturnLocationInteraction_When_LocationResponds()
        {
            // Arrange
            var intent = new SimpleIntent { Verb = "open", Noun = "door" };
            var locationInteraction = new PositiveInteractionResult("Door opens");

            _mockLocation.Setup(l => l.RespondToSimpleInteraction(intent, _mockContext.Object, _mockGenerationClient.Object, _mockItemProcessorFactory.Object))
                .ReturnsAsync(locationInteraction);

            var contextInteraction = new InteractionResult();
            _mockContext.Setup(c => c.RespondToSimpleInteraction(intent, _mockGenerationClient.Object, _mockItemProcessorFactory.Object))
                .ReturnsAsync(contextInteraction);

            // Act
            var result = await _engine.Process(intent, _mockContext.Object, _mockGenerationClient.Object);

            // Assert
            result.resultObject.Should().Be(locationInteraction);
            result.ResultMessage.Should().Be("Door opens" + Environment.NewLine);
        }

        [Test]
        public async Task Should_ReturnContextInteraction_When_LocationDoesNotRespondButContextDoes()
        {
            // Arrange
            var intent = new SimpleIntent { Verb = "examine", Noun = "sword" };
            
            var locationInteraction = new InteractionResult();
            _mockLocation.Setup(l => l.RespondToSimpleInteraction(intent, _mockContext.Object, _mockGenerationClient.Object, _mockItemProcessorFactory.Object))
                .ReturnsAsync(locationInteraction);

            var contextInteraction = new PositiveInteractionResult("A sharp blade");
            _mockContext.Setup(c => c.RespondToSimpleInteraction(intent, _mockGenerationClient.Object, _mockItemProcessorFactory.Object))
                .ReturnsAsync(contextInteraction);

            // Act
            var result = await _engine.Process(intent, _mockContext.Object, _mockGenerationClient.Object);

            // Assert
            result.resultObject.Should().Be(contextInteraction);
            result.ResultMessage.Should().Be("A sharp blade" + Environment.NewLine);
        }

        [Test]
        public async Task Should_ReturnGeneratedResponse_When_LocationHasNoVerbMatch()
        {
            // Arrange
            var intent = new SimpleIntent { Verb = "push", Noun = "sword" };

            // Add sword to Repository and place it in the location so GetItemInScope can find it
            var sword = Repository.GetItem<ZorkOne.Item.Sword>();
            sword.CurrentLocation = _mockLocation.As<ICanContainItems>().Object;

            // Mock HasMatchingNoun so GetItemInScope can find the sword in the location
            _mockLocation.Setup(l => l.HasMatchingNoun("sword", true)).Returns((true, sword));

            var noVerbMatch = new NoVerbMatchInteractionResult { Noun = "sword", Verb = "push" };
            _mockLocation.Setup(l => l.RespondToSimpleInteraction(intent, _mockContext.Object, _mockGenerationClient.Object, _mockItemProcessorFactory.Object))
                .ReturnsAsync(noVerbMatch);

            var contextInteraction = new InteractionResult();
            _mockContext.Setup(c => c.RespondToSimpleInteraction(intent, _mockGenerationClient.Object, _mockItemProcessorFactory.Object))
                .ReturnsAsync(contextInteraction);

            var generatedResponse = "You can't push the sword.";
            _mockGenerationClient.Setup(g => g.GenerateNarration(It.IsAny<VerbHasNoEffectOperationRequest>(), ""))
                .ReturnsAsync(generatedResponse);

            // Act
            var result = await _engine.Process(intent, _mockContext.Object, _mockGenerationClient.Object);

            // Assert
            result.resultObject.Should().Be(noVerbMatch);
            result.ResultMessage.Should().Contain(generatedResponse);
        }

        [Test]
        public async Task Should_ReturnGeneratedResponse_When_ContextHasNoVerbMatch()
        {
            // Arrange
            var intent = new SimpleIntent { Verb = "push", Noun = "sword" };

            // Add sword to Repository and place it in the context (inventory) so GetItemInScope can find it
            var sword = Repository.GetItem<ZorkOne.Item.Sword>();
            sword.CurrentLocation = _mockContext.Object;

            // Mock HasMatchingNoun so GetItemInScope can find the sword in inventory
            _mockContext.Setup(c => c.HasMatchingNoun("sword", true)).Returns((true, sword));

            var locationInteraction = new InteractionResult();
            _mockLocation.Setup(l => l.RespondToSimpleInteraction(intent, _mockContext.Object, _mockGenerationClient.Object, _mockItemProcessorFactory.Object))
                .ReturnsAsync(locationInteraction);

            var noVerbMatch = new NoVerbMatchInteractionResult { Noun = "sword", Verb = "push" };
            _mockContext.Setup(c => c.RespondToSimpleInteraction(intent, _mockGenerationClient.Object, _mockItemProcessorFactory.Object))
                .ReturnsAsync(noVerbMatch);

            var generatedResponse = "Pushing the sword accomplishes nothing.";
            _mockGenerationClient.Setup(g => g.GenerateNarration(It.IsAny<VerbHasNoEffectOperationRequest>(), ""))
                .ReturnsAsync(generatedResponse);

            // Act
            var result = await _engine.Process(intent, _mockContext.Object, _mockGenerationClient.Object);

            // Assert
            result.resultObject.Should().Be(noVerbMatch);
            result.ResultMessage.Should().Contain(generatedResponse);
        }
    }


    /// <summary>
    ///     The ambiguity check itself now lives in <see cref="NounDisambiguator" />, shared with the
    ///     multi-noun and enter/exit engines (issue #532) - so these call it directly rather than
    ///     reaching into SimpleInteractionEngine by reflection.
    /// </summary>
    [TestFixture]
    public class CheckDisambiguationMethod : SimpleInteractionEngineTests
    {
        [Test]
        public void Should_ReturnNull_When_NoAmbiguousItems()
        {
            var intent = new SimpleIntent { Verb = "take", Noun = "sword" };

            var sword = new Mock<IItem>();
            sword.Setup(i => i.NounsForMatching).Returns(["sword", "blade"]);
            sword.Setup(i => i.NounsForPreciseMatching).Returns(["sword", "blade"]);

            _mockContext.Setup(c => c.GetAllItemsRecursively).Returns([sword.Object]);

            var result = NounDisambiguator.Check(intent.MatchNounAndAdjective, _mockContext.Object, "take {0}");

            result.Should().BeNull();
        }

        [Test]
        public void Should_ReturnNull_When_OnlyOneItemMatches()
        {
            var intent = new SimpleIntent { Verb = "take", Noun = "sword" };

            var sword = new Mock<IItem>();
            sword.Setup(i => i.NounsForMatching).Returns(["sword", "blade"]);

            var dagger = new Mock<IItem>();
            dagger.Setup(i => i.NounsForMatching).Returns(["dagger", "knife"]);

            _mockContext.Setup(c => c.GetAllItemsRecursively).Returns([sword.Object, dagger.Object]);

            var result = NounDisambiguator.Check(intent.MatchNounAndAdjective, _mockContext.Object, "take {0}");

            result.Should().BeNull();
        }

        [Test]
        public void Should_AskWhichOne_When_TwoItemsAnswerToTheSameNoun()
        {
            var intent = new SimpleIntent { Verb = "take", Noun = "door" };

            var red = new Mock<IItem>();
            red.Setup(i => i.NounsForMatching).Returns(["door", "red door"]);
            red.Setup(i => i.NounsForPreciseMatching).Returns(["door", "red door"]);

            var blue = new Mock<IItem>();
            blue.Setup(i => i.NounsForMatching).Returns(["door", "blue door"]);
            blue.Setup(i => i.NounsForPreciseMatching).Returns(["door", "blue door"]);

            _mockContext.Setup(c => c.GetAllItemsRecursively).Returns([red.Object, blue.Object]);

            var result = NounDisambiguator.Check(intent.MatchNounAndAdjective, _mockContext.Object, "take {0}");

            result.Should().NotBeNull();
            result!.InteractionMessage.Should().Contain("red door");
            result.InteractionMessage.Should().Contain("blue door");
            // The reply has to name one item unambiguously, or we would loop forever asking.
            result.PossibleResponses["door"].Should().BeOneOf("red door", "blue door");
        }
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up any static state
        Repository.Reset();
    }
}