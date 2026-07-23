using Microsoft.Extensions.Logging;
using Model;
using Model.AIParsing;
using Model.Intent;
using OpenAI.Chat;

namespace ZorkAI.OpenAI;

public class OpenAIParser : OpenAIClientBase, IAIParser
{
    public OpenAIParser(ILogger? logger) : base(logger)
    {
    }

    public OpenAIParser(ILogger? logger, IChatCompletionClient client) : base(logger, clientOverride: client)
    {
    }

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

        var responseContent = await Client!.CompleteChatAsync(messages, options);
        return ParsingHelper.GetIntent(input, responseContent, Logger);
    }

    public string LanguageModel => ModelName;
}
