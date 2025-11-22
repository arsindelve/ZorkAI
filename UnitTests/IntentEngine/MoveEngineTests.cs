using System.Reflection;
using GameEngine;
using GameEngine.IntentEngine;
using JetBrains.Annotations;
using Model.AIGeneration;
using Model.AIGeneration.Requests;
using Model.Intent;
using Model.Interface;
using Model.Location;
using Model.Movement;

namespace UnitTests.IntentEngine;

[TestFixture]
public class MoveEngineTests
{
    [SetUp]
    public void SetUp()
    {
        _moveEngine = new MoveEngine();
        _mockContext = new Mock<IContext>();
        _mockGenerationClient = new Mock<IGenerationClient>();
        _mockCurrentLocation = new Mock<ILocation>();
        _mockDestinationLocation = new Mock<ILocation>();
        _mockRandomChooser = new Mock<IRandomChooser>();

        _mockContext.Setup(c => c.CurrentLocation).Returns(_mockCurrentLocation.Object);
        _mockContext.Setup(c => c.CarryingWeight).Returns(0);
        _mockContext.SetupProperty(c => c.LastNoun);
    }

    private MoveEngine _moveEngine;
    private Mock<IContext> _mockContext;
    private Mock<IGenerationClient> _mockGenerationClient;
    private Mock<ILocation> _mockCurrentLocation;
    private Mock<ILocation> _mockDestinationLocation;
    private Mock<IRandomChooser> _mockRandomChooser;

    [TestFixture]
    public class ProcessMethod : MoveEngineTests
    {
        [Test]
        public async Task Should_ThrowArgumentException_When_IntentIsNotMoveIntent()
        {
            // Arrange
            var wrongIntent = new SimpleIntent { Verb = "take", Noun = "sword" };

            // Act & Assert
            await FluentActions.Invoking(async () =>
                    await _moveEngine.Process(wrongIntent, _mockContext.Object, _mockGenerationClient.Object))
                .Should().ThrowAsync<ArgumentException>()
                .WithMessage("Cast error");
        }

        [Test]
        public async Task Should_SetLastMovementDirection_When_Processing()
        {
            // Arrange
            var moveIntent = new MoveIntent { Direction = Direction.N };
            var movement = new MovementParameters
            {
                Location = _mockDestinationLocation.Object,
                WeightLimit = 100,
                CanGo = _ => true
            };

            _mockCurrentLocation.Setup(l => l.Navigate(Direction.N, _mockContext.Object))
                .Returns(movement);
            _mockCurrentLocation.Setup(l =>
                l.OnLeaveLocation(_mockContext.Object, _mockDestinationLocation.Object, _mockCurrentLocation.Object));
            _mockDestinationLocation.Setup(l => l.BeforeEnterLocation(_mockContext.Object, _mockCurrentLocation.Object))
                .Returns("");
            _mockDestinationLocation.Setup(l =>
                    l.AfterEnterLocation(_mockContext.Object, _mockCurrentLocation.Object,
                        _mockGenerationClient.Object))
                .ReturnsAsync("");

            // Act
            await _moveEngine.Process(moveIntent, _mockContext.Object, _mockGenerationClient.Object);

            // Assert
            _mockContext.VerifySet(c => c.LastMovementDirection = Direction.N, Times.Once);
        }


        [Test]
        public async Task Should_ReturnWeightLimitMessage_When_CarryingTooMuch()
        {
            // Arrange
            var moveIntent = new MoveIntent { Direction = Direction.N };
            var movement = new MovementParameters
            {
                Location = _mockDestinationLocation.Object,
                WeightLimit = 50,
                WeightLimitFailureMessage = "You are carrying too much to go that way."
            };

            _mockCurrentLocation.Setup(l => l.Navigate(Direction.N, _mockContext.Object))
                .Returns(movement);
            _mockContext.Setup(c => c.CarryingWeight).Returns(75);

            // Act
            var result = await _moveEngine.Process(moveIntent, _mockContext.Object, _mockGenerationClient.Object);

            // Assert
            result.resultObject.Should().BeNull();
            result.ResultMessage.Should().Be("You are carrying too much to go that way.");
        }

        [Test]
        public async Task Should_ReturnCustomFailureMessage_When_CannotGo()
        {
            // Arrange
            var moveIntent = new MoveIntent { Direction = Direction.N };
            var movement = new MovementParameters
            {
                Location = _mockDestinationLocation.Object,
                WeightLimit = 100,
                CanGo = _ => false,
                CustomFailureMessage = "The door is locked."
            };

            _mockCurrentLocation.Setup(l => l.Navigate(Direction.N, _mockContext.Object))
                .Returns(movement);

            // Act
            var result = await _moveEngine.Process(moveIntent, _mockContext.Object, _mockGenerationClient.Object);

            // Assert
            result.resultObject.Should().BeNull();
            result.ResultMessage.Should().Be("The door is locked." + Environment.NewLine);
        }


    }

