using GameEngine;
using GameEngine.IntentEngine;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;
using Model.Item;
using ZorkOne.Interface;

namespace UnitTests.IntentEngine;

[TestFixture]
public class EnterSubLocationEngineTests
{
    [SetUp]
    public void SetUp()
    {
        Repository.Reset();
        _engine = new EnterSubLocationEngine();
        _mockContext = new Mock<IContext>();
        _mockGenerationClient = new Mock<IGenerationClient>();
        _mockGenerationClient.Setup(g => g.IsDisabled).Returns(true);
    }

    private EnterSubLocationEngine _engine = null!;
    private Mock<IContext> _mockContext = null!;
    private Mock<IGenerationClient> _mockGenerationClient = null!;

    [TestFixture]
    public class ProcessMethod : EnterSubLocationEngineTests
    {
        [Test]
        public async Task Should_ThrowArgumentException_When_IntentIsNotEnterSubLocationIntent()
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
        public async Task Should_ThrowArgumentException_When_NounIsNull()
        {
            // Arrange
            var intent = new EnterSubLocationIntent { Noun = null! };
            var location = Repository.GetLocation<WestOfHouse>();
            _mockContext.Setup(c => c.CurrentLocation).Returns(location);

            // Act & Assert
            await FluentActions.Invoking(async () =>
                    await _engine.Process(intent, _mockContext.Object, _mockGenerationClient.Object))
                .Should().ThrowAsync<ArgumentException>()
                .WithMessage("*null or empty noun*");
        }

        [Test]
        public async Task Should_ThrowArgumentException_When_NounIsEmpty()
        {
            // Arrange
            var intent = new EnterSubLocationIntent { Noun = "" };
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

            var intent = new EnterSubLocationIntent { Noun = "nonexistent" };

            // Act
            var result = await _engine.Process(intent, _mockContext.Object, _mockGenerationClient.Object);

            // Assert
            result.resultObject.Should().BeNull();
            result.ResultMessage.Should().Be("You cannot go that way. ");
        }

        [Test]
        public async Task Should_ReturnCantEnterThat_When_ItemIsNotSubLocationAndNotADoor()
        {
            // Arrange - sword is in scope but is neither a sub-location nor a door. You simply
            // can't enter it; say so plainly (issue #262) instead of a generic "can't go that way"
            // that the narrator would dress up as a mock of an imaginary place.
            var location = Repository.GetLocation<LivingRoom>();
            var sword = Repository.GetItem<Sword>();
            location.ItemPlacedHere(sword);

            _mockContext.Setup(c => c.CurrentLocation).Returns(location);
            _mockContext.Setup(c => c.Items).Returns(new List<IItem>());

            var intent = new EnterSubLocationIntent { Noun = "sword" };

            // Act
            var result = await _engine.Process(intent, _mockContext.Object, _mockGenerationClient.Object);

            // Assert
            result.resultObject.Should().BeNull();
            result.ResultMessage.Should().Be("You can't enter that. ");
        }

        [Test]
        public async Task Should_DeferToMovement_When_NounResolvesToAClosedDoor()
        {
            // issue #262: "enter <door>" with a valid door noun (here the kitchen window at Behind
            // House — a real IOpenAndClose item, NOT an ISubLocation) must defer to movement
            // (Direction.In) so the location map's door-open check and custom failure message apply,
            // exactly as "go in" does. Before the fix it fell through to a generic refusal, and the
            // narrator turned the perfectly valid noun into a mock of an imaginary object.
            var location = Repository.GetLocation<BehindHouse>();
            location.Init(); // places the KitchenWindow in the room
            Repository.GetItem<KitchenWindow>().IsOpen = false;

            _mockContext.Setup(c => c.CurrentLocation).Returns(location);
            _mockContext.Setup(c => c.Items).Returns(new List<IItem>());

            var intent = new EnterSubLocationIntent { Noun = "window" };

            // Act
            var result = await _engine.Process(intent, _mockContext.Object, _mockGenerationClient.Object);

            // Assert - the custom "closed door" failure message, not a generic "you cannot go that way"
            result.resultObject.Should().BeNull();
            result.ResultMessage.Should().Contain("The kitchen window is closed.");
            result.ResultMessage.Should().NotContain("cannot go that way");
        }

