using Model.Interface;
using Model.Item;
using Model.Movement;
using Model.Web;
using Planetfall.Item.Lawanda.Lab;

namespace UnitTests.Models;

[TestFixture]
public class GameResponseTests
{
    [Test]
    public void GameResponse_Constructor_ShouldSetAllProperties()
    {
        // Arrange
        var expectedResponse = "You see a forest.";
        var expectedLocationName = "Forest";
        var expectedMoves = 5;
        var expectedScore = 10;
        var expectedTime = 100;
        var expectedPreviousLocationName = "Cave";
        var expectedLastMovementDirection = "N";
        List<string> expectedInventory = new() { "sword", "lamp" };
        List<Direction> expectedExits = new() { Direction.N, Direction.S, Direction.E };
        var availablleActionsFromInventory = ApplicableVerbsAttribute.GetAvailableActions([new Lamp()]);
        var availablleActionsFromLocation = ApplicableVerbsAttribute.GetAvailableActions([new Lamp()]);

        // Act
        var gameResponse = new GameResponse(
            expectedResponse,
            expectedLocationName,
            expectedMoves,
            expectedScore,
            expectedTime,
            expectedPreviousLocationName,
            expectedLastMovementDirection,
            expectedInventory,
            expectedExits,
            availablleActionsFromLocation,
            availablleActionsFromInventory);

        // Assert
        gameResponse.Response.Should().Be(expectedResponse);
        gameResponse.LocationName.Should().Be(expectedLocationName);
        gameResponse.Moves.Should().Be(expectedMoves);
        gameResponse.Score.Should().Be(expectedScore);
        gameResponse.Time.Should().Be(expectedTime);
        gameResponse.PreviousLocationName.Should().Be(expectedPreviousLocationName);
        gameResponse.LastMovementDirection.Should().Be(expectedLastMovementDirection);
        gameResponse.Inventory.Should().BeEquivalentTo(expectedInventory);
        gameResponse.Exits.Should().BeEquivalentTo(expectedExits);
        gameResponse.ActionsAvailableFromInventory.Should().BeEquivalentTo(availablleActionsFromInventory);
        gameResponse.ActionsAvailableFromLocation.Should().BeEquivalentTo(availablleActionsFromLocation);
    }

    [Test]
    public void GameResponse_Constructor_WithNullPreviousLocationName_ShouldAcceptNullValue()
    {
        // Arrange
        var expectedResponse = "You see a forest.";
        var expectedLocationName = "Forest";
        var expectedMoves = 5;
        var expectedScore = 10;
        var expectedTime = 100;
        string? expectedPreviousLocationName = null;
        var expectedLastMovementDirection = "N";
        List<string> expectedInventory = new() { "sword", "lamp" };
        List<Direction> expectedExits = new() { Direction.N, Direction.S, Direction.E };
        var availablleActionsFromInventory = ApplicableVerbsAttribute.GetAvailableActions([new Lamp()]);
        var availablleActionsFromLocation = ApplicableVerbsAttribute.GetAvailableActions([new Lamp()]);

        // Act
        var gameResponse = new GameResponse(
            expectedResponse,
            expectedLocationName,
            expectedMoves,
            expectedScore,
            expectedTime,
            expectedPreviousLocationName,
            expectedLastMovementDirection,
            expectedInventory,
            expectedExits,
            availablleActionsFromInventory,
            availablleActionsFromLocation);

        // Assert
        gameResponse.PreviousLocationName.Should().BeNull();
    }

    [Test]
    public void GameResponse_Constructor_WithNullLastMovementDirection_ShouldAcceptNullValue()
    {
        // Arrange
        var expectedResponse = "You see a forest.";
        var expectedLocationName = "Forest";
        var expectedMoves = 5;
        var expectedScore = 10;
        var expectedTime = 100;
        var expectedPreviousLocationName = "Cave";
        string? expectedLastMovementDirection = null;
        List<string> expectedInventory = new() { "sword", "lamp" };
        List<Direction> expectedExits = new() { Direction.N, Direction.S, Direction.E };
        var availablleActionsFromInventory = ApplicableVerbsAttribute.GetAvailableActions([new Lamp()]);
        var availablleActionsFromLocation = ApplicableVerbsAttribute.GetAvailableActions([new Lamp()]);

        // Act
        var gameResponse = new GameResponse(
            expectedResponse,
            expectedLocationName,
            expectedMoves,
            expectedScore,
            expectedTime,
            expectedPreviousLocationName,
            expectedLastMovementDirection,
            expectedInventory,
            expectedExits,
            availablleActionsFromInventory,
            availablleActionsFromLocation);

        // Assert
        gameResponse.LastMovementDirection.Should().BeNull();
    }

