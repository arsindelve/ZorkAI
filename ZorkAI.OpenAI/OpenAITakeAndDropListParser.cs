using Microsoft.Extensions.Logging;
using Model;
using Model.AIParsing;
using Newtonsoft.Json;
using OpenAI.Chat;

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

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(prompt)
        };

        var options = new ChatCompletionOptions
        {
            Temperature = 0f,
            ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat()
        };

        ChatCompletion completion;
        try
        {
            completion = await Client!.CompleteChatAsync(messages, options);
        }
        catch (Exception ex)
        {
            // Some self-hosted OpenAI-compatible servers (issue #383) reject the JSON response
            // format. Retry once without it; ParseItems below tolerates the free-form output.
            Logger?.LogDebug(ex, "JSON response format rejected; retrying without it.");
            completion = await Client!.CompleteChatAsync(messages, new ChatCompletionOptions { Temperature = 0f });
        }

        return ParseItems(completion.Content[0].Text);
    }

    /// <summary>
    ///     Parses the items array out of model output, tolerating the code fences and surrounding
    ///     prose that local models emit without a JSON response format. Public and static so it is
    ///     unit-testable without a model.
    /// </summary>
    public static string[] ParseItems(string? raw)
    {
        var json = LlmJson.ExtractJsonObject(raw);
        if (json is null)
            return [];

        try
        {
            var result = JsonConvert.DeserializeObject<ItemsResponse>(json);
            return result?.Items ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private class ItemsResponse
    {
        [JsonProperty("items")]
        public string[] Items { get; set; } = [];
    }
}
