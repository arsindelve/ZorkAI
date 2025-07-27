using Model.AIGeneration;
using Model.Interface;
using Model.Item;

namespace GameEngine.ConversationPatterns;

/// <summary>
/// Handles the "[character], [text]" conversation pattern
/// </summary>
public class CharacterCommaPattern : PatternBase
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

                if (inputLower.StartsWith(lowerNoun + ","))
                {
                    var text = input.Substring(noun.Length + 1).Trim();
                    return await talkable.OnBeingTalkedTo(text, context, client);
                }
            }
        }

        return null;
    }
}
