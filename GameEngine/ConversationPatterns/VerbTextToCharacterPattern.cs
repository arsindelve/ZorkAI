using Model.AIGeneration;
using Model.Interface;
using Model.Item;

namespace GameEngine.ConversationPatterns;

/// <summary>
/// Handles the "[verb] [text] [to|at] [character]" conversation pattern
/// Examples: "say 'hi' to bob", "yell 'get lost' at guard"
/// </summary>
public class VerbTextToCharacterPattern : PatternBase
{
    public override async Task<string?> TryMatch(string input, string inputLower, List<ICanBeTalkedTo> talkables, IContext context, IGenerationClient client)
    {
        var words = inputLower.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length < 2)
            return null;

        var firstWord = words[0];
        var lastWord = words[^1].TrimEnd('.', '!', '?', '\'', '\"');

        // Check if first word is a say verb
        if (!Verbs.SayVerbs.Any(v => string.Equals(v, firstWord, StringComparison.OrdinalIgnoreCase)))
            return null;

        foreach (var talkable in talkables)
        {
            if (talkable is not IItem item)
                continue;

            foreach (var noun in item.NounsForMatching)
            {
                var lowerNoun = noun.ToLowerInvariant();

                if (string.Equals(lastWord, lowerNoun, StringComparison.OrdinalIgnoreCase))
                {
                    var middleWords = words.Skip(1).Take(words.Length - 2).ToList();
                    if (middleWords.Count > 0 && (middleWords[^1] == "to" || middleWords[^1] == "at"))
                        middleWords.RemoveAt(middleWords.Count - 1);

                    var text = string.Join(" ", middleWords).TrimStart(' ', '.', ',', ':').Trim();
                    text = StripOuterQuotes(text);
                    return await talkable.OnBeingTalkedTo(text, context, client);
                }
            }
        }

        return null;
    }
}
