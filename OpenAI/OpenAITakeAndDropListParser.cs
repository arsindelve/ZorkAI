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
        return await Go(locationDescription, input, ParsingHelper.TakeUserPrompt);
    }

    public async Task<string[]> GetListOfItemsToDrop(string input, string inventoryDescription, string sessionId)
    {
        return await Go(inventoryDescription, input, ParsingHelper.DropUserPrompt);
    }

    private async Task<string[]> Go(string formatStringOne, string formatStringTwo, string promptName)
    {
        var prompt = string.Format(promptName, formatStringOne, formatStringTwo);
        var options = GetChatCompletionsOptions(prompt, 0f);
        Response<ChatCompletions>? response = await Client.GetChatCompletionsAsync(options);
        return JsonConvert.DeserializeObject<string[]>(response.Value.Choices[0].Message.Content) ?? [];
    }
}