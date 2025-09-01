using Amazon.DynamoDBv2.Model;
using Model.Interface;

namespace DynamoDb;

public class DynamoDbSavedGameRepository : DynamoDbRepositoryBase, ISavedGameRepository
{
    public async Task<string?> GetSavedGame(string id, string sessionId, string tableName)
    {
        var request = new GetItemRequest
        {
            TableName = tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "id", new AttributeValue { S = id } },
                {
                    "session_id", new AttributeValue { S = sessionId }
                }
            }
        };

        var response = await Client.GetItemAsync(request);
        return response.Item["gameData"].S;
    }

    public async Task<string> SaveGame(string? id, string clientId, string name, string gameData, string tableName)
    {
        id ??= Guid.NewGuid().ToString();
        var item = new Dictionary<string, AttributeValue>
        {
            { "id", new AttributeValue(id) },
            { "name", new AttributeValue(name) },
            { "session_id", new AttributeValue(clientId) },
            { "gameData", new AttributeValue(gameData) },
            { "date", new AttributeValue(DateTime.UtcNow.Ticks.ToString()) }
        };

        await Client.PutItemAsync(tableName, item);
        return id;
    }

    public async Task<List<(string Id, string Name, DateTime SavedOn)>> GetSavedGames(string sessionId,
        string tableName)
    {
        var request = new QueryRequest
        {
            TableName = tableName,
            IndexName = "session_id-index",
            KeyConditionExpression = "#session_id = :sessionIdVal",
            ExpressionAttributeNames = new Dictionary<string, string> { { "#session_id", "session_id" } },
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                { { ":sessionIdVal", new AttributeValue { S = sessionId } } }
        };

        var response = await Client.QueryAsync(request);

        var returnValue = response
            .Items
            .Select(item => (
                item["id"].S,
                item["name"].S,
                new DateTime(long.Parse(item["date"].S))
            ))
            .ToList();

        return returnValue;
    }

    public async Task DeleteSavedGame(string id, string sessionId, string tableName)
    {
        var request = new DeleteItemRequest
        {
            TableName = tableName,
            Key = new Dictionary<string, AttributeValue>
            {
                { "id", new AttributeValue { S = id } },
                { "session_id", new AttributeValue { S = sessionId } }
            }
        };

        await Client.DeleteItemAsync(request);
    }
}