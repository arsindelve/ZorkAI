namespace GameEngine;

/// <summary>
///     Whether this process is running without AWS at all: file-backed sessions and saved games, a built-in
///     narrator prompt, a local conversation classifier, and no CloudWatch telemetry (issue #383).
///     <para>
///     This is a single explicit opt-in and is deliberately <b>not</b> inferred from the AI endpoint
///     configuration. <c>OPENAI_BASE_URL</c> and <c>ZORKAI_PROVIDER</c> say where the <i>model</i>
///     lives, which is a different question from whether we have AWS — and <c>OPENAI_BASE_URL</c> in
///     particular is a generic name that gateways, proxies and Azure OpenAI also use. Inferring from
///     it would mean that pointing a <i>deployed</i> Lambda at an LLM proxy silently moved every
///     player's session and saved game from DynamoDB onto the function's ephemeral filesystem, which
///     presents as data loss rather than as a misconfiguration. The same reasoning is already written
///     down for <c>GameEngine.CloudLoggingEnabled</c>; this is that rule applied to storage, where the
///     blast radius is larger.
///     </para>
///     <para>
///     The console keeps its one-flag convenience: <c>--provider ollama</c> still gives fully local
///     play, because Program.cs <i>sets</i> this variable when it sees a custom endpoint, rather than
///     leaving every downstream consumer to independently infer the same thing.
///     </para>
/// </summary>
public static class SelfHostedMode
{
    public const string EnvironmentVariableName = "ZORKAI_SELF_HOSTED";

    /// <summary>Values that count as "yes". Anything else, including unset, means cloud mode.</summary>
    private static readonly string[] Affirmative = ["1", "true", "yes", "on"];

    /// <summary>True when the environment explicitly opts this process into self-hosted mode.</summary>
    public static bool IsEnabled => Resolve(Environment.GetEnvironmentVariable);

    /// <summary>
    ///     Pure resolution from a variable lookup, separated from the real environment for testability.
    /// </summary>
    public static bool Resolve(Func<string, string?> getVariable)
    {
        var value = getVariable(EnvironmentVariableName)?.Trim();

        return !string.IsNullOrEmpty(value) &&
               Affirmative.Contains(value, StringComparer.OrdinalIgnoreCase);
    }
}
