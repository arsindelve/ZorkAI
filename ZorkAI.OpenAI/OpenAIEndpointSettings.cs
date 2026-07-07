using System.ClientModel;
using OpenAI;
using OpenAI.Chat;

namespace ZorkAI.OpenAI;

/// <summary>
///     Resolves where every OpenAI-protocol client in the engine should point: the real OpenAI API
///     (default), or any OpenAI-compatible server such as LM Studio, Ollama or koboldcpp (issue #383).
///     Resolution is driven entirely by environment variables so the console, Lambdas and web server
///     all pick it up without wiring changes:
///     <list type="bullet">
///         <item><c>ZORKAI_PROVIDER</c> — a named preset ("openai", "lmstudio", "ollama", "koboldcpp")
///         supplying the provider's conventional local base URL.</item>
///         <item><c>OPENAI_BASE_URL</c> — an explicit endpoint (e.g. http://localhost:1234/v1);
///         wins over the preset's default URL.</item>
///         <item><c>OPENAI_MODEL</c> — overrides every hardcoded model id (narrator, parser, etc.);
///         local servers typically expose a single loaded model, so one override covers all roles.</item>
///         <item><c>OPEN_AI_KEY</c> — the existing API key variable. Optional when a custom endpoint
///         is configured, since local servers accept any (or no) key.</item>
///     </list>
/// </summary>
public sealed class OpenAIEndpointSettings
{
    /// <summary>Placeholder sent to keyless local servers; the OpenAI SDK requires a non-empty credential.</summary>
    private const string KeylessPlaceholder = "not-needed";

    // Conventional default ports for well-known OpenAI-compatible local servers. An entry here is
    // just a base-URL default, not a behavioral switch — every provider speaks the same protocol.
    private static readonly Dictionary<string, string> ProviderDefaultUrls = new(StringComparer.OrdinalIgnoreCase)
    {
        ["lmstudio"] = "http://localhost:1234/v1",
        ["ollama"] = "http://localhost:11434/v1",
        ["koboldcpp"] = "http://localhost:5001/v1"
    };

    private OpenAIEndpointSettings(Uri? endpoint, string? apiKey, string? modelOverride)
    {
        Endpoint = endpoint;
        ApiKey = apiKey;
        ModelOverride = modelOverride;
    }

    /// <summary>The custom endpoint to use, or null to use the official OpenAI API.</summary>
    public Uri? Endpoint { get; }

    /// <summary>The API key, or null if none was configured.</summary>
    public string? ApiKey { get; }

    /// <summary>When set, replaces every client's hardcoded model id.</summary>
    public string? ModelOverride { get; }

    /// <summary>
    ///     True when pointed at a self-hosted/OpenAI-compatible endpoint rather than the official API.
    ///     Also used as the signal to skip cloud-only infrastructure (CloudWatch logging) at startup.
    /// </summary>
    public bool IsSelfHosted => Endpoint is not null;

    /// <summary>
    ///     True when a usable client can be constructed: either a key is present, or the endpoint is
    ///     a custom server that does not require one.
    /// </summary>
    public bool CanCreateClient => !string.IsNullOrEmpty(ApiKey) || IsSelfHosted;

    public static OpenAIEndpointSettings FromEnvironment()
    {
        return Resolve(Environment.GetEnvironmentVariable);
    }

    /// <summary>
    ///     Pure resolution from a variable lookup, separated from the real environment for testability.
    /// </summary>
    public static OpenAIEndpointSettings Resolve(Func<string, string?> getVariable)
    {
        var provider = getVariable("ZORKAI_PROVIDER")?.Trim();
        var explicitUrl = getVariable("OPENAI_BASE_URL")?.Trim();
        var model = NullIfEmpty(getVariable("OPENAI_MODEL"));
        var apiKey = NullIfEmpty(getVariable("OPEN_AI_KEY"));

        string? baseUrl = explicitUrl;
        if (string.IsNullOrEmpty(baseUrl) && !string.IsNullOrEmpty(provider) &&
            !provider.Equals("openai", StringComparison.OrdinalIgnoreCase))
        {
            if (!ProviderDefaultUrls.TryGetValue(provider, out baseUrl))
                throw new InvalidOperationException(
                    $"Unknown ZORKAI_PROVIDER '{provider}'. Valid values: openai, " +
                    $"{string.Join(", ", ProviderDefaultUrls.Keys)} — or set OPENAI_BASE_URL directly " +
                    "for any other OpenAI-compatible server.");
        }

        if (string.IsNullOrEmpty(baseUrl))
            return new OpenAIEndpointSettings(null, apiKey, model);

        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var endpoint))
            throw new InvalidOperationException(
                $"OPENAI_BASE_URL '{baseUrl}' is not a valid absolute URL. " +
                "Expected something like http://localhost:1234/v1.");

        return new OpenAIEndpointSettings(endpoint, apiKey, model);
    }

    /// <summary>
    ///     Creates a chat client for the requested model, honoring the endpoint and model overrides.
    /// </summary>
    public ChatClient CreateClient(string modelName)
    {
        var model = ModelOverride ?? modelName;

        if (Endpoint is null)
            return new ChatClient(model, ApiKey ?? throw new InvalidOperationException(
                "An OpenAI API key is required when no custom endpoint is configured."));

        return new ChatClient(
            model,
            new ApiKeyCredential(ApiKey ?? KeylessPlaceholder),
            new OpenAIClientOptions { Endpoint = Endpoint });
    }

    private static string? NullIfEmpty(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
