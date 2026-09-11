using Microsoft.Extensions.Logging;
using Model.Interface;
using SecretsManager;

namespace GameEngine.Web;

/// <summary>
///     Resolves the OpenAI API key that <c>OpenAIClientBase</c> reads out of the environment.
///
///     The trap this closes: <c>OPEN_AI_KEY</c> has never appeared in any <c>serverless.template</c>. It
///     was applied by hand to the Zork and Planetfall Lambdas and persisted only because CloudFormation
///     does not manage properties a template never declares. So the deploy of a brand-new game stack
///     reports success, the stack is valid, and every request then 502s during host startup with
///     "Missing environment variable OPEN_AI_KEY" — which is exactly what StationfallStack did on
///     release 2.0.9. Nothing in the repo recorded that the dependency existed.
///
///     The env var stays the first source, so the functions that already carry it (and local dev, and
///     the console app) are untouched and spend no Secrets Manager call per cold start. When it is
///     absent we fall back to the <see cref="SecretName" /> secret, which is real infrastructure the
///     account already owns and the deploy role can read — the same mechanism
///     <c>IInfocomGame.SystemPromptSecretKey</c> already uses for the per-game system prompt.
///
///     Deliberately non-throwing: a missing or unreadable secret leaves the environment untouched so the
///     operator still gets the client's explicit "Missing environment variable OPEN_AI_KEY", rather than
///     an opaque failure raised from inside DI registration.
/// </summary>
public static class OpenAiKeyResolver
{
    public const string EnvironmentVariableName = "OPEN_AI_KEY";
    public const string SecretName = "OpenAiApiKey";

    /// <summary>
    ///     Ensures <see cref="EnvironmentVariableName" /> holds a usable key, fetching it from Secrets
    ///     Manager if the environment does not already supply one.
    /// </summary>
    /// <returns><c>true</c> if a non-blank key is now in the environment.</returns>
    public static async Task<bool> EnsureKeyAvailable(ISecretsManager secretsManager, ILogger? logger = null)
    {
        if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(EnvironmentVariableName)))
            return true;

        try
        {
            var secret = await secretsManager.GetSecret(SecretName);

            // A blank secret is not a key. Leave the environment alone rather than handing the OpenAI
            // client whitespace, which would fail later and further from the cause.
            if (string.IsNullOrWhiteSpace(secret))
            {
                logger?.LogError("Secret {SecretName} is empty; OpenAI generation will be unavailable.",
                    SecretName);
                return false;
            }

            Environment.SetEnvironmentVariable(EnvironmentVariableName, secret);
            logger?.LogInformation("Resolved the OpenAI key from secret {SecretName}.", SecretName);
            return true;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Could not read secret {SecretName}; OpenAI generation will be unavailable.",
                SecretName);
            return false;
        }
    }

    /// <summary>
    ///     Cold-start entry point for the Lambda hosts, which cannot await. Blocking is safe and
    ///     deliberate here: it runs once on the Lambda init thread, where there is no synchronization
    ///     context to deadlock against, and it must complete before the host builds the OpenAI client.
    ///
    ///     Called from each LambdaEntryPoint rather than from ServicesHelper on purpose. A side effect
    ///     buried in DI registration would run inside every Startup unit test and turn them into real
    ///     network calls; LambdaEntryPoint is production-only (its own tests never instantiate it), so
    ///     the unit suite stays offline and deterministic.
    /// </summary>
    public static void EnsureKeyAvailableAtStartup(ILogger? logger = null)
    {
        EnsureKeyAvailable(new AmazonSecretsManager(), logger).GetAwaiter().GetResult();
    }
}
