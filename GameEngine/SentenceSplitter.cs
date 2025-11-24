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
    ///     - "talk to Mr. Jones" → ["talk to Mr. Jones"]
    ///     - "go N." → ["go N."]
    /// </summary>
    /// <param name="input">The raw user input</param>
    /// <returns>List of individual sentences/commands</returns>
    public static List<string> Split(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return new List<string>();

        var sentences = new List<string>();
        var currentSentence = new StringBuilder();

        var tokens = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        for (var i = 0; i < tokens.Length; i++)
        {
            var token = tokens[i];
            currentSentence.Append(token);

            // Check if token ends with period
            if (token.EndsWith('.'))
            {
                var withoutPeriod = token.TrimEnd('.').ToLower();

                // Check if it's an abbreviation (Mr., Dr., etc.)
                if (CommonAbbreviations.Contains(withoutPeriod))
                {
                    currentSentence.Append(' ');
                    continue;
                }

                // Check if it's a single letter followed by period (like "N." for North)
                // Keep it as part of current sentence - it's likely a direction command
                if (withoutPeriod.Length == 1)
                {
                    // If this is the last token or next token doesn't start with capital,
                    // treat it as end of sentence
                    if (i == tokens.Length - 1)
                    {
                        var sentence = currentSentence.ToString().Trim();
                        if (!string.IsNullOrWhiteSpace(sentence))
                        {
                            sentences.Add(sentence);
                        }
                        currentSentence.Clear();
                    }
                    else
                    {
                        currentSentence.Append(' ');
                    }
                    continue;
                }

                // It's a sentence delimiter - add the sentence without the period
                var completeSentence = currentSentence.ToString().TrimEnd('.').Trim();
                if (!string.IsNullOrWhiteSpace(completeSentence))
                {
                    sentences.Add(completeSentence);
                }
                currentSentence.Clear();
            }
            else if (i < tokens.Length - 1)
            {
                currentSentence.Append(' ');
            }
        }

        // Add any remaining content
        var final = currentSentence.ToString().TrimEnd('.').Trim();
        if (!string.IsNullOrWhiteSpace(final))
        {
            sentences.Add(final);
        }

        // If no sentences were created, return the original input as a single sentence
        return sentences.Count > 0 ? sentences : new List<string> { input.Trim() };
    }
}
