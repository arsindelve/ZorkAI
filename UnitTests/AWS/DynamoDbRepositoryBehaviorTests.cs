using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using DynamoDb;

namespace UnitTests.AWS;

[TestFixture]
public class DynamoDbSavedGameRepositoryBehaviorTests
{
    private Mock<IAmazonDynamoDB> _client = null!;
    private DynamoDbSavedGameRepository _target = null!;

    [SetUp]
    public void SetUp()
    {
        _client = new Mock<IAmazonDynamoDB>();
        _target = new DynamoDbSavedGameRepository(_client.Object,
            () => Guid.Parse("11111111-1111-1111-1111-111111111111"));
    }

    [Test]
    public async Task SaveGame_WritesAllFields_AndUsesInjectedIdFactory()
    {
        PutItemRequest? captured = null;
        _client.Setup(c => c.PutItemAsync(It.IsAny<PutItemRequest>(), It.IsAny<CancellationToken>()))
            .Callback<PutItemRequest, CancellationToken>((request, _) => captured = request)
            .ReturnsAsync(new PutItemResponse());

        var id = await _target.SaveGame(null, "client-1", "Before the door", "encoded", "saves");

        id.Should().Be("11111111-1111-1111-1111-111111111111");
        captured.Should().NotBeNull();
        captured!.TableName.Should().Be("saves");
        captured.Item["id"].S.Should().Be(id);
        captured.Item["session_id"].S.Should().Be("client-1");
        captured.Item["name"].S.Should().Be("Before the door");
        captured.Item["gameData"].S.Should().Be("encoded");
        long.TryParse(captured.Item["date"].S, out _).Should().BeTrue();
    }

