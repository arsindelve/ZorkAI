using Azure.AI.OpenAI;
using Microsoft.Extensions.Logging;

namespace OpenAI;

public abstract class OpenAIClientBase
{
    protected readonly OpenAIClient Client;
    protected readonly ILogger? Logger;

    protected OpenAIClientBase(ILogger? logger)
    {
        Logger = logger;
        var key = Environment.GetEnvironmentVariable("OPEN_AI_KEY");

        if (string.IsNullOrEmpty(key))
            throw new Exception("Missing environment variable OPEN_AI_KEY");

        Client = new OpenAIClient(key);
    }

    protected abstract string DeploymentName { get; }

    protected ChatCompletionsOptions GetChatCompletionsOptions(string? systemPrompt)
    {
        return new ChatCompletionsOptions
        {
            DeploymentName = DeploymentName,
            Messages =
            {
                new ChatRequestSystemMessage(systemPrompt)
            }
        };
    }
}