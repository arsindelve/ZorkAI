using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Logging;
using Model;
using Model.AIParsing;
using Newtonsoft.Json;

namespace OpenAI;

public class OpenAITakeAndDropListParser(ILogger? logger) : OpenAIClientBase(logger), IAITakeAndAndDropParser
{
    protected override string DeploymentName => "gpt-4o-mini";

    public async Task<string[]> GetListOfItemsToTake(string input, string locationDescription, string sessionId)
    {
        var prompt = string.Format(ParsingHelper.TakeUserPrompt, locationDescription, input);
        var options = GetChatCompletionsOptions(prompt, 0f);
        Response<ChatCompletions>? response = await Client.GetChatCompletionsAsync(options);
        return JsonConvert.DeserializeObject<string[]>(response.Value.Choices[0].Message.Content) ?? [];
    }

    public Task<string[]> GetListOfItemsToDrop(string input, string inventoryDescription, string sessionId)
    {
        throw new NotImplementedException();
    }
}