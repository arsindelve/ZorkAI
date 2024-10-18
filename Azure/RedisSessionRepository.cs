using Model;
using Model.Interface;
using StackExchange.Redis;

namespace Azure;

public class RedisSessionRepository : ISessionRepository
{
    private static readonly IDatabase Cache;

    static RedisSessionRepository()
    {
        var environmentVariable = Environment.GetEnvironmentVariable("REDIS_CONNECTION");

        if (string.IsNullOrEmpty(environmentVariable))
            throw new Exception("Missing Environment Variable 'REDIS_CONNECTION'");

        var connection = ConnectionMultiplexer.Connect(environmentVariable);
        Cache = connection.GetDatabase();
    }

    public async Task<string?> GetSession(string sessionId)
    {
        return (await Cache.StringGetAsync(sessionId))!;
    }

    public Task WriteSession(string sessionId, string gameData)
    {
        return Cache.StringSetAsync(sessionId, gameData);
    }
}