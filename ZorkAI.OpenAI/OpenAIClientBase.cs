using Microsoft.Extensions.Logging;
using OpenAI.Chat;

namespace ZorkAI.OpenAI;

public abstract class OpenAIClientBase
{
    protected readonly ChatClient? Client;
    protected readonly ILogger? Logger;
    protected readonly bool HasApiKey;

    protected OpenAIClientBase(ILogger? logger, bool requireApiKey = true)
    {
        Logger = logger;
        var key = Environment.GetEnvironmentVariable("OPEN_AI_KEY");

        if (string.IsNullOrEmpty(key))
        {
            if (requireApiKey)
                throw new Exception("Missing environment variable OPEN_AI_KEY");

            HasApiKey = false;
            Client = null;
        }
        else
        {
            HasApiKey = true;
            Client = new ChatClient(model: ModelName, apiKey: key);
        }
    }

    protected abstract string ModelName { get; }
}