    [TestFixture]
    public class GoMethod : MoveEngineTests
    {
        [Test]
        public async Task Should_PerformCompleteLocationTransition()
        {
            // Arrange
            var movement = new MovementParameters
            {
                Location = _mockDestinationLocation.Object
            };

            var beforeText = "Moving north...\n";
            var afterText = "Something happens.\n";

            _mockCurrentLocation.Setup(l =>
                l.OnLeaveLocation(_mockContext.Object, _mockDestinationLocation.Object, _mockCurrentLocation.Object));
            _mockDestinationLocation.Setup(l => l.BeforeEnterLocation(_mockContext.Object, _mockCurrentLocation.Object))
                .Returns(beforeText);
            _mockDestinationLocation.Setup(l =>
                    l.AfterEnterLocation(_mockContext.Object, _mockCurrentLocation.Object,
                        _mockGenerationClient.Object))
                .ReturnsAsync(afterText);

            // Act
            var result = await MoveEngine.Go(_mockContext.Object, _mockGenerationClient.Object, movement);

            // Assert
            result.Should().Contain(beforeText);
            result.Should().Contain(afterText);
            result.Should().EndWith(Environment.NewLine);

            _mockContext.VerifySet(c => c.CurrentLocation = _mockDestinationLocation.Object, Times.Once);
            _mockCurrentLocation.Verify(
                l => l.OnLeaveLocation(_mockContext.Object, _mockDestinationLocation.Object,
                    _mockCurrentLocation.Object), Times.Once);
            _mockDestinationLocation.Verify(
                l => l.BeforeEnterLocation(_mockContext.Object, _mockCurrentLocation.Object), Times.Once);
            _mockDestinationLocation.Verify(
                l => l.AfterEnterLocation(_mockContext.Object, _mockCurrentLocation.Object,
                    _mockGenerationClient.Object), Times.Once);
        }
    }

    [TestFixture]
    public class GetGeneratedCantGoThatWayResponseMethod : MoveEngineTests
    {
        [Test]
        public async Task Should_ReturnStandardMessage_When_RandomChooserReturnsFalse()
        {
            // Arrange
            var moveEngineWithMock = new TestableMoveEngine(_mockRandomChooser.Object);
            _mockRandomChooser.Setup(r => r.RollDiceSuccess(5)).Returns(false);

            // Use reflection to call the private method
            var method = typeof(MoveEngine).GetMethod("GetGeneratedCantGoThatWayResponse",
                BindingFlags.NonPublic | BindingFlags.Instance);

            // Act
            var result = await (Task<string>)method!.Invoke(moveEngineWithMock,
                new object[] { _mockGenerationClient.Object, "north", _mockContext.Object })!;

            // Assert
            result.Should().Be("You cannot go that way. ");
            _mockGenerationClient.Verify(
                g => g.GenerateNarration(It.IsAny<CannotGoThatWayRequest>(), It.IsAny<string>()), Times.Never);
        }

    }

    [TestFixture]
    public class ChooserProperty : MoveEngineTests
    {
        [Test]
        public void Should_ReturnRandomChooserInstance()
        {
            // Act
            var chooser = _moveEngine.Chooser;

            // Assert
            chooser.Should().NotBeNull();
            chooser.Should().BeOfType<RandomChooser>();
        }
    }

    [TestFixture]
    public class DarkLocationDeathMechanic : MoveEngineTests
    {
        [Test]
        public async Task Should_NotTriggerDeath_When_MovingFromLightToDark()
        {
            // Arrange
            var moveEngineWithMock = new TestableMoveEngine(_mockRandomChooser.Object);
            var moveIntent = new MoveIntent { Direction = Direction.N };
            var movement = new MovementParameters
            {
                Location = _mockDestinationLocation.Object,
                WeightLimit = 100,
                CanGo = _ => true
            };

            // Setup: Currently in a lit location, moving to a dark location
            _mockContext.Setup(c => c.ItIsDarkHere).Returns(false);
            _mockCurrentLocation.Setup(l => l.Navigate(Direction.N, _mockContext.Object)).Returns(movement);
            _mockCurrentLocation.Setup(l =>
                l.OnLeaveLocation(_mockContext.Object, _mockDestinationLocation.Object, _mockCurrentLocation.Object));
            _mockDestinationLocation.Setup(l => l.BeforeEnterLocation(_mockContext.Object, _mockCurrentLocation.Object))
                .Returns("");
            _mockDestinationLocation.Setup(l =>
                    l.AfterEnterLocation(_mockContext.Object, _mockCurrentLocation.Object,
                        _mockGenerationClient.Object))
                .ReturnsAsync("");

            // Act
            var result = await moveEngineWithMock.Process(moveIntent, _mockContext.Object, _mockGenerationClient.Object);

            // Assert - should not trigger death (result.resultObject should be null)
            result.resultObject.Should().BeNull();
        }

