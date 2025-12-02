using GameEngine;
using Model.AIGeneration;
using Model.Intent;
using Model.Interaction;
using Model.Interface;
using Model.Item;
using Model.Location;
using ZorkOne;

namespace UnitTests;

[TestFixture]
public class ContextTests
{
    [SetUp]
    public void SetUp()
    {
        // Reset Repository for clean test state
        Repository.Reset();

        _mockLocation = new Mock<ILocation>();
        _mockLocation.As<ICanContainItems>(); // Setup interface before accessing Object
        _mockItem = new Mock<IItem>();
        _mockGenerationClient = new Mock<IGenerationClient>();
        _mockItemProcessorFactory = new Mock<IItemProcessorFactory>();

        _context = new TestableContext();
        _context.CurrentLocation = _mockLocation.Object;
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up any static state
        Repository.Reset();
    }

    private TestableContext _context;
    private Mock<ILocation> _mockLocation;
    private Mock<IItem> _mockItem;
    private Mock<IGenerationClient> _mockGenerationClient;
    private Mock<IItemProcessorFactory> _mockItemProcessorFactory;

    [TestFixture]
    public class ConstructorTests : ContextTests
    {
        [Test]
        public void Should_InitializeWithDefaultValues_When_UsingParameterlessConstructor()
        {
            // Act
            var context = new TestableContext();

            // Assert
            context.Score.Should().Be(0);
            context.Moves.Should().Be(0);
            context.Verbosity.Should().Be(Verbosity.SuperBrief); // ZorkI context defaults to SuperBrief
            context.LastNoun.Should().Be("");
            context.Items.Should().BeEmpty();
            context.Actors.Should().BeEmpty();
            context.CurrentLocation.Should().NotBeNull();
        }
    }

    [TestFixture]
    public class InventoryManagement : ContextTests
    {
        [Test]
        public void Should_AddItemToInventory_When_TakeItem()
        {
            // Arrange
            _mockItem.Setup(i => i.HasEverBeenPickedUp).Returns(false);
            _mockItem.Setup(i => i.OnBeingTakenCallback).Returns("");

            // Act
            _context.Take(_mockItem.Object);

            // Assert
            _context.Items.Should().Contain(_mockItem.Object);
            _mockItem.VerifySet(i => i.CurrentLocation = _context, Times.Once);
            _mockItem.VerifySet(i => i.HasEverBeenPickedUp = true, Times.Once);
        }

        [Test]
        public void Should_ThrowException_When_TakeNullItem()
        {
            // Act & Assert
            FluentActions.Invoking(() => _context.Take(null))
                .Should().Throw<Exception>()
                .WithMessage("Null item was added to inventory");
        }

        [Test]
        public void Should_RemoveItemFromPreviousLocation_When_TakeItem()
        {
            // Arrange
            var mockPreviousLocation = new Mock<ICanContainItems>();
            _mockItem.Setup(i => i.CurrentLocation).Returns(mockPreviousLocation.Object);
            _mockItem.Setup(i => i.OnBeingTakenCallback).Returns("");

            // Act
            _context.Take(_mockItem.Object);

            // Assert
            mockPreviousLocation.Verify(l => l.RemoveItem(_mockItem.Object), Times.Once);
        }

        [Test]
        public void Should_AddPoints_When_TakeItemWithPoints()
        {
            // Arrange
            var mockPointsItem = new Mock<IItem>();
            mockPointsItem.As<IGivePointsWhenFirstPickedUp>().Setup(p => p.NumberOfPoints).Returns(10);
            mockPointsItem.Setup(i => i.HasEverBeenPickedUp).Returns(false);
            mockPointsItem.Setup(i => i.OnBeingTakenCallback).Returns("");

            // Act
            _context.Take(mockPointsItem.Object);

            // Assert
            _context.Score.Should().Be(10);
        }


        [Test]
        public void Should_ThrowException_When_DropNullItem()
        {
            // Act & Assert
            FluentActions.Invoking(() => _context.Drop(null!))
                .Should().Throw<Exception>()
                .WithMessage("Null item was dropped from inventory");
        }

        [Test]
        public void Should_ThrowException_When_DropToLocationThatCannotHoldItems()
        {
            // Arrange
            var mockBadLocation = new Mock<ILocation>(); // Doesn't implement ICanContainItems
            _context.CurrentLocation = mockBadLocation.Object;
            _context.Items.Add(_mockItem.Object);

            // Act & Assert
            FluentActions.Invoking(() => _context.Drop(_mockItem.Object))
                .Should().Throw<Exception>()
                .WithMessage("Current location can't hold item");
        }

