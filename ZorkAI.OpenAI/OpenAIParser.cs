using Microsoft.Extensions.Logging;
using Model;
using Model.AIParsing;
using Model.Intent;
using OpenAI.Chat;

namespace ZorkAI.OpenAI;

public class OpenAIParser(ILogger? logger) : OpenAIClientBase(logger), IAIParser
{
    protected override string ModelName => "gpt-4o";

    public async Task<IntentBase> AskTheAIParser(string input, string locationDescription, string sessionId)
    {
        var systemPrompt = string.Format(ParsingHelper.SystemPrompt, locationDescription, input);

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemPrompt)
        };

        var options = new ChatCompletionOptions
        {
            Temperature = 0f
        };

        ChatCompletion completion = await Client!.CompleteChatAsync(messages, options);
        var responseContent = completion.Content[0].Text;
        return ParsingHelper.GetIntent(input, responseContent, Logger);
    }

    public string LanguageModel => ModelName;
}
