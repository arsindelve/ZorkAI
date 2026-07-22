using OpenAI.Chat;
using ZorkAI.OpenAI;

namespace ChatLambda;

/// <summary>
///     Companion conversation against a self-hosted, OpenAI-compatible server (LM Studio, Ollama,
///     koboldcpp — issue #383) instead of the cloud LangGraph Lambda. One instance plays one
///     character: it is constructed with that character's personality system prompt and keeps a
///     short rolling conversation memory so exchanges stay coherent within a session.
///     <para>
///     It implements all three character chat interfaces so <see cref="CompanionChatFactory" /> can
///     hand it out for whichever character is being wired. Responses carry no
///     <see cref="CompanionMetadata" /> — the couple of metadata-driven behaviors (Floyd's Repair
///     Room actions) simply degrade to plain dialog when playing locally.
///     </para>
/// </summary>
public class LocalCompanionChat : IChatWithFloyd, IChatWithBlather, IChatWithAmbassador
{
    // Local servers usually ignore the model id and use whatever is loaded; OPENAI_MODEL overrides
    // this via OpenAIEndpointSettings when set.
    private const string DefaultModel = "gpt-4o-mini";

    // Keep the last N exchanges so the character remembers the conversation without growing the
    // prompt without bound. Not serialized: memory resets on save/restore, which is acceptable.
    private const int MaxRememberedExchanges = 8;

    private readonly List<(string PlayerSaid, string CharacterSaid)> _history = new();
    private readonly string _systemPrompt;
    private readonly Lazy<ChatClient> _client;

    // Appended to every character prompt so local replies render the way the game already renders
    // character speech (see the characters' idle-speech SystemPrompts): third-person narration with
    // attributed, quoted dialog - not bare first person.
    private const string ConversationFormatRule =
        "\n\nRESPONSE FORMAT: Narrate in third person, prefacing any speech with an attribution " +
        "(e.g. 'Floyd says,' / 'Blather bellows,' / 'The ambassador wheezes,') and putting the " +
        "spoken words in double quotes. One to three sentences total.";

    public LocalCompanionChat(string systemPrompt)
    {
        _systemPrompt = systemPrompt + ConversationFormatRule;
        _client = new Lazy<ChatClient>(() =>
            OpenAIEndpointSettings.FromEnvironment().CreateClient(DefaultModel));
    }

    public Task<CompanionResponse> AskFloydAsync(string prompt)
    {
        return AskAsync(prompt);
    }

    public Task<CompanionResponse> AskBlatherAsync(string prompt)
    {
        return AskAsync(prompt);
    }

    public Task<CompanionResponse> AskAmbassadorAsync(string prompt)
    {
        return AskAsync(prompt);
    }

    private async Task<CompanionResponse> AskAsync(string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            throw new ArgumentException("Question cannot be empty", nameof(prompt));

        var messages = new List<ChatMessage> { new SystemChatMessage(_systemPrompt) };

        foreach (var (playerSaid, characterSaid) in _history)
        {
            messages.Add(new UserChatMessage(playerSaid));
            messages.Add(new AssistantChatMessage(characterSaid));
        }

        messages.Add(new UserChatMessage(prompt));

        var options = new ChatCompletionOptions { Temperature = 0.7f };
        ChatCompletion completion = await _client.Value.CompleteChatAsync(messages, options);
        var response = ChatGPTClient.NormalizeQuotes(completion.Content[0].Text.Trim());

        _history.Add((prompt, response));
        if (_history.Count > MaxRememberedExchanges)
            _history.RemoveAt(0);

        return new CompanionResponse(response, null);
    }
}
