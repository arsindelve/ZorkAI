using FluentAssertions;
using Model.Web;
using NUnit.Framework;

namespace UnitTests.Models;

[TestFixture]
public class RestoreGameRequestTests
{
    [Test]
    public void RestoreGameRequest_Constructor_ShouldSetAllProperties()
    {
        // Arrange
        string expectedSessionId = "session123";
        string expectedClientId = "client456";
        string expectedId = "game789";

        // Act
        var restoreGameRequest = new RestoreGameRequest(expectedSessionId, expectedClientId, expectedId);

        // Assert
        restoreGameRequest.SessionId.Should().Be(expectedSessionId);
        restoreGameRequest.ClientId.Should().Be(expectedClientId);
        restoreGameRequest.Id.Should().Be(expectedId);
    }

    [Test]
    public void RestoreGameRequest_Equality_SameValues_ShouldBeEqual()
    {
        // Arrange
        var request1 = new RestoreGameRequest("session123", "client456", "game789");
        var request2 = new RestoreGameRequest("session123", "client456", "game789");

        // Act & Assert
        request1.Should().BeEquivalentTo(request2);
        (request1 == request2).Should().BeTrue();
        request1.GetHashCode().Should().Be(request2.GetHashCode());
    }

    [Test]
    public void RestoreGameRequest_Equality_DifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var request1 = new RestoreGameRequest("session123", "client456", "game789");

        // Different SessionId
        var request2 = new RestoreGameRequest("differentSession", "client456", "game789");

        // Different ClientId
        var request3 = new RestoreGameRequest("session123", "differentClient", "game789");

        // Different Id
        var request4 = new RestoreGameRequest("session123", "client456", "differentGame");

        // Act & Assert
        request1.Should().NotBeEquivalentTo(request2);
        (request1 == request2).Should().BeFalse();

        request1.Should().NotBeEquivalentTo(request3);
        (request1 == request3).Should().BeFalse();

        request1.Should().NotBeEquivalentTo(request4);
        (request1 == request4).Should().BeFalse();
    }

    [Test]
    public void RestoreGameRequest_WithNull_ShouldAcceptNullValues()
    {
        // Arrange & Act
        // Note: In practice, you shouldn't pass null to these parameters,
        // but we're testing that the record type handles it without throwing
        var request = new RestoreGameRequest(null!, null!, null!);

        // Assert
        request.SessionId.Should().BeNull();
        request.ClientId.Should().BeNull();
        request.Id.Should().BeNull();
    }

    [Test]
    public void RestoreGameRequest_Deconstruction_ShouldWork()
    {
        // Arrange
        string expectedSessionId = "session123";
        string expectedClientId = "client456";
        string expectedId = "game789";
        var request = new RestoreGameRequest(expectedSessionId, expectedClientId, expectedId);

        // Act
        var (sessionId, clientId, id) = request;

        // Assert
        sessionId.Should().Be(expectedSessionId);
        clientId.Should().Be(expectedClientId);
        id.Should().Be(expectedId);
    }
}
