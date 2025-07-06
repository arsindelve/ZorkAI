using FluentAssertions;
using Model.Web;
using NUnit.Framework;

namespace UnitTests.Models;

[TestFixture]
public class GameRequestTests
{
    [Test]
    public void GameRequest_Constructor_ShouldSetProperties()
    {
        // Arrange
        var expectedInput = "look";
        var expectedSessionId = "abc123";

        // Act
        var gameRequest = new GameRequest(expectedInput, expectedSessionId);

        // Assert
        gameRequest.Input.Should().Be(expectedInput);
        gameRequest.SessionId.Should().Be(expectedSessionId);
    }

    [Test]
    public void GameRequest_WithEmptyInput_ShouldSetEmptyString()
    {
        // Arrange
        var expectedInput = "";
        var expectedSessionId = "session456";

        // Act
        var gameRequest = new GameRequest(expectedInput, expectedSessionId);

        // Assert
        gameRequest.Input.Should().BeEmpty();
        gameRequest.SessionId.Should().Be(expectedSessionId);
    }

    [Test]
    public void GameRequest_WithEmptySessionId_ShouldSetEmptyString()
    {
        // Arrange
        var expectedInput = "inventory";
        var expectedSessionId = "";

        // Act
        var gameRequest = new GameRequest(expectedInput, expectedSessionId);

        // Assert
        gameRequest.Input.Should().Be(expectedInput);
        gameRequest.SessionId.Should().BeEmpty();
    }

    [Test]
    public void GameRequest_WithNullInput_ShouldAcceptNullValue()
    {
        // Arrange
        string? input = null;
        var sessionId = "session789";

        // Act
        var gameRequest = new GameRequest(input!, sessionId);

        // Assert
        gameRequest.Input.Should().BeNull();
        gameRequest.SessionId.Should().Be(sessionId);
    }

    [Test]
    public void GameRequest_WithNullSessionId_ShouldAcceptNullValue()
    {
        // Arrange
        var input = "go north";
        string? sessionId = null;

        // Act
        var gameRequest = new GameRequest(input, sessionId!);

        // Assert
        gameRequest.Input.Should().Be(input);
        gameRequest.SessionId.Should().BeNull();
    }

    [Test]
    public void GameRequest_Equality_SameValues_ShouldBeEqual()
    {
        // Arrange
        var gameRequest1 = new GameRequest("take sword", "abc123");
        var gameRequest2 = new GameRequest("take sword", "abc123");

        // Act & Assert
        gameRequest1.Should().Be(gameRequest2);
        (gameRequest1 == gameRequest2).Should().BeTrue();
    }

    [Test]
    public void GameRequest_Equality_DifferentInput_ShouldNotBeEqual()
    {
        // Arrange
        var gameRequest1 = new GameRequest("take sword", "abc123");
        var gameRequest2 = new GameRequest("drop sword", "abc123");

        // Act & Assert
        gameRequest1.Should().NotBe(gameRequest2);
        (gameRequest1 == gameRequest2).Should().BeFalse();
    }

    [Test]
    public void GameRequest_Equality_DifferentSessionId_ShouldNotBeEqual()
    {
        // Arrange
        var gameRequest1 = new GameRequest("take sword", "abc123");
        var gameRequest2 = new GameRequest("take sword", "xyz789");

        // Act & Assert
        gameRequest1.Should().NotBe(gameRequest2);
        (gameRequest1 == gameRequest2).Should().BeFalse();
    }

    [Test]
    public void GameRequest_WithDeconstruction_ShouldExtractCorrectValues()
    {
        // Arrange
        var gameRequest = new GameRequest("examine potion", "def456");

        // Act
        var (input, sessionId) = gameRequest;

        // Assert
        input.Should().Be("examine potion");
        sessionId.Should().Be("def456");
    }
}