        [Test]
        public async Task Should_NotTriggerDeath_When_MovingFromDarkToLight()
        {
            // Arrange
            var moveEngineWithMock = new TestableMoveEngine(_mockRandomChooser.Object);
            var moveIntent = new MoveIntent { Direction = Direction.N };
            var movement = new MovementParameters
            {
                Location = _mockDestinationLocation.Object,
                WeightLimit = 100,
                CanGo = _ => true
            };

            // Setup: Start in dark, but after moving, we're in light
            var darkFirst = true;
            _mockContext.Setup(c => c.ItIsDarkHere).Returns(() =>
            {
                if (darkFirst)
                {
                    darkFirst = false;
                    return true;
                }

                return false;
            });

            _mockCurrentLocation.Setup(l => l.Navigate(Direction.N, _mockContext.Object)).Returns(movement);
            _mockCurrentLocation.Setup(l =>
                l.OnLeaveLocation(_mockContext.Object, _mockDestinationLocation.Object, _mockCurrentLocation.Object));
            _mockDestinationLocation.Setup(l => l.BeforeEnterLocation(_mockContext.Object, _mockCurrentLocation.Object))
                .Returns("");
            _mockDestinationLocation.Setup(l =>
                    l.AfterEnterLocation(_mockContext.Object, _mockCurrentLocation.Object,
                        _mockGenerationClient.Object))
                .ReturnsAsync("");

            // Act
            var result = await moveEngineWithMock.Process(moveIntent, _mockContext.Object, _mockGenerationClient.Object);

            // Assert - should not trigger death
            result.resultObject.Should().BeNull();
        }

        [Test]
        public async Task Should_NotTriggerDeath_When_MovingDarkToDarkButRandomCheckFails()
        {
            // Arrange
            var moveEngineWithMock = new TestableMoveEngine(_mockRandomChooser.Object);
            var moveIntent = new MoveIntent { Direction = Direction.N };
            var movement = new MovementParameters
            {
                Location = _mockDestinationLocation.Object,
                WeightLimit = 100,
                CanGo = _ => true
            };

            // Setup: Both locations dark, but random check fails (no death)
            _mockContext.Setup(c => c.ItIsDarkHere).Returns(true);
            _mockRandomChooser.Setup(r => r.RollDiceSuccess(2)).Returns(false);
            _mockCurrentLocation.Setup(l => l.Navigate(Direction.N, _mockContext.Object)).Returns(movement);
            _mockCurrentLocation.Setup(l =>
                l.OnLeaveLocation(_mockContext.Object, _mockDestinationLocation.Object, _mockCurrentLocation.Object));
            _mockDestinationLocation.Setup(l => l.BeforeEnterLocation(_mockContext.Object, _mockCurrentLocation.Object))
                .Returns("");
            _mockDestinationLocation.Setup(l =>
                    l.AfterEnterLocation(_mockContext.Object, _mockCurrentLocation.Object,
                        _mockGenerationClient.Object))
                .ReturnsAsync("");

            // Act
            var result = await moveEngineWithMock.Process(moveIntent, _mockContext.Object, _mockGenerationClient.Object);

            // Assert - should not trigger death because random check failed
            result.resultObject.Should().BeNull();
        }

        [Test]
        public async Task Should_TriggerDeathCheck_When_MovingDarkToDarkAndRandomCheckSucceeds()
        {
            // Arrange
            var moveEngineWithMock = new TestableMoveEngine(_mockRandomChooser.Object);
            var moveIntent = new MoveIntent { Direction = Direction.N };
            var movement = new MovementParameters
            {
                Location = _mockDestinationLocation.Object,
                WeightLimit = 100,
                CanGo = _ => true
            };

            // Setup: Both locations dark, and random check succeeds
            _mockContext.Setup(c => c.ItIsDarkHere).Returns(true);
            _mockRandomChooser.Setup(r => r.RollDiceSuccess(2)).Returns(true);
            _mockCurrentLocation.Setup(l => l.Navigate(Direction.N, _mockContext.Object)).Returns(movement);
            _mockCurrentLocation.Setup(l =>
                l.OnLeaveLocation(_mockContext.Object, _mockDestinationLocation.Object, _mockCurrentLocation.Object));
            _mockDestinationLocation.Setup(l => l.BeforeEnterLocation(_mockContext.Object, _mockCurrentLocation.Object))
                .Returns("");
            _mockDestinationLocation.Setup(l =>
                    l.AfterEnterLocation(_mockContext.Object, _mockCurrentLocation.Object,
                        _mockGenerationClient.Object))
                .ReturnsAsync("");

            // Setup a minimal Game mock
            var mockGame = new Mock<IInfocomGame>();
            mockGame.Setup(g => g.GameName).Returns("UnknownGame");
            _mockContext.Setup(c => c.Game).Returns(mockGame.Object);

            // Act
            var result = await moveEngineWithMock.Process(moveIntent, _mockContext.Object, _mockGenerationClient.Object);

            // Assert - HandleDarkLocationDeath is called and attempts to find DeathProcessor
            // Since there's no DeathProcessor for "UnknownGame", result.resultObject should be null
            // but the code path was exercised
            result.resultObject.Should().BeNull();
            _mockRandomChooser.Verify(r => r.RollDiceSuccess(2), Times.Once);
        }
    }

    // Test helper class to allow mocking of the internal Chooser property
    private class TestableMoveEngine(IRandomChooser chooser) : MoveEngine
    {
        [UsedImplicitly]
        internal override IRandomChooser Chooser => chooser;
    }
}