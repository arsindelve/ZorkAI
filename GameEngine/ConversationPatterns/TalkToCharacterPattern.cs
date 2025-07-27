using Model.AIGeneration;
using Model.Interface;
using Model.Item;

namespace GameEngine.ConversationPatterns;

/// <summary>
/// Handles the "talk to [character]" conversation pattern
/// </summary>
public class TalkToCharacterPattern : PatternBase
{
    public override async Task<string?> TryMatch(string input, string inputLower, List<ICanBeTalkedTo> talkables, IContext context, IGenerationClient client)
    {
        if (!inputLower.StartsWith("talk to ") && !inputLower.StartsWith("speak to ") && !inputLower.StartsWith("speak with ") && !inputLower.StartsWith("talk with "))
            return null;

        string characterPart;
        if (inputLower.StartsWith("talk to "))
            characterPart = inputLower.Substring("talk to ".Length).Trim();
        else if (inputLower.StartsWith("speak to "))
            characterPart = inputLower.Substring("speak to ".Length).Trim();
        else if (inputLower.StartsWith("speak with "))
            characterPart = inputLower.Substring("speak with ".Length).Trim();
        else
            characterPart = inputLower.Substring("talk with ".Length).Trim();

        var match = FindMatchingTalkable(characterPart, talkables);
        if (match.HasValue)
        {
            return await match.Value.talkable.OnBeingTalkedTo("hello", context, client);
        }

        return null;
    }
}
