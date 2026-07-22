using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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

    /// <summary>
    ///     Shared plumbing for the JSON-mode parser calls: send a single system prompt, force a JSON
    ///     object reply, deserialize it. Returns null when the model reply has no content or does not
    ///     deserialize - callers treat that as "no answer" and fall back.
    /// </summary>
    protected async Task<T?> CompleteJsonChatAsync<T>(string prompt, float temperature = 0f) where T : class
    {
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(prompt)
        };

        var options = new ChatCompletionOptions
        {
            Temperature = temperature,
            ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat()
        };

        ChatCompletion completion = await Client!.CompleteChatAsync(messages, options);
        if (completion.Content.Count == 0)
            return null;

        return JsonConvert.DeserializeObject<T>(completion.Content[0].Text);
    }
}
