using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Model.Interface;

namespace DynamoDb;

public class DynamoDbSessionRepository : DynamoDbRepositoryBase, ISessionRepository
{
    public async Task<string?> GetSession(string sessionId, string tableName)
    {
        var table = Table.LoadTable(Client, tableName);
        var result = await table.GetItemAsync(sessionId);

        if (result is null)
            return null;

        return result["gameData"];
    }

    public async Task WriteSession(string sessionId, string gameData, string tableName)
    {
        var item = new Dictionary<string, AttributeValue>
        {
            { "session_id", new AttributeValue(sessionId) },
            { "gameData", new AttributeValue(gameData) }
        };

        await Client.PutItemAsync(tableName, item);
    }
}