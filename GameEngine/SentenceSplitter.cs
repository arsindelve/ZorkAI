using System.Text;

namespace GameEngine;

/// <summary>
///     Splits user input into multiple sentences based on period delimiters,
///     while intelligently handling common abbreviations and edge cases.
/// </summary>
public static class SentenceSplitter
{
    private static readonly HashSet<string> CommonAbbreviations = new()
    {
        "mr", "mrs", "ms", "dr", "prof", "sr", "jr", "st", "vs", "etc", "no", "corp", "inc"
    };

    /// <summary>
    ///     Splits input by periods, avoiding false positives from abbreviations.
    ///     Examples:
    ///     - "take lamp. go north" → ["take lamp", "go north"]
    ///     - "look.wait.wait" → ["look", "wait", "wait"]
    ///     - "talk to Mr. Jones" → ["talk to Mr. Jones"]
    ///     - "e. w." → ["e.", "w."]
    /// </summary>
    /// <param name="input">The raw user input</param>
    /// <returns>List of individual sentences/commands</returns>
    public static List<string> Split(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return new List<string>();

        // First, split on periods
        var parts = input.Split('.');
        var sentences = new List<string>();

        for (var i = 0; i < parts.Length; i++)
        {
            var part = parts[i].Trim();

            // Skip empty parts
            if (string.IsNullOrWhiteSpace(part))
                continue;

            // Check if this part ends with an abbreviation
            var words = part.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length > 0)
            {
                var lastWord = words[^1].ToLower();

                // If the last word is an abbreviation and there's a next part,
                // merge this part with the next
                if (CommonAbbreviations.Contains(lastWord) && i < parts.Length - 1)
                {
                    // Reconstruct with period and continue to next iteration
                    // Trim the next part to avoid double spaces
                    var nextPart = i + 1 < parts.Length ? parts[i + 1].Trim() : "";
                    parts[i + 1] = part + ". " + nextPart;
                    continue;
                }
            }

            // Check if this is a single letter (directional command like "n", "e", etc.)
            // AND it's not part of a larger command
            if (part.Length == 1 && char.IsLetter(part[0]))
            {
                sentences.Add(part + ".");
            }
            else if (words.Length > 1 && words[^1].Length == 1 && char.IsLetter(words[^1][0]))
            {
                // Multi-word command ending with single letter (like "go N")
                // Keep the period
                sentences.Add(part + ".");
            }
            else
            {
                // Regular command - no period needed
                sentences.Add(part);
            }
        }

        // If no sentences were created, return empty list (not the original input)
        return sentences;
    }
}
