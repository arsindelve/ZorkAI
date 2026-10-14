using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OpenAI.Chat;

namespace ZorkAI.OpenAI;

public abstract class OpenAIClientBase
{
    protected readonly IChatCompletionClient? Client;
    protected readonly ILogger? Logger;

    // Historically "do we have an OPEN_AI_KEY"; now "do we have a usable client". A self-hosted
    // OpenAI-compatible endpoint (issue #383) needs no key, but subclasses that gate optional
    // features on this flag (PronounResolver, hints) should still run against it.
    protected readonly bool HasApiKey;

    // Exposed so subclasses can spin up an additional client on a different model
    // (e.g. ChatGPTClient runs Floyd's companion speech on a cheaper, faster model).
    protected readonly string? ApiKey;

    // Resolved in the constructor, not in a field initializer. A field initializer runs for *every*
    // construction, before the constructor body - including the clientOverride test-double path that
    // never wants the environment read at all, and the requireApiKey:false clients whose whole
    // contract is to degrade quietly when AI is unavailable. Since Resolve throws on a malformed
    // ZORKAI_PROVIDER or OPENAI_BASE_URL, a field initializer turned a config typo into a hard crash
    // out of PronounResolver's and the hint model's constructors. Null here means "settings could not
    // be resolved", which is a degraded state, not a usable one.
    private readonly OpenAIEndpointSettings? _settings;

    // False when the real Client was replaced by an injected seam: CreateAdditionalClient must not
    // reach for the network behind a test double's back.
    private readonly bool _canCreateAdditionalClient;

    protected OpenAIClientBase(ILogger? logger, bool requireApiKey = true, string? modelOverride = null,
        IChatCompletionClient? clientOverride = null)
    {
        Logger = logger;

        if (clientOverride is not null)
        {
            // Deliberately before any environment read: an injected seam must not be able to fail on
            // the host's configuration.
            // An injected client means we have a working generation seam, so HasApiKey is true - but
            // ApiKey stays null on purpose. ApiKey only exists to let a subclass build a *second*
            // real ChatClient on another model (Floyd's companion speech); there is no raw key behind
            // an injected seam, and any subclass that needs a companion client injects that too.
            HasApiKey = true;
            Client = clientOverride;
            return;
        }

        try
        {
            _settings = OpenAIEndpointSettings.FromEnvironment();
        }
        catch (Exception ex)
        {
            // A malformed endpoint configuration is fatal for a client that must work, and merely
            // disabling for one that is optional. Either way it is reported, never silent.
            if (requireApiKey)
                throw;

            Logger?.LogError(ex,
                "Could not resolve the OpenAI endpoint configuration; this optional client is disabled.");

            HasApiKey = false;
            Client = null;
            return;
        }

        if (!_settings.CanCreateClient)
        {
            if (requireApiKey)
                throw new Exception(
                    "Missing environment variable OPEN_AI_KEY. To use a self-hosted, OpenAI-compatible " +
                    "server instead (LM Studio, Ollama, koboldcpp...), set OPENAI_BASE_URL or ZORKAI_PROVIDER.");

            HasApiKey = false;
            Client = null;
        }
        else
        {
            HasApiKey = true;
            ApiKey = _settings.ApiKey;
            _canCreateAdditionalClient = true;
            // modelOverride lets a subclass whose model is constructor-selectable build the base Client
            // with the right model directly, avoiding a virtual ModelName call before the subclass's
            // fields are initialized (and avoiding a second, unused client).
            Client = new OpenAIChatCompletionClient(_settings.CreateClient(modelOverride ?? ModelName));
        }
    }

    protected abstract string ModelName { get; }

    /// <summary>
    ///     Creates a second client on a different model against the same endpoint/credentials as the
    ///     primary <see cref="Client" /> (null if no real client can be constructed). Subclasses must
    ///     use this rather than newing a ChatClient so custom endpoints and model overrides apply
    ///     everywhere.
    /// </summary>
    protected IChatCompletionClient? CreateAdditionalClient(string modelName)
    {
        return _canCreateAdditionalClient && _settings is not null
            ? new OpenAIChatCompletionClient(_settings.CreateClient(modelName))
            : null;
    }

    /// <summary>
    ///     Shared plumbing for JSON-mode parser calls: send a single system prompt, ask for a JSON
    ///     object reply, and deserialize it to <typeparamref name="T" />. Tolerant of self-hosted
    ///     OpenAI-compatible servers (issue #383) the same way <see cref="OpenAITakeAndDropListParser" />
    ///     is: if the endpoint rejects the JSON response format it retries once without it, and the
    ///     reply is run through <see cref="LlmJson.ExtractJsonObject" /> to strip code fences/chatter
    ///     before parsing. Returns null when there is no usable object - callers treat that as "no
    ///     answer" and fall back.
    /// </summary>
    protected async Task<T?> CompleteJsonChatAsync<T>(string prompt, float temperature = 0f) where T : class
    {
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(prompt)
        };

        var options = new ChatCompletionOptions
        {
            Temperature = temperature,
            ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat()
        };

        string response;
        try
        {
            response = await Client!.CompleteChatAsync(messages, options);
        }
        catch (Exception ex)
        {
            // Some self-hosted OpenAI-compatible servers (issue #383) reject the JSON response
            // format. Retry once without it; ExtractJsonObject below tolerates the free-form output.
            Logger?.LogDebug(ex, "JSON response format rejected; retrying without it.");
            response = await Client!.CompleteChatAsync(messages, new ChatCompletionOptions { Temperature = temperature });
        }

        var json = LlmJson.ExtractJsonObject(response);
        if (json is null)
            return null;

        try
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
