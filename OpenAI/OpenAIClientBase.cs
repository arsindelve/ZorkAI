using Azure.AI.OpenAI;
using Microsoft.Extensions.Logging;

namespace OpenAI;

public abstract class OpenAIClientBase
{
    protected readonly OpenAIClient? Client;
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
            Client = new OpenAIClient(key);
        }
    }

    protected abstract string DeploymentName { get; }

    protected ChatCompletionsOptions GetChatCompletionsOptions(string? message, float temperature)
    {
        return new ChatCompletionsOptions
        {
            Temperature = temperature,
            DeploymentName = DeploymentName,
            Messages =
            {
                new ChatRequestSystemMessage(message)
            }
        };
    }
}