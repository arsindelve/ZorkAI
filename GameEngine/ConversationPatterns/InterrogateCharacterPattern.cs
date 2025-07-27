using Model.AIGeneration;
using Model.Interface;
using Model.Item;

namespace GameEngine.ConversationPatterns;

/// <summary>
/// Handles the "interrogate [character]" conversation pattern
/// </summary>
public class InterrogateCharacterPattern : PatternBase
{
    public override async Task<string?> TryMatch(string input, string inputLower, List<ICanBeTalkedTo> talkables, IContext context, IGenerationClient client)
    {
        if (!inputLower.StartsWith("interrogate "))
            return null;

        var characterPart = input.Substring("interrogate ".Length).Trim();

        foreach (var talkable in talkables)
        {
            if (talkable is not IItem item)
                continue;

            foreach (var noun in item.NounsForMatching)
            {
                if (string.Equals(noun, characterPart, StringComparison.OrdinalIgnoreCase))
                {
                    return await talkable.OnBeingTalkedTo("Tell me everything you know.", context, client);
                }
            }
        }

        return null;
    }
}
