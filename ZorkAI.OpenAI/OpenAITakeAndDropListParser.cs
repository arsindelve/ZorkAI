using Microsoft.Extensions.Logging;
using Model;
using Model.AIParsing;
using Newtonsoft.Json;
using OpenAI.Chat;

namespace ZorkAI.OpenAI;

public class OpenAITakeAndDropListParser : OpenAIClientBase, IAITakeAndAndDropParser
{
    public OpenAITakeAndDropListParser(ILogger? logger) : base(logger)
    {
    }

    public OpenAITakeAndDropListParser(ILogger? logger, IChatCompletionClient client)
        : base(logger, clientOverride: client)
    {
    }

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

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(prompt)
        };

        var options = new ChatCompletionOptions
        {
            Temperature = 0f,
            ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat()
        };

        var response = await Client!.CompleteChatAsync(messages, options);
        var result = JsonConvert.DeserializeObject<ItemsResponse>(response);
        return result?.Items ?? [];
    }

    private class ItemsResponse
    {
        [JsonProperty("items")]
        public string[] Items { get; set; } = [];
    }
}
