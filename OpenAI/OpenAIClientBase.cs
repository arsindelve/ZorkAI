using Azure.AI.OpenAI;
using Microsoft.Extensions.Logging;

namespace OpenAI;

public abstract class OpenAIClientBase
{
    protected readonly OpenAIClient Client;
    protected readonly ILogger? Logger;
    
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
    
    protected OpenAIClientBase(ILogger? logger)
    {
        Logger = logger;
        var key = Environment.GetEnvironmentVariable("OPEN_AI_KEY");

        if (string.IsNullOrEmpty(key))
            throw new Exception("Missing environment variable OPEN_AI_KEY");

        Client = new OpenAIClient(key);
    }
}