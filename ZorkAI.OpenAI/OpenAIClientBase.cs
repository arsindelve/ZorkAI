using Microsoft.Extensions.Logging;
using OpenAI.Chat;

namespace ZorkAI.OpenAI;

public abstract class OpenAIClientBase
{
    protected readonly ChatClient? Client;
    protected readonly ILogger? Logger;

    // Historically "do we have an OPEN_AI_KEY"; now "do we have a usable client". A self-hosted
    // OpenAI-compatible endpoint (issue #383) needs no key, but subclasses that gate optional
    // features on this flag (PronounResolver, hints) should still run against it.
    protected readonly bool HasApiKey;

    // Exposed so subclasses can spin up an additional client on a different model
    // (e.g. ChatGPTClient runs Floyd's companion speech on a cheaper, faster model).
    protected readonly string? ApiKey;

    private readonly OpenAIEndpointSettings _settings;

    protected OpenAIClientBase(ILogger? logger, bool requireApiKey = true, string? modelOverride = null)
    {
        Logger = logger;
        _settings = OpenAIEndpointSettings.FromEnvironment();

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
            // modelOverride lets a subclass whose model is constructor-selectable build the base Client
            // with the right model directly, avoiding a virtual ModelName call before the subclass's
            // fields are initialized (and avoiding a second, unused client).
            Client = _settings.CreateClient(modelOverride ?? ModelName);
        }
    }

    protected abstract string ModelName { get; }

    /// <summary>
    ///     Creates a second client on a different model against the same endpoint/credentials as the
    ///     primary <see cref="Client" /> (null if no client can be constructed). Subclasses must use
    ///     this rather than newing a ChatClient so custom endpoints and model overrides apply everywhere.
    /// </summary>
    protected ChatClient? CreateAdditionalClient(string modelName)
    {
        return Client is null ? null : _settings.CreateClient(modelName);
    }
}
