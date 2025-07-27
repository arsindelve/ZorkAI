using Model.AIGeneration;
using Model.Interface;
using Model.Item;

namespace GameEngine.ConversationPatterns;

/// <summary>
/// Handles various verb-based conversation patterns like:
/// - "[verb] [character] [text]" (tell bob hello)
/// - "[verb] [to/at] [character] [text]" (say to bob hello)
/// </summary>
public class VerbCharacterPattern : PatternBase
{
    public override async Task<string?> TryMatch(string input, string inputLower, List<ICanBeTalkedTo> talkables, IContext context, IGenerationClient client)
    {
        foreach (var talkable in talkables)
        {
            if (talkable is not IItem item)
                continue;

            foreach (var noun in item.NounsForMatching)
            {
                var lowerNoun = noun.ToLowerInvariant();

                foreach (var verb in Verbs.SayVerbs)
                {
                    var verbLower = verb.ToLowerInvariant();

                    // Pattern: verb noun (tell bob hello)
                    var prefix = verbLower + " " + lowerNoun;
                    if (inputLower.StartsWith(prefix))
                    {
                        var text = input.Substring(prefix.Length).TrimStart(' ', '.', ',', ':').Trim();
                        text = StripOuterQuotes(text);

                        // Special case for "tell [character] to [action]"
                        if (verbLower == "tell" && text.StartsWith("to "))
                        {
                            text = text.Substring(3).Trim(); // Remove the "to " part
                        }

                        return await talkable.OnBeingTalkedTo(text, context, client);
                    }

                    // Pattern: verb [to|at] noun (say to bob hello)
                    foreach (var prep in new[] { "to", "at" })
                    {
                        var prefixWithPrep = verbLower + " " + prep + " " + lowerNoun;
                        if (inputLower.StartsWith(prefixWithPrep))
                        {
                            var text = input.Substring(prefixWithPrep.Length).TrimStart(' ', '.', ',', ':').Trim();
                            text = StripOuterQuotes(text);
                            return await talkable.OnBeingTalkedTo(text, context, client);
                        }
                    }
                }
            }
        }

        return null;
    }
}