    [Test]
    public async Task SaveGame_PreservesProvidedId()
    {
        _client.Setup(c => c.PutItemAsync(It.IsAny<PutItemRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PutItemResponse());

        await _target.SaveGame("existing", "client", "name", "data", "saves");

        _client.Verify(c => c.PutItemAsync(It.Is<PutItemRequest>(r => r.Item["id"].S == "existing"),
            It.IsAny<CancellationToken>()));
    }

    [Test]
    public async Task GetSavedGame_UsesCompositeKey_AndReturnsGameData()
    {
        _client.Setup(c => c.GetItemAsync(It.IsAny<GetItemRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetItemResponse
            {
                Item = new Dictionary<string, AttributeValue> { { "gameData", new AttributeValue("encoded") } }
            });

        var result = await _target.GetSavedGame("save-1", "client-1", "saves");
        result.Should().Be("encoded");
        _client.Verify(c => c.GetItemAsync(It.Is<GetItemRequest>(r =>
                r.TableName == "saves" && r.Key["id"].S == "save-1" && r.Key["session_id"].S == "client-1"),
            It.IsAny<CancellationToken>()));
    }

    [Test]
    public async Task GetSavedGames_QueriesSessionIndex_AndMapsRows()
    {
        var savedOn = new DateTime(2026, 7, 22, 12, 0, 0, DateTimeKind.Utc);
        _client.Setup(c => c.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QueryResponse
            {
                Items =
                [
                    new Dictionary<string, AttributeValue>
                    {
                        { "id", new AttributeValue("save-1") },
                        { "name", new AttributeValue("At the pod") },
                        { "date", new AttributeValue(savedOn.Ticks.ToString()) }
                    }
                ]
            });

        var result = await _target.GetSavedGames("client-1", "saves");

        result.Should().ContainSingle().Which.Should().Be(("save-1", "At the pod", new DateTime(savedOn.Ticks)));
        _client.Verify(c => c.QueryAsync(It.Is<QueryRequest>(r => r.TableName == "saves" &&
            r.IndexName == "session_id-index" && r.ExpressionAttributeValues[":sessionIdVal"].S == "client-1"),
            It.IsAny<CancellationToken>()));
    }

    [Test]
    public async Task DeleteSavedGame_UsesCompositeKey()
    {
        _client.Setup(c => c.DeleteItemAsync(It.IsAny<DeleteItemRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeleteItemResponse());

        await _target.DeleteSavedGameAsync("save-1", "client-1", "saves");

        _client.Verify(c => c.DeleteItemAsync(It.Is<DeleteItemRequest>(r => r.TableName == "saves" &&
            r.Key["id"].S == "save-1" && r.Key["session_id"].S == "client-1"), It.IsAny<CancellationToken>()));
    }
}

[TestFixture]
public class DynamoDbSessionRepositoryBehaviorTests
{
    private Mock<IAmazonDynamoDB> _client = null!;
    private DynamoDbSessionRepository _target = null!;

    [SetUp]
    public void SetUp()
    {
        _client = new Mock<IAmazonDynamoDB>();
        _target = new DynamoDbSessionRepository(_client.Object);
    }

    [Test]
    public async Task GetSessionState_ReturnsData_WhenItemExists()
    {
        _client.Setup(c => c.GetItemAsync(It.IsAny<GetItemRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetItemResponse
            {
                Item = new Dictionary<string, AttributeValue> { { "gameData", new AttributeValue("state") } }
            });

        var result = await _target.GetSessionState("session-1", "sessions");
        result.Should().Be("state");
        _client.Verify(c => c.GetItemAsync(It.Is<GetItemRequest>(r => r.TableName == "sessions" &&
            r.Key["session_id"].S == "session-1"), It.IsAny<CancellationToken>()));
    }

    [Test]
    public async Task GetSessionState_ReturnsNull_WhenItemDoesNotExist()
    {
        _client.Setup(c => c.GetItemAsync(It.IsAny<GetItemRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetItemResponse { Item = new Dictionary<string, AttributeValue>() });

        var result = await _target.GetSessionState("missing", "sessions");
        result.Should().BeNull();
    }

    [Test]
    public async Task WriteSessionState_WritesSessionAndGameData()
    {
        _client.Setup(c => c.PutItemAsync(It.IsAny<PutItemRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PutItemResponse());

        await _target.WriteSessionState("session-1", "state", "sessions");

        _client.Verify(c => c.PutItemAsync(It.Is<PutItemRequest>(r => r.TableName == "sessions" &&
            r.Item["session_id"].S == "session-1" && r.Item["gameData"].S == "state"),
            It.IsAny<CancellationToken>()));
    }

    [Test]
    public async Task WriteSessionStep_WritesNumericTurnAndTranscriptFields()
    {
        _client.Setup(c => c.PutItemAsync(It.IsAny<PutItemRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PutItemResponse());

        await _target.WriteSessionStep("session-1", 42, "look", "A room.", "steps");

        _client.Verify(c => c.PutItemAsync(It.Is<PutItemRequest>(r => r.TableName == "steps" &&
            r.Item["session_id"].S == "session-1" && r.Item["ts"].N == "42" &&
            r.Item["input"].S == "look" && r.Item["output"].S == "A room."),
            It.IsAny<CancellationToken>()));
    }

    [Test]
    public async Task GetSessionStepsAsText_RequestsChronologicalRows_AndFormatsTranscript()
    {
        _client.Setup(c => c.QueryAsync(It.IsAny<QueryRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QueryResponse
            {
                Items =
                [
                    new Dictionary<string, AttributeValue>
                        { { "input", new AttributeValue("look") }, { "output", new AttributeValue("A room.") } },
                    new Dictionary<string, AttributeValue>
                        { { "input", new AttributeValue("north") }, { "output", new AttributeValue("You move.") } }
                ]
            });

        var result = await _target.GetSessionStepsAsText("session-1", "steps");

        result.Should().Be("> look\nA room.\n> north\nYou move.");
        _client.Verify(c => c.QueryAsync(It.Is<QueryRequest>(r => r.TableName == "steps" &&
            r.ScanIndexForward == true && r.ExpressionAttributeValues[":sid"].S == "session-1"),
            It.IsAny<CancellationToken>()));
    }
}
