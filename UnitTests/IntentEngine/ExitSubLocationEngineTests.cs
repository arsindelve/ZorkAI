using GameEngine;
using GameEngine.IntentEngine;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;
using Model.Item;
using ZorkOne.Location.RiverLocation;

namespace UnitTests.IntentEngine;

[TestFixture]
public class ExitSubLocationEngineTests
{
    [SetUp]
    public void SetUp()
    {
        Repository.Reset();
        _engine = new ExitSubLocationEngine();
        _mockContext = new Mock<IContext>();
        _mockGenerationClient = new Mock<IGenerationClient>();
        _mockGenerationClient.Setup(g => g.IsDisabled).Returns(true);
    }

    private ExitSubLocationEngine _engine = null!;
    private Mock<IContext> _mockContext = null!;
    private Mock<IGenerationClient> _mockGenerationClient = null!;

    [TestFixture]
    public class ProcessMethod : ExitSubLocationEngineTests
    {
        [Test]
        public async Task Should_ThrowArgumentException_When_IntentIsNotExitSubLocationIntent()
        {
            // Arrange
            var wrongIntent = new SimpleIntent { Verb = "take", Noun = "sword" };
            var location = Repository.GetLocation<WestOfHouse>();
            _mockContext.Setup(c => c.CurrentLocation).Returns(location);

            // Act & Assert
            await FluentActions.Invoking(async () =>
                    await _engine.Process(wrongIntent, _mockContext.Object, _mockGenerationClient.Object))
                .Should().ThrowAsync<ArgumentException>()
                .WithMessage("Cast error");
        }

        [Test]
        public async Task Should_ThrowArgumentException_When_NounOneIsNull()
        {
            // Arrange
            var intent = new ExitSubLocationIntent { NounOne = null! };
            var location = Repository.GetLocation<WestOfHouse>();
            _mockContext.Setup(c => c.CurrentLocation).Returns(location);

            // Act & Assert
            await FluentActions.Invoking(async () =>
                    await _engine.Process(intent, _mockContext.Object, _mockGenerationClient.Object))
                .Should().ThrowAsync<ArgumentException>()
                .WithMessage("*null or empty noun*");
        }

        [Test]
        public async Task Should_ThrowArgumentException_When_NounOneIsEmpty()
        {
            // Arrange
            var intent = new ExitSubLocationIntent { NounOne = "" };
            var location = Repository.GetLocation<WestOfHouse>();
            _mockContext.Setup(c => c.CurrentLocation).Returns(location);

            // Act & Assert
            await FluentActions.Invoking(async () =>
                    await _engine.Process(intent, _mockContext.Object, _mockGenerationClient.Object))
                .Should().ThrowAsync<ArgumentException>()
                .WithMessage("*null or empty noun*");
        }

        [Test]
        public async Task Should_ReturnCantGoMessage_When_ItemNotInScope()
        {
            // Arrange
            var location = Repository.GetLocation<WestOfHouse>();
            _mockContext.Setup(c => c.CurrentLocation).Returns(location);
            _mockContext.Setup(c => c.Items).Returns(new List<IItem>());

            var intent = new ExitSubLocationIntent { NounOne = "nonexistent" };

            // Act
            var result = await _engine.Process(intent, _mockContext.Object, _mockGenerationClient.Object);

            // Assert
            result.resultObject.Should().BeNull();
            result.ResultMessage.Should().Be("You cannot go that way. ");
        }

        [Test]
        public async Task Should_TryNounTwo_When_NounOneNotFound()
        {
            // Arrange
            var location = Repository.GetLocation<Shore>();
            var boat = Repository.GetItem<PileOfPlastic>();
            boat.IsInflated = true;
            location.ItemPlacedHere(boat);
            location.SubLocation = boat; // In the boat

            _mockContext.Setup(c => c.CurrentLocation).Returns(location);
            _mockContext.Setup(c => c.Items).Returns(new List<IItem>());

            // "get out of boat" - "out" is NounOne, "boat" is NounTwo
            var intent = new ExitSubLocationIntent { NounOne = "out", NounTwo = "boat" };

            // Act
            var result = await _engine.Process(intent, _mockContext.Object, _mockGenerationClient.Object);

            // Assert
            result.resultObject.Should().BeNull();
            result.ResultMessage.Should().Contain("feet");
        }

        [Test]
        public async Task Should_ReturnCantGoMessage_When_ItemIsNotSubLocation()
        {
            // Arrange - sword is not a sub-location
            var location = Repository.GetLocation<LivingRoom>();
            var sword = Repository.GetItem<Sword>();
            location.ItemPlacedHere(sword);

            _mockContext.Setup(c => c.CurrentLocation).Returns(location);
            _mockContext.Setup(c => c.Items).Returns(new List<IItem>());

            var intent = new ExitSubLocationIntent { NounOne = "sword" };

            // Act
            var result = await _engine.Process(intent, _mockContext.Object, _mockGenerationClient.Object);

            // Assert
            result.resultObject.Should().BeNull();
            result.ResultMessage.Should().Be("You cannot go that way. ");
        }

        [Test]
        public async Task Should_ReturnNotInMessage_When_NotInSubLocation()
        {
            // Arrange - boat is here but we're not in it
            var location = Repository.GetLocation<Shore>();
            var boat = Repository.GetItem<PileOfPlastic>();
            boat.IsInflated = true;
            location.ItemPlacedHere(boat);
            location.SubLocation = null; // NOT in the boat

            _mockContext.Setup(c => c.CurrentLocation).Returns(location);
            _mockContext.Setup(c => c.Items).Returns(new List<IItem>());

            var intent = new ExitSubLocationIntent { NounOne = "boat" };

            // Act
            var result = await _engine.Process(intent, _mockContext.Object, _mockGenerationClient.Object);

            // Assert
            result.resultObject.Should().BeNull();
            result.ResultMessage.Should().Contain("not in");
        }

        [Test]
        public async Task Should_CallGetOut_When_InSubLocation()
        {
            // Arrange - we're in the boat
            var location = Repository.GetLocation<Shore>();
            var boat = Repository.GetItem<PileOfPlastic>();
            boat.IsInflated = true;
            location.ItemPlacedHere(boat);
            location.SubLocation = boat; // IN the boat

            _mockContext.Setup(c => c.CurrentLocation).Returns(location);
            _mockContext.Setup(c => c.Items).Returns(new List<IItem>());

            var intent = new ExitSubLocationIntent { NounOne = "boat" };

            // Act
            var result = await _engine.Process(intent, _mockContext.Object, _mockGenerationClient.Object);

            // Assert
            result.resultObject.Should().BeNull();
            result.ResultMessage.Should().Contain("feet");
            location.SubLocation.Should().BeNull(); // No longer in boat
        }

        [Test]
        public async Task Should_PreventExit_When_OnRiver()
        {
            // Arrange - we're on the river in the boat
            var location = Repository.GetLocation<FrigidRiverFour>();
            var boat = Repository.GetItem<PileOfPlastic>();
            boat.IsInflated = true;
            location.ItemPlacedHere(boat);
            location.SubLocation = boat;

            _mockContext.Setup(c => c.CurrentLocation).Returns(location);
            _mockContext.Setup(c => c.Items).Returns(new List<IItem>());

            var intent = new ExitSubLocationIntent { NounOne = "boat" };

            // Act
            var result = await _engine.Process(intent, _mockContext.Object, _mockGenerationClient.Object);

            // Assert
            result.resultObject.Should().BeNull();
            result.ResultMessage.Should().Contain("fatal");
            location.SubLocation.Should().Be(boat); // Still in boat
        }
    }
}