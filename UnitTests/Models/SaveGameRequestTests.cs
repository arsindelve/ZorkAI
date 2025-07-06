using Model.Web;

namespace UnitTests.Models;

[TestFixture]
public class SaveGameRequestTests
{
    [Test]
    public void SaveGameRequest_Constructor_ShouldSetAllProperties()
    {
        // Arrange
        var expectedSessionId = "session123";
        var expectedClientId = "client456";
        var expectedName = "Adventure Game";
        var expectedId = "game789";

        // Act
        var saveGameRequest = new SaveGameRequest(expectedSessionId, expectedClientId, expectedName, expectedId);

        // Assert
        saveGameRequest.SessionId.Should().Be(expectedSessionId);
        saveGameRequest.ClientId.Should().Be(expectedClientId);
        saveGameRequest.Name.Should().Be(expectedName);
        saveGameRequest.Id.Should().Be(expectedId);
    }

    [Test]
    public void SaveGameRequest_Constructor_WithNullId_ShouldAcceptNull()
    {
        // Arrange
        var expectedSessionId = "session123";
        var expectedClientId = "client456";
        var expectedName = "Adventure Game";
        string? expectedId = null;

        // Act
        var saveGameRequest = new SaveGameRequest(expectedSessionId, expectedClientId, expectedName, expectedId);

        // Assert
        saveGameRequest.SessionId.Should().Be(expectedSessionId);
        saveGameRequest.ClientId.Should().Be(expectedClientId);
        saveGameRequest.Name.Should().Be(expectedName);
        saveGameRequest.Id.Should().BeNull();
    }

    [Test]
    public void SaveGameRequest_Equals_WithIdenticalObjects_ShouldBeEqual()
    {
        // Arrange
        var saveGameRequest1 = new SaveGameRequest("session123", "client456", "Adventure Game", "game789");
        var saveGameRequest2 = new SaveGameRequest("session123", "client456", "Adventure Game", "game789");

        // Act & Assert
        saveGameRequest1.Should().BeEquivalentTo(saveGameRequest2);
        (saveGameRequest1 == saveGameRequest2).Should().BeTrue();
        saveGameRequest1.Equals(saveGameRequest2).Should().BeTrue();
    }

    [Test]
    public void SaveGameRequest_Equals_WithDifferentSessionId_ShouldNotBeEqual()
    {
        // Arrange
        var saveGameRequest1 = new SaveGameRequest("session123", "client456", "Adventure Game", "game789");
        var saveGameRequest2 = new SaveGameRequest("differentSession", "client456", "Adventure Game", "game789");

        // Act & Assert
        saveGameRequest1.Should().NotBeEquivalentTo(saveGameRequest2);
    }

    [Test]
    public void SaveGameRequest_Equals_WithDifferentClientId_ShouldNotBeEqual()
    {
        // Arrange
        var saveGameRequest1 = new SaveGameRequest("session123", "client456", "Adventure Game", "game789");
        var saveGameRequest2 = new SaveGameRequest("session123", "differentClient", "Adventure Game", "game789");

        // Act & Assert
        saveGameRequest1.Should().NotBeEquivalentTo(saveGameRequest2);
    }

    [Test]
    public void SaveGameRequest_Equals_WithDifferentName_ShouldNotBeEqual()
    {
        // Arrange
        var saveGameRequest1 = new SaveGameRequest("session123", "client456", "Adventure Game", "game789");
        var saveGameRequest2 = new SaveGameRequest("session123", "client456", "Different Game", "game789");

        // Act & Assert
        saveGameRequest1.Should().NotBeEquivalentTo(saveGameRequest2);
    }

    [Test]
    public void SaveGameRequest_Equals_WithDifferentId_ShouldNotBeEqual()
    {
        // Arrange
        var saveGameRequest1 = new SaveGameRequest("session123", "client456", "Adventure Game", "game789");
        var saveGameRequest2 = new SaveGameRequest("session123", "client456", "Adventure Game", "differentId");

        // Act & Assert
        saveGameRequest1.Should().NotBeEquivalentTo(saveGameRequest2);
    }

    [Test]
    public void SaveGameRequest_Equals_WithOneNullId_ShouldNotBeEqual()
    {
        // Arrange
        var saveGameRequest1 = new SaveGameRequest("session123", "client456", "Adventure Game", "game789");
        var saveGameRequest2 = new SaveGameRequest("session123", "client456", "Adventure Game", null);

        // Act & Assert
        saveGameRequest1.Should().NotBeEquivalentTo(saveGameRequest2);
    }

    [Test]
    public void SaveGameRequest_Equals_WithBothNullIds_ShouldBeEqual()
    {
        // Arrange
        var saveGameRequest1 = new SaveGameRequest("session123", "client456", "Adventure Game", null);
        var saveGameRequest2 = new SaveGameRequest("session123", "client456", "Adventure Game", null);

        // Act & Assert
        saveGameRequest1.Should().BeEquivalentTo(saveGameRequest2);
    }

    [Test]
    public void SaveGameRequest_ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var saveGameRequest = new SaveGameRequest("session123", "client456", "Adventure Game", "game789");

        // Act
        var result = saveGameRequest.ToString();

        // Assert
        result.Should().Contain("session123");
        result.Should().Contain("client456");
        result.Should().Contain("Adventure Game");
        result.Should().Contain("game789");
    }

    [Test]
    public void SaveGameRequest_WithOperator_ShouldCreateNewInstanceWithChangedProperty()
    {
        // Arrange
        var originalRequest = new SaveGameRequest("session123", "client456", "Adventure Game", "game789");
        var newName = "Updated Game";

        // Act
        var updatedRequest = originalRequest with { Name = newName };

        // Assert
        updatedRequest.Should().NotBeSameAs(originalRequest);
        updatedRequest.SessionId.Should().Be(originalRequest.SessionId);
        updatedRequest.ClientId.Should().Be(originalRequest.ClientId);
        updatedRequest.Name.Should().Be(newName);
        updatedRequest.Id.Should().Be(originalRequest.Id);
        originalRequest.Name.Should().Be("Adventure Game"); // Original unchanged
    }
}
