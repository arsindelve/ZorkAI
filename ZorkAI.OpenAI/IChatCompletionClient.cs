using OpenAI.Chat;

namespace ZorkAI.OpenAI;

/// <summary>
/// Small seam around the OpenAI SDK so prompt construction and fallback behavior can be tested
/// without credentials or network calls.
/// </summary>
public interface IChatCompletionClient
{
    Task<string> CompleteChatAsync(IReadOnlyList<ChatMessage> messages, ChatCompletionOptions options);
}

internal sealed class OpenAIChatCompletionClient(ChatClient client) : IChatCompletionClient
{
    public async Task<string> CompleteChatAsync(IReadOnlyList<ChatMessage> messages, ChatCompletionOptions options)
    {
        ChatCompletion completion = await client.CompleteChatAsync(messages, options);
        return completion.Content[0].Text;
    }
}