    [Test]
    public void GameResponse_Constructor_WithEmptyInventory_ShouldSetEmptyList()
    {
        // Arrange
        var expectedResponse = "You see a forest.";
        var expectedLocationName = "Forest";
        var expectedMoves = 5;
        var expectedScore = 10;
        var expectedTime = 100;
        var expectedPreviousLocationName = "Cave";
        var expectedLastMovementDirection = "N";
        List<string> expectedInventory = new();
        List<Direction> expectedExits = new() { Direction.N, Direction.S, Direction.E };
        var availablleActionsFromInventory = ApplicableVerbsAttribute.GetAvailableActions([new Lamp()]);
        var availablleActionsFromLocation = ApplicableVerbsAttribute.GetAvailableActions([new Lamp()]);

        // Act
        var gameResponse = new GameResponse(
            expectedResponse,
            expectedLocationName,
            expectedMoves,
            expectedScore,
            expectedTime,
            expectedPreviousLocationName,
            expectedLastMovementDirection,
            expectedInventory,
            expectedExits,
            availablleActionsFromInventory,
            availablleActionsFromLocation);

        // Assert
        gameResponse.Inventory.Should().BeEmpty();
    }

    [Test]
    public void GameResponse_Constructor_WithEmptyExits_ShouldSetEmptyList()
    {
        // Arrange
        var expectedResponse = "You see a forest.";
        var expectedLocationName = "Forest";
        var expectedMoves = 5;
        var expectedScore = 10;
        var expectedTime = 100;
        var expectedPreviousLocationName = "Cave";
        var expectedLastMovementDirection = "N";
        List<string> expectedInventory = new() { "sword", "lamp" };
        List<Direction> expectedExits = new();
        var availablleActionsFromInventory = ApplicableVerbsAttribute.GetAvailableActions([new Lamp()]);
        var availablleActionsFromLocation = ApplicableVerbsAttribute.GetAvailableActions([new Lamp()]);

        // Act
        var gameResponse = new GameResponse(
            expectedResponse,
            expectedLocationName,
            expectedMoves,
            expectedScore,
            expectedTime,
            expectedPreviousLocationName,
            expectedLastMovementDirection,
            expectedInventory,
            expectedExits,
            availablleActionsFromInventory,
            availablleActionsFromLocation);

        // Assert
        gameResponse.Exits.Should().BeEmpty();
    }

    [Test]
    public void GameResponse_GameEngineConstructor_ShouldSetPropertiesFromGameEngine()
    {
        // Arrange
        var expectedResponse = "You see a forest.";
        var expectedLocationName = "Forest";
        var expectedMoves = 5;
        var expectedScore = 10;
        var expectedTime = 100;
        var expectedPreviousLocationName = "Cave";
        var expectedDirectionEnum = Direction.N;
        var expectedLastMovementDirection = "N";
        List<string> expectedInventory = new() { "sword", "lamp" };
        List<Direction> expectedExits = new() { Direction.N, Direction.S, Direction.E };
        var availablleActionsFromInventory = ApplicableVerbsAttribute.GetAvailableActions([new Lamp()]);
        var availablleActionsFromLocation = ApplicableVerbsAttribute.GetAvailableActions([new Lamp()]);

        var mockGameEngine = new Mock<IGameEngine>();
        mockGameEngine.Setup(ge => ge.LocationName).Returns(expectedLocationName);
        mockGameEngine.Setup(ge => ge.Moves).Returns(expectedMoves);
        mockGameEngine.Setup(ge => ge.Score).Returns(expectedScore);
        mockGameEngine.Setup(ge => ge.CurrentTime).Returns(expectedTime);
        mockGameEngine.Setup(ge => ge.PreviousLocationName).Returns(expectedPreviousLocationName);
        mockGameEngine.Setup(ge => ge.LastMovementDirection).Returns(expectedDirectionEnum);
        mockGameEngine.Setup(ge => ge.Inventory).Returns(expectedInventory);
        mockGameEngine.Setup(ge => ge.Exits).Returns(expectedExits);
        mockGameEngine.Setup(ge => ge.Context!.GetAvailableActionsForInventory())
            .Returns(availablleActionsFromInventory);
        mockGameEngine.Setup(ge => ge.Context!.CurrentLocation.GetAvailableActionsInLocation())
            .Returns(availablleActionsFromLocation);

        // Act
        var gameResponse = new GameResponse(expectedResponse, mockGameEngine.Object);

        // Assert
        gameResponse.Response.Should().Be(expectedResponse);
        gameResponse.LocationName.Should().Be(expectedLocationName);
        gameResponse.Moves.Should().Be(expectedMoves);
        gameResponse.Score.Should().Be(expectedScore);
        gameResponse.Time.Should().Be(expectedTime);
        gameResponse.PreviousLocationName.Should().Be(expectedPreviousLocationName);
        gameResponse.LastMovementDirection.Should().Be(expectedLastMovementDirection);
        gameResponse.Inventory.Should().BeEquivalentTo(expectedInventory);
        gameResponse.Exits.Should().BeEquivalentTo(expectedExits);
    }

