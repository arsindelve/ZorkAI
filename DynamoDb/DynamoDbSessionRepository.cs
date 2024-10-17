using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Model.Interface;

namespace DynamoDb;

public class DynamoDbSessionRepository : DynamoDbRepositoryBase, ISessionRepository
{
    private const string TableName = "zork_session_ondemand";


    public async Task<string?> GetSession(string sessionId)
    {
        var table = Table.LoadTable(Client, TableName);
        var result = await table.GetItemAsync(sessionId);

        if (result is null)
            return null;

        return result["gameData"];
    }

    public async Task WriteSession(string sessionId, string gameData)
    {
        var item = new Dictionary<string, AttributeValue>
        {
            { "session_id", new AttributeValue(sessionId) },
            { "gameData", new AttributeValue(gameData) }
        };

        await Client.PutItemAsync(TableName, item);
    }
}