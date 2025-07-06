using Model.Interface;
using Model.Movement;
using Model.Web;

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
            expectedExits);

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
            expectedExits);

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
            expectedExits);

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
            expectedExits);

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
            expectedExits);

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

        var mockGameEngine = new Mock<IGameEngine>();
        mockGameEngine.Setup(ge => ge.LocationName).Returns(expectedLocationName);
        mockGameEngine.Setup(ge => ge.Moves).Returns(expectedMoves);
        mockGameEngine.Setup(ge => ge.Score).Returns(expectedScore);
        mockGameEngine.Setup(ge => ge.CurrentTime).Returns(expectedTime);
        mockGameEngine.Setup(ge => ge.PreviousLocationName).Returns(expectedPreviousLocationName);
        mockGameEngine.Setup(ge => ge.LastMovementDirection).Returns(expectedDirectionEnum);
        mockGameEngine.Setup(ge => ge.Inventory).Returns(expectedInventory);
        mockGameEngine.Setup(ge => ge.Exits).Returns(expectedExits);

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
            new List<string> { "sword", "lamp" },
            new List<Direction> { Direction.N, Direction.S, Direction.E });

        var gameResponse2 = new GameResponse(
            "You see a mountain.", // Different response
            "Mountain", // Different location
            6, // Different moves
            15, // Different score
            120, // Different time
            "Forest", // Different previous location
            "S", // Different direction
            new List<string> { "potion" }, // Different inventory
            new List<Direction> { Direction.W, Direction.E }); // Different exits

        // Act & Assert
        gameResponse1.Should().NotBeEquivalentTo(gameResponse2);
        gameResponse1.GetHashCode().Should().NotBe(gameResponse2.GetHashCode());
    }
}