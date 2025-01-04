using Microsoft.Extensions.Logging;
using Model;
using Model.AIParsing;
using Model.Intent;

namespace OpenAI;

public class OpenAIParser(ILogger? logger) : OpenAIClientBase(logger), IAIParser
{
    protected override string DeploymentName => "gpt-4o";

    public async Task<IntentBase> AskTheAIParser(string input, string locationDescription, string sessionId)
    {
        var systemPrompt = string.Format(ParsingHelper.Prompt, locationDescription, input);
        var options = GetChatCompletionsOptions(systemPrompt);
        options.Temperature = 0;

        var response = await Client.GetChatCompletionsAsync(options);
        var responseMessage = response.Value.Choices[0].Message;
        return ParsingHelper.GetIntent(input, responseMessage.Content, Logger);
    }

    public string LanguageModel => DeploymentName;
}