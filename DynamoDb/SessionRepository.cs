﻿using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;

namespace DynamoDb;

public class SessionRepository : ISessionRepository
{
    private readonly AmazonDynamoDBClient _client;

    public SessionRepository()
    {
        var clientConfig = new AmazonDynamoDBConfig
        {
            RegionEndpoint = RegionEndpoint.USEast1
        };

        _client = new AmazonDynamoDBClient(clientConfig);
    }

    public async Task<string?> GetSession(string sessionId)
    {
        var table = Table.LoadTable(_client, "zork_session");
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

        await _client.PutItemAsync("zork_session", item);
    }
}