    [Test]
    public void GameResponse_GameEngineConstructor_WhenDark_ShouldHideLocationExitsAndActions()
    {
        // Issue #238: in an unlit dark room the prose hides everything ("It is pitch black..."),
        // but the structured payload was still leaking the location's exits and action chips.
        // The location-derived fields must be empty in the dark, mirroring the engine's
        // existing "too dark to see" prose gating.
        var populatedExits = new List<Direction> { Direction.S, Direction.N };
        var locationActions = ApplicableVerbsAttribute.GetAvailableActions([new Lamp()]);
        var inventoryActions = ApplicableVerbsAttribute.GetAvailableActions([new Lamp()]);
        List<string> inventory = new() { "lamp" };

        var mockGameEngine = new Mock<IGameEngine>();
        mockGameEngine.Setup(ge => ge.LocationName).Returns("Cellar");
        mockGameEngine.Setup(ge => ge.Inventory).Returns(inventory);
        mockGameEngine.Setup(ge => ge.Exits).Returns(populatedExits);
        mockGameEngine.Setup(ge => ge.Context!.ItIsDarkHere).Returns(true);
        mockGameEngine.Setup(ge => ge.Context!.GetAvailableActionsForInventory()).Returns(inventoryActions);
        mockGameEngine.Setup(ge => ge.Context!.CurrentLocation.GetAvailableActionsInLocation())
            .Returns(locationActions);

        // Act
        var gameResponse = new GameResponse(
            "It is pitch black. You are likely to be eaten by a grue.", mockGameEngine.Object);

        // Assert — location-derived fields are hidden in the dark...
        gameResponse.Exits.Should().BeEmpty();
        gameResponse.ActionsAvailableFromLocation.Should().BeEmpty();
        // ...but inventory-derived fields remain (the player can still feel what they carry).
        gameResponse.Inventory.Should().BeEquivalentTo(inventory);
        gameResponse.ActionsAvailableFromInventory.Should().BeEquivalentTo(inventoryActions);
    }

    [Test]
    public void GameResponse_GameEngineConstructor_WhenLit_ShouldExposeLocationExitsAndActions()
    {
        // Control for issue #238: when the room is lit, the location-derived fields populate normally.
        var populatedExits = new List<Direction> { Direction.S, Direction.N };
        var locationActions = ApplicableVerbsAttribute.GetAvailableActions([new Lamp()]);
        var inventoryActions = ApplicableVerbsAttribute.GetAvailableActions([new Lamp()]);

        var mockGameEngine = new Mock<IGameEngine>();
        mockGameEngine.Setup(ge => ge.LocationName).Returns("Cellar");
        mockGameEngine.Setup(ge => ge.Inventory).Returns(new List<string> { "lamp" });
        mockGameEngine.Setup(ge => ge.Exits).Returns(populatedExits);
        mockGameEngine.Setup(ge => ge.Context!.ItIsDarkHere).Returns(false);
        mockGameEngine.Setup(ge => ge.Context!.GetAvailableActionsForInventory()).Returns(inventoryActions);
        mockGameEngine.Setup(ge => ge.Context!.CurrentLocation.GetAvailableActionsInLocation())
            .Returns(locationActions);

        // Act
        var gameResponse = new GameResponse("You are in the cellar.", mockGameEngine.Object);

        // Assert
        gameResponse.Exits.Should().BeEquivalentTo(populatedExits);
        gameResponse.ActionsAvailableFromLocation.Should().BeEquivalentTo(locationActions);
    }

    [Test]
    public void GameResponse_Equality_DifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var gameResponse1 = new GameResponse(
            "You see a forest.",
            "Forest",
            5,
            10,
            100,
            "Cave",
            "N",
            ["sword", "lamp"],
            [Direction.N, Direction.S, Direction.E], new Dictionary<string, List<string>>(), new Dictionary<string, List<string>>());

        var gameResponse2 = new GameResponse(
            "You see a mountain.", // Different response
            "Mountain", // Different location
            6, // Different moves
            15, // Different score
            120, // Different time
            "Forest", // Different previous location
            "S", // Different direction
            new List<string> { "potion" }, // Different inventory
            new List<Direction> { Direction.W, Direction.E }, new Dictionary<string, List<string>>() ,
            new Dictionary<string, List<string>>()); // Different exits

        // Act & Assert
        gameResponse1.Should().NotBeEquivalentTo(gameResponse2);
        gameResponse1.GetHashCode().Should().NotBe(gameResponse2.GetHashCode());
    }
}