using Microsoft.Extensions.Logging;
using Model;
using Model.AIParsing;
using Model.Intent;
using Newtonsoft.Json;
using OpenAI.Chat;

namespace ZorkAI.OpenAI;

public class OpenAIParser(ILogger? logger) : OpenAIClientBase(logger), IAIParser
{
    protected override string ModelName => "gpt-4o";

    // Fixed seed so identical input yields the same parse across runs as far as the model allows
    // (best-effort reproducibility on top of temperature 0).
    private const long ParseSeed = 1024;

    public async Task<IntentBase> AskTheAIParser(string input, string locationDescription, string sessionId)
    {
        // NB: build via BuildSystemPrompt (string.Replace), never string.Format — the prompt embeds literal
        // { } in its JSON examples, which string.Format would throw a FormatException on.
        var systemPrompt = StructuredIntentParsing.BuildSystemPrompt(locationDescription, input);

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemPrompt)
        };

        // Structured Outputs: the model must return JSON conforming to the strict schema. This guarantees
        // well-formed output with a valid intent/direction, so the old failure modes (malformed/duplicate
        // tags -> NullIntent or HTTP 500, out-of-vocabulary intent) can no longer occur. See
        // StructuredIntentParsing for the schema/contract rationale.
        // Seed is an evaluation-only API in the OpenAI SDK (OPENAI001); suppress locally. It only tightens
        // reproducibility and can be dropped without affecting the structured-output guarantee.
#pragma warning disable OPENAI001
        var options = new ChatCompletionOptions
        {
            Temperature = 0f,
            Seed = ParseSeed,
            ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                jsonSchemaFormatName: "parsed_intent",
                jsonSchema: BinaryData.FromString(StructuredIntentParsing.JsonSchema),
                jsonSchemaIsStrict: true)
        };
#pragma warning restore OPENAI001

        ChatCompletion completion = await Client!.CompleteChatAsync(messages, options);
        var responseContent = completion.Content[0].Text;

        // Render the validated JSON to the canonical tag form and reuse the existing, battle-tested
        // intent-construction pipeline unchanged. A strict schema makes valid JSON overwhelmingly likely,
        // but a refusal or a max-tokens truncation could still hand back non-JSON — degrade to NullIntent
        // rather than throw, matching the graceful behavior of the tag-based path we replaced.
        ParsedIntent? parsed;
        try
        {
            parsed = JsonConvert.DeserializeObject<ParsedIntent>(responseContent);
        }
        catch (JsonException ex)
        {
            Logger?.LogWarning("Structured parse returned invalid JSON for input '{Input}': {Message}", input,
                ex.Message);
            return new NullIntent();
        }

        if (parsed is null)
        {
            Logger?.LogWarning("Structured parse returned null JSON for input '{Input}'", input);
            return new NullIntent();
        }

        var tags = StructuredIntentParsing.ToTagString(parsed);
        return ParsingHelper.GetIntent(input, tags, Logger);
    }

    public string LanguageModel => ModelName;
}