        [Test]
        public void Should_CalculateCarryingWeight_When_HasItems()
        {
            // Arrange
            var item1 = new Mock<IItem>();
            item1.Setup(i => i.Size).Returns(5);
            var item2 = new Mock<IItem>();
            item2.Setup(i => i.Size).Returns(3);

            _context.Items.Add(item1.Object);
            _context.Items.Add(item2.Object);

            // Act & Assert
            _context.CarryingWeight.Should().Be(8);
        }
    }

    [TestFixture]
    public class ItemSearching : ContextTests
    {
        [Test]
        public void Should_ReturnTrue_When_HasItemOfType()
        {
            // Arrange
            var specificItem = new TestSpecificItem();
            _context.Items.Add(specificItem);

            // Act & Assert
            _context.HasItem<TestSpecificItem>().Should().BeTrue();
        }

        [Test]
        public void Should_ReturnFalse_When_DoesNotHaveItemOfType()
        {
            // Act & Assert
            _context.HasItem<TestSpecificItem>().Should().BeFalse();
        }

        [Test]
        public void Should_FindItemByNoun_When_ItemExists()
        {
            // Arrange
            _mockItem.Setup(i => i.HasMatchingNoun("sword", It.IsAny<bool>()))
                .Returns((true, _mockItem.Object));
            _context.Items.Add(_mockItem.Object);

            // Act
            var result = _context.HasMatchingNoun("sword");

            // Assert
            result.HasItem.Should().BeTrue();
            result.TheItem.Should().Be(_mockItem.Object);
        }

        [Test]
        public void Should_ReturnFalse_When_ItemNotFoundByNoun()
        {
            // Arrange
            _mockItem.Setup(i => i.HasMatchingNoun("sword", It.IsAny<bool>()))
                .Returns((false, null));
            _context.Items.Add(_mockItem.Object);

            // Act
            var result = _context.HasMatchingNoun("sword");

            // Assert
            result.HasItem.Should().BeFalse();
            result.TheItem.Should().BeNull();
        }

        [Test]
        public void Should_FindItemByNounAndAdjective_When_ItemExists()
        {
            // Arrange
            _mockItem.Setup(i => i.HasMatchingNounAndAdjective("sword", "sharp", It.IsAny<bool>()))
                .Returns((true, _mockItem.Object));
            _context.Items.Add(_mockItem.Object);

            // Act
            var result = _context.HasMatchingNounAndAdjective("sword", "sharp");

            // Assert
            result.HasItem.Should().BeTrue();
            result.TheItem.Should().Be(_mockItem.Object);
        }

        [Test]
        public void Should_FallbackToNounOnly_When_AdjectiveIsEmpty()
        {
            // Arrange
            _mockItem.Setup(i => i.HasMatchingNoun("sword", It.IsAny<bool>()))
                .Returns((true, _mockItem.Object));
            _context.Items.Add(_mockItem.Object);

            // Act
            var result = _context.HasMatchingNounAndAdjective("sword", "");

            // Assert
            result.HasItem.Should().BeTrue();
            result.TheItem.Should().Be(_mockItem.Object);
        }
    }


    [TestFixture]
    public class DarknessDetection : ContextTests
    {
        [Test]
        public void Should_ReturnTrue_When_InDarkLocationWithoutLightSource()
        {
            // Arrange
            var darkLocation = new Mock<ILocation>();
            darkLocation.As<IDarkLocation>().Setup(d => d.IsNoLongerDark).Returns(false);
            darkLocation.As<ICanContainItems>().Setup(l => l.Items).Returns(new List<IItem>());
            _context.CurrentLocation = darkLocation.Object;

            // Act & Assert
            _context.ItIsDarkHere.Should().BeTrue();
        }

        [Test]
        public void Should_ReturnFalse_When_InDarkLocationWithLightSource()
        {
            // Arrange
            var darkLocation = new Mock<ILocation>();
            darkLocation.As<IDarkLocation>().Setup(d => d.IsNoLongerDark).Returns(false);
            darkLocation.As<ICanContainItems>().Setup(l => l.Items).Returns(new List<IItem>());

            var lightSource = new Mock<IItem>();
            lightSource.As<IAmALightSource>();
            lightSource.As<ICannotBeTurnedOff>();
            _context.Items.Add(lightSource.Object);
            _context.CurrentLocation = darkLocation.Object;

            // Act & Assert
            _context.ItIsDarkHere.Should().BeFalse();
        }

