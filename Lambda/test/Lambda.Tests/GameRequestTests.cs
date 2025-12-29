using Model.Web;
using Xunit;

namespace Lambda.Tests;

public class GameRequestTests
{
    [Fact]
    public void GameRequest_DefaultsNoGeneratedResponsesToFalse()
    {
        // Arrange & Act
        var request = new GameRequest("take lamp", "session-123");

        // Assert
        Assert.False(request.NoGeneratedResponses);
    }

    [Fact]
    public void GameRequest_CanSetNoGeneratedResponsesToTrue()
    {
        // Arrange & Act
        var request = new GameRequest("take lamp", "session-123", NoGeneratedResponses: true);

        // Assert
        Assert.True(request.NoGeneratedResponses);
    }

    [Fact]
    public void GameRequest_CanSetNoGeneratedResponsesToFalseExplicitly()
    {
        // Arrange & Act
        var request = new GameRequest("take lamp", "session-123", NoGeneratedResponses: false);

        // Assert
        Assert.False(request.NoGeneratedResponses);
    }

    [Fact]
    public void GameRequest_PreservesInputAndSessionId()
    {
        // Arrange
        const string expectedInput = "go north";
        const string expectedSessionId = "session-456";

        // Act
        var request = new GameRequest(expectedInput, expectedSessionId, NoGeneratedResponses: true);

        // Assert
        Assert.Equal(expectedInput, request.Input);
        Assert.Equal(expectedSessionId, request.SessionId);
        Assert.True(request.NoGeneratedResponses);
    }
}
