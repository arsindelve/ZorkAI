using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OpenAI.Chat;
using ZorkAI.OpenAI;

namespace ChatLambda;

/// <summary>
///     Conversation classifier for self-hosted play (issue #383): decides whether input is speech
///     directed at a companion and rewrites it in second person, using the configured local
///     OpenAI-compatible endpoint instead of the cloud Lambda that
///     <see cref="ParseConversation" /> invokes.
///     <para>
///     Deliberately fail-safe: any transport error or unparseable model output yields
///     (false, ""), which sends the input down the normal command path — and the engine's
///     deterministic direct-address detection ("floyd, ...") still catches explicit conversation
///     on its own, so a flaky local model degrades the subtle cases only.
///     </para>
/// </summary>
public class LocalParseConversation : IParseConversation
{
    private const string DefaultModel = "gpt-4o-mini";

    private const string ClassifierPrompt =
        "You classify one line of player input from a text adventure game that has companion " +
        "characters (such as a robot named Floyd).\n" +
        "Decide: is the input SPEECH OR A COMMAND DIRECTED AT A CHARACTER, or is it an ordinary " +
        "game command the player themselves performs?\n" +
        "If it IS directed at a character, rewrite it as second-person text to say to that " +
        "character, dropping the character's name.\n" +
        "\n" +
        "Examples:\n" +
        "  \"floyd, go north\" -> {\"is_conversational\": true, \"rewritten\": \"go north\"}\n" +
        "  \"tell floyd to wait here\" -> {\"is_conversational\": true, \"rewritten\": \"wait here\"}\n" +
        "  \"ask the robot what happened\" -> {\"is_conversational\": true, \"rewritten\": \"what happened?\"}\n" +
        "  \"go north\" -> {\"is_conversational\": false, \"rewritten\": \"\"}\n" +
        "  \"examine floyd\" -> {\"is_conversational\": false, \"rewritten\": \"\"}\n" +
        "  \"take the lamp\" -> {\"is_conversational\": false, \"rewritten\": \"\"}\n" +
        "\n" +
        "Respond with ONLY a JSON object exactly like the examples: " +
        "{\"is_conversational\": <bool>, \"rewritten\": \"<string>\"}\n" +
        "\n" +
        "Player input: \"{0}\"";

    private readonly Lazy<ChatClient> _client = new(() =>
        OpenAIEndpointSettings.FromEnvironment().CreateClient(DefaultModel));

    public ILogger Logger { get; set; } = NullLogger.Instance;

    public async Task<(bool isConversational, string response)> ParseAsync(string input)
    {
        try
        {
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(ClassifierPrompt.Replace("{0}", input))
            };

            // No JSON response format on purpose: some local servers reject it. The prompt plus
            // tolerant extraction below is the compatible path.
            var options = new ChatCompletionOptions { Temperature = 0f };
            ChatCompletion completion = await _client.Value.CompleteChatAsync(messages, options);
            var raw = completion.Content[0].Text;

            if (TryExtractClassification(raw, out var isConversational, out var rewritten))
                return (isConversational, rewritten);

            Logger.LogDebug("Local conversation classifier returned unparseable output: {Raw}", raw);
            return (false, string.Empty);
        }
        catch (Exception ex)
        {
            Logger.LogDebug(ex, "Local conversation classifier failed; treating input as a normal command.");
            return (false, string.Empty);
        }
    }

    /// <summary>
    ///     Pulls the classification out of raw model output, tolerating code fences, surrounding
    ///     prose, and property-name variants. Public and static so it is unit-testable without a model.
    /// </summary>
    public static bool TryExtractClassification(string? raw, out bool isConversational, out string rewritten)
    {
        isConversational = false;
        rewritten = string.Empty;

        var json = LlmJson.ExtractJsonObject(raw);
        if (json is null)
            return false;

        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            if (!TryGetBool(root, out isConversational, "is_conversational", "isConversational"))
                return false;

            if (TryGetString(root, out var value, "rewritten", "response"))
                rewritten = value;

            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool TryGetBool(JsonElement root, out bool value, params string[] names)
    {
        foreach (var name in names)
            if (root.TryGetProperty(name, out var property) &&
                property.ValueKind is JsonValueKind.True or JsonValueKind.False)
            {
                value = property.GetBoolean();
                return true;
            }

        value = false;
        return false;
    }

    private static bool TryGetString(JsonElement root, out string value, params string[] names)
    {
        foreach (var name in names)
            if (root.TryGetProperty(name, out var property) && property.ValueKind == JsonValueKind.String)
            {
                value = property.GetString() ?? string.Empty;
                return true;
            }

        value = string.Empty;
        return false;
    }
}
