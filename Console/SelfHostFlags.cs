namespace ZorkConsole;

/// <summary>
///     Maps the console's optional self-hosting flags onto the environment variables
///     <c>OpenAIEndpointSettings</c> reads, so <c>ZorkOne --provider ollama --model llama3.1</c> works
///     without exporting anything first (issue #383). Flags win over pre-existing variables.
///     <para>
///     Parsing is separated from applying so it is unit-testable without mutating the process
///     environment, the same shape as <see cref="GameArgumentResolver" />.
///     </para>
/// </summary>
public static class SelfHostFlags
{
    /// <summary>The flags this accepts, and the environment variable each one sets.</summary>
    private static readonly Dictionary<string, string> FlagVariables = new(StringComparer.OrdinalIgnoreCase)
    {
        ["--provider"] = "ZORKAI_PROVIDER",
        ["--endpoint"] = "OPENAI_BASE_URL",
        ["--model"] = "OPENAI_MODEL"
    };

    /// <summary>
    ///     Reads the flags out of the process arguments, returning variable name to value. Later
    ///     occurrences of the same flag win, matching how a shell would treat a repeated option.
    /// </summary>
    public static Dictionary<string, string> Parse(string[]? args)
    {
        var resolved = new Dictionary<string, string>();

        if (args is null)
            return resolved;

        // Starts at 1 because args[0] is the game name. On a match the index advances past the value
        // as well, so a consumed value can never be re-read as a flag: without that,
        // "--model --provider ollama" set OPENAI_MODEL to "--provider" *and* ZORKAI_PROVIDER to
        // "ollama", which is two wrong answers from one typo.
        for (var i = 1; i < args.Length - 1; i++)
        {
            if (!FlagVariables.TryGetValue(args[i], out var variable))
                continue;

            resolved[variable] = args[i + 1];
            i++;
        }

        return resolved;
    }

    /// <summary>Applies <see cref="Parse" />'s result to the process environment.</summary>
    public static void ApplyToEnvironment(string[]? args)
    {
        foreach (var (variable, value) in Parse(args))
            Environment.SetEnvironmentVariable(variable, value);
    }
}
