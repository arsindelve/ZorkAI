using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Model.Interface;

namespace DynamoDb;

public class DynamoDbSessionRepository : DynamoDbRepositoryBase, ISessionRepository
{
    public async Task<string?> GetSessionState(string sessionId, string tableName)
    {
        var table = Table.LoadTable(Client, tableName);
        var result = await table.GetItemAsync(sessionId);

        if (result is null)
            return null;

        return result["gameData"];
    }

    public async Task WriteSessionState(string sessionId, string gameData, string tableName)
    {
        var item = new Dictionary<string, AttributeValue>
        {
            { "session_id", new AttributeValue(sessionId) },
            { "gameData", new AttributeValue(gameData) }
        };

        await Client.PutItemAsync(tableName, item);
    }

    public async Task WriteSessionStep(string sessionId, long turnIndex, string input, string output, string tableName)
    {
        var item = new Dictionary<string, AttributeValue>
        {
            { "session_id", new AttributeValue(sessionId) },
            { "ts", new AttributeValue { N = turnIndex.ToString() } },
            { "input", new AttributeValue(input) },
            { "output", new AttributeValue(output) }
        };

        await Client.PutItemAsync(tableName, item);
    }

    public async Task<string> GetSessionStepsAsText(string sessionId, string tableName)
    {
        var request = new QueryRequest
        {
            TableName = tableName,
            KeyConditionExpression = "session_id = :sid",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":sid", new AttributeValue(sessionId) }
            },
            ScanIndexForward = true // Ensures chronological order (ascending by ts)
        };

        var response = await Client.QueryAsync(request);
        return string.Join("\n", response.Items.Select(item => $"> {item["input"].S}\n{item["output"].S}"));
    }
}