        [Test]
        public async Task Should_ReturnAlreadyInMessage_When_AlreadyInSubLocation()
        {
            // Arrange - PileOfPlastic (magic boat) is a sub-location
            var location = Repository.GetLocation<Shore>();
            var boat = Repository.GetItem<PileOfPlastic>();
            boat.IsInflated = true;
            location.ItemPlacedHere(boat);

            _mockContext.Setup(c => c.CurrentLocation).Returns(location);
            _mockContext.Setup(c => c.Items).Returns(new List<IItem>());
            _mockContext.Setup(c => c.GetItems<IAmPointyAndPunctureThings>())
                .Returns(new List<IAmPointyAndPunctureThings>());

            // Simulate being already in the boat
            location.SubLocation = boat;

            var intent = new EnterSubLocationIntent { Noun = "boat" };

            // Act
            var result = await _engine.Process(intent, _mockContext.Object, _mockGenerationClient.Object);

            // Assert
            result.resultObject.Should().BeNull();
            result.ResultMessage.Should().Contain("already");
        }

        [Test]
        public async Task Should_CallGetIn_When_ValidSubLocation()
        {
            // Arrange - PileOfPlastic (magic boat) is a sub-location
            var location = Repository.GetLocation<Shore>();
            var boat = Repository.GetItem<PileOfPlastic>();
            boat.IsInflated = true;
            location.ItemPlacedHere(boat);
            location.SubLocation = null; // Not in the boat yet

            _mockContext.Setup(c => c.CurrentLocation).Returns(location);
            _mockContext.Setup(c => c.Items).Returns(new List<IItem>());
            _mockContext.Setup(c => c.GetItems<IAmPointyAndPunctureThings>())
                .Returns(new List<IAmPointyAndPunctureThings>());

            var intent = new EnterSubLocationIntent { Noun = "boat" };

            // Act
            var result = await _engine.Process(intent, _mockContext.Object, _mockGenerationClient.Object);

            // Assert
            result.resultObject.Should().BeNull();
            result.ResultMessage.Should().Contain("magic boat");
        }

        [Test]
        public async Task Should_PunctureBoat_When_EnteringWithPointyItem()
        {
            // Arrange - PileOfPlastic (magic boat) is a sub-location
            var location = Repository.GetLocation<Shore>();
            var boat = Repository.GetItem<PileOfPlastic>();
            boat.IsInflated = true;
            boat.IsPunctured = false;
            location.ItemPlacedHere(boat);
            location.SubLocation = null;

            var sword = Repository.GetItem<Sword>();
            _mockContext.Setup(c => c.CurrentLocation).Returns(location);
            _mockContext.Setup(c => c.Items).Returns(new List<IItem> { sword });
            _mockContext.Setup(c => c.GetItems<IAmPointyAndPunctureThings>())
                .Returns(new List<IAmPointyAndPunctureThings> { sword });

            var intent = new EnterSubLocationIntent { Noun = "boat" };

            // Act
            var result = await _engine.Process(intent, _mockContext.Object, _mockGenerationClient.Object);

            // Assert
            result.resultObject.Should().BeNull();
            result.ResultMessage.Should().Contain("punctured");
            boat.IsPunctured.Should().BeTrue();
        }

        [Test]
        public async Task Should_FailToEnter_When_BoatNotInflated()
        {
            // Arrange
            var location = Repository.GetLocation<Shore>();
            var boat = Repository.GetItem<PileOfPlastic>();
            boat.IsInflated = false;
            location.ItemPlacedHere(boat);

            _mockContext.Setup(c => c.CurrentLocation).Returns(location);
            _mockContext.Setup(c => c.Items).Returns(new List<IItem>());

            var intent = new EnterSubLocationIntent { Noun = "plastic" };

            // Act
            var result = await _engine.Process(intent, _mockContext.Object, _mockGenerationClient.Object);

            // Assert
            result.resultObject.Should().BeNull();
            result.ResultMessage.Should().Contain("pile of plastic");
        }
    }
}