        [Test]
        public void Should_ReturnFalse_When_NotInDarkLocation()
        {
            // Arrange - _mockLocation doesn't implement IDarkLocation
            var mockLocationAsContainer = _mockLocation.As<ICanContainItems>();
            mockLocationAsContainer.Setup(l => l.Items).Returns(new List<IItem>());

            // Act & Assert
            _context.ItIsDarkHere.Should().BeFalse();
        }
    }

    [TestFixture]
    public class ScoringAndGameState : ContextTests
    {
        [Test]
        public void Should_AddPointsToScore_When_AddPoints()
        {
            // Arrange
            _context.Score = 50;

            // Act
            var newScore = _context.AddPoints(25);

            // Assert
            newScore.Should().Be(75);
            _context.Score.Should().Be(75);
        }

        [Test]
        public void Should_IncrementMoves_When_ProcessBeginningOfTurn()
        {
            // Arrange
            _context.Moves = 5;

            // Act
            _context.ProcessBeginningOfTurn();

            // Assert
            _context.Moves.Should().Be(6);
        }

        [Test]
        public void Should_ReturnEmptyHanded_When_NoItems()
        {
            // Act
            var description = _context.ItemListDescription("test", _mockLocation.Object);

            // Assert
            description.Should().Be("You are empty-handed");
        }

        [Test]
        public void Should_ReturnItemList_When_HasItems()
        {
            // Arrange
            _mockItem.Setup(i => i.GenericDescription(_context.CurrentLocation)).Returns("a sword");
            _context.Items.Add(_mockItem.Object);

            // Act
            var description = _context.ItemListDescription("test", _mockLocation.Object);

            // Assert
            description.Should().Contain("You are carrying:");
            description.Should().Contain("a sword");
        }
    }

    [TestFixture]
    public class ActorManagement : ContextTests
    {
        [Test]
        public void Should_AddActor_When_RegisterActor()
        {
            // Arrange
            var mockActor = new Mock<ITurnBasedActor>();

            // Act
            _context.RegisterActor(mockActor.Object);

            // Assert
            _context.Actors.Should().Contain(mockActor.Object);
        }

        [Test]
        public void Should_NotAddDuplicate_When_RegisterSameActorTwice()
        {
            // Arrange
            var mockActor = new Mock<ITurnBasedActor>();

            // Act
            _context.RegisterActor(mockActor.Object);
            _context.RegisterActor(mockActor.Object);

            // Assert
            _context.Actors.Should().HaveCount(1);
            _context.Actors.Should().Contain(mockActor.Object);
        }

        [Test]
        public void Should_RemoveActor_When_RemoveActor()
        {
            // Arrange
            var mockActor = new Mock<ITurnBasedActor>();
            _context.Actors.Add(mockActor.Object);

            // Act
            _context.RemoveActor(mockActor.Object);

            // Assert
            _context.Actors.Should().NotContain(mockActor.Object);
        }
    }

    [TestFixture]
    public class SimpleInteractionTests : ContextTests
    {
        [Test]
        public async Task Should_ReturnNoNounMatch_When_NoItemsInInventory()
        {
            // Arrange
            var intent = new SimpleIntent { Verb = "use", Noun = "sword" };

            // Act
            var result = await _context.RespondToSimpleInteraction(intent, _mockGenerationClient.Object,
                _mockItemProcessorFactory.Object);

            // Assert
            result.Should().BeOfType<NoNounMatchInteractionResult>();
        }

        [Test]
        public async Task Should_CallItemInteraction_When_ItemExists()
        {
            // Arrange
            var intent = new SimpleIntent { Verb = "use", Noun = "sword" };
            var expectedResult = new PositiveInteractionResult("Sword used");

            _mockItem.Setup(i => i.RespondToSimpleInteraction(intent, _context, _mockGenerationClient.Object,
                    _mockItemProcessorFactory.Object))
                .ReturnsAsync(expectedResult);
            _context.Items.Add(_mockItem.Object);

            // Act
            var result = await _context.RespondToSimpleInteraction(intent, _mockGenerationClient.Object,
                _mockItemProcessorFactory.Object);

            // Assert
            result.Should().Be(expectedResult);
        }
    }
}

// Test helper classes - using existing game items to avoid interface implementation complexity
public class TestSpecificItem : Sword // Use existing Sword class
{
}

public class TestableContext : ZorkIContext // Use existing ZorkI context
{
    public TestableContext()
    {
        // Initialize with test state
        Score = 0;
        Moves = 0;
    }
}