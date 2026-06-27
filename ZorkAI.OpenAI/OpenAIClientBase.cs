using Microsoft.Extensions.Logging;
using OpenAI.Chat;

namespace ZorkAI.OpenAI;

public abstract class OpenAIClientBase
{
    protected readonly ChatClient? Client;
    protected readonly ILogger? Logger;
    protected readonly bool HasApiKey;

    // Exposed so subclasses can spin up an additional client on a different model
    // (e.g. ChatGPTClient runs Floyd's companion speech on a cheaper, faster model).
    protected readonly string? ApiKey;

    protected OpenAIClientBase(ILogger? logger, bool requireApiKey = true, string? modelOverride = null)
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
            ApiKey = key;
            // modelOverride lets a subclass whose model is constructor-selectable build the base Client
            // with the right model directly, avoiding a virtual ModelName call before the subclass's
            // fields are initialized (and avoiding a second, unused client).
            Client = new ChatClient(model: modelOverride ?? ModelName, apiKey: key);
        }
    }

    protected abstract string ModelName { get; }
}
