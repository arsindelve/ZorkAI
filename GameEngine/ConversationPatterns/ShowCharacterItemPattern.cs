using Model.AIGeneration;
using Model.Interface;
using Model.Item;

namespace GameEngine.ConversationPatterns;

/// <summary>
/// Handles the "show [item] to [character]" conversation pattern
/// </summary>
public class ShowCharacterItemPattern : PatternBase
{
    private static readonly string[] ShowVerbs = ["show", "present", "display"];

    public override async Task<string?> TryMatch(string input, string inputLower, List<ICanBeTalkedTo> talkables, IContext context, IGenerationClient client)
    {
        foreach (var verb in ShowVerbs)
        {
            if (inputLower.StartsWith(verb + " "))
            {
                // Look for " to " or " at " in the input
                int toIndex = inputLower.IndexOf(" to ", verb.Length, StringComparison.Ordinal);
                if (toIndex < 0)
                    toIndex = inputLower.IndexOf(" at ", verb.Length, StringComparison.Ordinal);

                if (toIndex > verb.Length) // Found after the verb
                {
                    var itemPart = input.Substring(verb.Length + 1, toIndex - verb.Length - 1).Trim();
                    var characterPart = input.Substring(toIndex + 4).Trim(); // +4 to skip " to "/" at "

                    foreach (var talkable in talkables)
                    {
                        if (talkable is not IItem item)
                            continue;

                        foreach (var noun in item.NounsForMatching)
                        {
                            if (string.Equals(noun, characterPart, StringComparison.OrdinalIgnoreCase))
                            {
                                return await talkable.OnBeingTalkedTo("look at this " + itemPart, context, client);
                            }
                        }
                    }
                }
            }
        }

        return null;
    }
}
