using Microsoft.Extensions.Logging;
using Model;
using Model.AIParsing;
using Newtonsoft.Json;

namespace ZorkAI.OpenAI;

/// <summary>
/// Production implementation of the agentic fall-through narrator seam (issue #136). Asks a small,
/// cheap model - in JSON mode, offering exactly the two tools the engine has hands for - whether an
/// unhandled action on a held item plausibly drops or destroys it, grounded strictly in the live
/// location description and inventory passed in.
/// </summary>
public class OpenAIAgenticActionParser(ILogger? logger) : OpenAIClientBase(logger), IAgenticActionParser
{
    protected override string ModelName => "gpt-4o-mini";

    public async Task<AgenticActionResult> Resolve(string playerInput, string inventoryDescription,
        string locationDescription)
    {
        var prompt = string.Format(ParsingHelper.AgenticActionUserPrompt, locationDescription,
            inventoryDescription, playerInput);

        // Deflection-narrator discipline (see Request.DeflectionTemperature): low creativity so
        // the narrator can't embellish its way into an ungrounded tool call.
        var response = await CompleteJsonChatAsync<AgenticResponse>(prompt, 0.4f);

        if (response is null)
            return new AgenticActionResult(string.Empty, []);

        var toolCalls = (response.ToolCalls ?? [])
            .Select(ToToolCall)
            .Where(toolCall => toolCall is not null)
            .Cast<AgenticToolCall>()
            .ToList();

        Logger?.LogDebug("Agentic narrator for \"{Input}\": {Count} tool call(s), narration: {Narration}",
            playerInput, toolCalls.Count, response.Narration);

        return new AgenticActionResult(response.Narration ?? string.Empty, toolCalls);
    }

    private static AgenticToolCall? ToToolCall(RawToolCall raw)
    {
        if (string.IsNullOrWhiteSpace(raw.Target))
            return null;

        // Unknown tool names are dropped rather than guessed at - the engine only has two hands,
        // and a hallucinated third tool must never turn into a state change.
        return raw.Tool?.Trim().ToLowerInvariant() switch
        {
            "drop" => new AgenticToolCall(AgenticTool.Drop, raw.Target),
            "destroy" => new AgenticToolCall(AgenticTool.Destroy, raw.Target),
            _ => null
        };
    }

    private class AgenticResponse
    {
        [JsonProperty("narration")] public string? Narration { get; set; }

        [JsonProperty("tool_calls")] public List<RawToolCall>? ToolCalls { get; set; }
    }

    private class RawToolCall
    {
        [JsonProperty("tool")] public string? Tool { get; set; }

        [JsonProperty("target")] public string? Target { get; set; }
    }
}
