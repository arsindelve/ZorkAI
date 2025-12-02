using System.Text;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using ChatLambda;

namespace UnitTests.LambdaChat;

[TestFixture]
public class ChatWithFloydTests
{
    private Mock<IAmazonLambda> _mockLambdaClient;

    [SetUp]
    public void Setup()
    {
        _mockLambdaClient = new Mock<IAmazonLambda>();
    }

    [Test]
    public async Task AskFloydAsync_WithMetadata_ReturnsCompanionResponseWithMetadata()
    {
        // Arrange
        var responseJson = """
        {
          "statusCode": 200,
          "body": "{\"results\":{\"single_message\":\"Floyd feels nervous about going north.\",\"metadata\":{\"assistant_type\":\"GoSomewhere\",\"parameters\":{\"direction\":\"north\"}}}}"
        }
        """;

        var invokeResponse = new InvokeResponse
        {
            StatusCode = 200,
            Payload = new MemoryStream(Encoding.UTF8.GetBytes(responseJson))
        };

        _mockLambdaClient
            .Setup(x => x.InvokeAsync(It.IsAny<InvokeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(invokeResponse);

        var target = new ChatWithFloyd(_mockLambdaClient.Object);

        // Act
        var result = await target.AskFloydAsync("floyd, go north");

        // Assert
        result.Should().NotBeNull();
        result.Message.Should().Be("Floyd feels nervous about going north.");
        result.Metadata.Should().NotBeNull();
        result.Metadata!.AssistantType.Should().Be("GoSomewhere");
        result.Metadata.Parameters.Should().NotBeNull();
        result.Metadata.Parameters.Should().ContainKey("direction");
        result.Metadata.Parameters!["direction"].ToString().Should().Be("north");
    }

    [Test]
    public async Task AskFloydAsync_WithoutMetadata_ReturnsCompanionResponseWithoutMetadata()
    {
        // Arrange
        var responseJson = """
        {
          "statusCode": 200,
          "body": "{\"results\":{\"single_message\":\"Floyd beeps happily!\"}}"
        }
        """;

        var invokeResponse = new InvokeResponse
        {
            StatusCode = 200,
            Payload = new MemoryStream(Encoding.UTF8.GetBytes(responseJson))
        };

        _mockLambdaClient
            .Setup(x => x.InvokeAsync(It.IsAny<InvokeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(invokeResponse);

        var target = new ChatWithFloyd(_mockLambdaClient.Object);

        // Act
        var result = await target.AskFloydAsync("floyd, you're awesome!");

        // Assert
        result.Should().NotBeNull();
        result.Message.Should().Be("Floyd beeps happily!");
        result.Metadata.Should().BeNull();
    }

    [Test]
    public async Task AskFloydAsync_WithNullMetadata_ReturnsCompanionResponseWithoutMetadata()
    {
        // Arrange
        var responseJson = """
        {
          "statusCode": 200,
          "body": "{\"results\":{\"single_message\":\"Floyd tilts his head.\",\"metadata\":null}}"
        }
        """;

        var invokeResponse = new InvokeResponse
        {
            StatusCode = 200,
            Payload = new MemoryStream(Encoding.UTF8.GetBytes(responseJson))
        };

        _mockLambdaClient
            .Setup(x => x.InvokeAsync(It.IsAny<InvokeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(invokeResponse);

        var target = new ChatWithFloyd(_mockLambdaClient.Object);

        // Act
        var result = await target.AskFloydAsync("floyd, what do you think?");

        // Assert
        result.Should().NotBeNull();
        result.Message.Should().Be("Floyd tilts his head.");
        result.Metadata.Should().BeNull();
    }

    [Test]
    public async Task AskFloydAsync_WithComplexParameters_ParsesCorrectly()
    {
        // Arrange
        var responseJson = """
        {
          "statusCode": 200,
          "body": "{\"results\":{\"single_message\":\"Floyd examines the item carefully.\",\"metadata\":{\"assistant_type\":\"ExamineItem\",\"parameters\":{\"item\":\"card\",\"detail_level\":\"high\",\"count\":3}}}}"
        }
        """;

        var invokeResponse = new InvokeResponse
        {
            StatusCode = 200,
            Payload = new MemoryStream(Encoding.UTF8.GetBytes(responseJson))
        };

        _mockLambdaClient
            .Setup(x => x.InvokeAsync(It.IsAny<InvokeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(invokeResponse);

        var target = new ChatWithFloyd(_mockLambdaClient.Object);

        // Act
        var result = await target.AskFloydAsync("floyd, examine the card");

        // Assert
        result.Should().NotBeNull();
        result.Message.Should().Be("Floyd examines the item carefully.");
        result.Metadata.Should().NotBeNull();
        result.Metadata!.AssistantType.Should().Be("ExamineItem");
        result.Metadata.Parameters.Should().NotBeNull();
        result.Metadata.Parameters.Should().ContainKey("item");
        result.Metadata.Parameters.Should().ContainKey("detail_level");
        result.Metadata.Parameters.Should().ContainKey("count");
    }

    [Test]
    public void AskFloydAsync_WithEmptyPrompt_ThrowsArgumentException()
    {
        // Arrange
        var target = new ChatWithFloyd(_mockLambdaClient.Object);

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(async () => await target.AskFloydAsync(""));
    }

    [Test]
    public void AskFloydAsync_WithWhitespacePrompt_ThrowsArgumentException()
    {
        // Arrange
        var target = new ChatWithFloyd(_mockLambdaClient.Object);

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(async () => await target.AskFloydAsync("   "));
    }

    [Test]
    public void AskFloydAsync_WithNonSuccessStatusCode_ThrowsException()
    {
        // Arrange
        var invokeResponse = new InvokeResponse
        {
            StatusCode = 500,
            Payload = new MemoryStream(Encoding.UTF8.GetBytes("{}"))
        };

        _mockLambdaClient
            .Setup(x => x.InvokeAsync(It.IsAny<InvokeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(invokeResponse);

        var target = new ChatWithFloyd(_mockLambdaClient.Object);

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(async () => await target.AskFloydAsync("test"));
        ex!.Message.Should().Contain("Lambda invocation failed with status code: 500");
    }

    [Test]
    public void AskFloydAsync_WithMalformedJson_ThrowsException()
    {
        // Arrange
        var responseJson = "{ invalid json }";

        var invokeResponse = new InvokeResponse
        {
            StatusCode = 200,
            Payload = new MemoryStream(Encoding.UTF8.GetBytes(responseJson))
        };

        _mockLambdaClient
            .Setup(x => x.InvokeAsync(It.IsAny<InvokeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(invokeResponse);

        var target = new ChatWithFloyd(_mockLambdaClient.Object);

        // Act & Assert
        Assert.ThrowsAsync<Exception>(async () => await target.AskFloydAsync("test"));
    }

    [Test]
    public void AskFloydAsync_WithMissingMessage_ThrowsException()
    {
        // Arrange
        var responseJson = """
        {
          "statusCode": 200,
          "body": "{\"results\":{\"metadata\":{\"assistant_type\":\"Test\"}}}"
        }
        """;

        var invokeResponse = new InvokeResponse
        {
            StatusCode = 200,
            Payload = new MemoryStream(Encoding.UTF8.GetBytes(responseJson))
        };

        _mockLambdaClient
            .Setup(x => x.InvokeAsync(It.IsAny<InvokeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(invokeResponse);

        var target = new ChatWithFloyd(_mockLambdaClient.Object);

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(async () => await target.AskFloydAsync("test"));
        ex!.Message.Should().Contain("Failed to extract message from Lambda response");
    }

    [Test]
    public async Task AskFloydAsync_SendsCorrectRequestPayload()
    {
        // Arrange
        InvokeRequest? capturedRequest = null;

        var responseJson = """
        {
          "statusCode": 200,
          "body": "{\"results\":{\"single_message\":\"Floyd beeps!\"}}"
        }
        """;

        var invokeResponse = new InvokeResponse
        {
            StatusCode = 200,
            Payload = new MemoryStream(Encoding.UTF8.GetBytes(responseJson))
        };

        _mockLambdaClient
            .Setup(x => x.InvokeAsync(It.IsAny<InvokeRequest>(), It.IsAny<CancellationToken>()))
            .Callback<InvokeRequest, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(invokeResponse);

        var target = new ChatWithFloyd(_mockLambdaClient.Object);

        // Act
        await target.AskFloydAsync("test prompt");

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.Payload.Should().Contain("test prompt");
        capturedRequest.Payload.Should().Contain("floyd");
    }
}
