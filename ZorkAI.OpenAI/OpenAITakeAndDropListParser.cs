using Microsoft.Extensions.Logging;
using Model;
using Model.AIParsing;
using Newtonsoft.Json;

namespace ZorkAI.OpenAI;

public class OpenAITakeAndDropListParser(ILogger? logger) : OpenAIClientBase(logger), IAITakeAndAndDropParser
{
    protected override string ModelName => "gpt-4o-mini";

    public async Task<string[]> GetListOfItemsToTake(string input, string locationDescription)
    {
        return await Go(locationDescription, input, ParsingHelper.TakeUserPrompt);
    }

    public async Task<string[]> GetListOfItemsToDrop(string input, string inventoryDescription)
    {
        return await Go(inventoryDescription, input, ParsingHelper.DropUserPrompt);
    }

    private async Task<string[]> Go(string formatStringOne, string formatStringTwo, string promptName)
    {
        var prompt = string.Format(promptName, formatStringOne, formatStringTwo);
        var result = await CompleteJsonChatAsync<ItemsResponse>(prompt);
        return result?.Items ?? [];
    }

    private class ItemsResponse
    {
        [JsonProperty("items")]
        public string[] Items { get; set; } = [];
    }
}
