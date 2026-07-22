namespace ZorkAI.OpenAI;

/// <summary>
///     Helpers for parsing JSON out of raw LLM output. Cloud models with a JSON response format
///     return clean JSON, but self-hosted OpenAI-compatible servers (issue #383) frequently wrap it
///     in markdown code fences or add chatter around it — and some reject the JSON response format
///     outright, so callers fall back to free-form output and need tolerant extraction.
/// </summary>
public static class LlmJson
{
    /// <summary>
    ///     Extracts the first JSON object from raw model output: strips markdown code fences and
    ///     any prose before/after the outermost braces. Returns null when no object is present.
    /// </summary>
    public static string? ExtractJsonObject(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        var text = StripCodeFences(raw);

        var start = text.IndexOf('{');
        var end = text.LastIndexOf('}');
        if (start < 0 || end <= start)
            return null;

        return text.Substring(start, end - start + 1);
    }

    /// <summary>
    ///     Removes surrounding markdown code fences (``` or ```json) if present.
    /// </summary>
    public static string StripCodeFences(string raw)
    {
        var text = raw.Trim();
        if (!text.StartsWith("```", StringComparison.Ordinal))
            return text;

        var firstNewline = text.IndexOf('\n');
        if (firstNewline < 0)
            return text;

        text = text[(firstNewline + 1)..];
        var closingFence = text.LastIndexOf("```", StringComparison.Ordinal);
        if (closingFence >= 0)
            text = text[..closingFence];

        return text.Trim();
    }
}
