using Model.AIGeneration;
using Model.Interface;
using Model.Item;

namespace GameEngine.ConversationPatterns;

/// <summary>
/// Handles the "ask [character] about [topic]" conversation pattern
/// </summary>
public class AskAboutPattern : PatternBase
{
    public override async Task<string?> TryMatch(string input, string inputLower, List<ICanBeTalkedTo> talkables, IContext context, IGenerationClient client)
    {
        if (!inputLower.StartsWith("ask "))
            return null;

        var parts = inputLower.Split(new[] { " about " }, StringSplitOptions.None);
        if (parts.Length < 2)
            return null;

        var characterPart = parts[0].Substring(4).Trim(); // Remove "ask "
        var aboutPart = "what about " + parts[1].Trim() + "?";

        var match = FindMatchingTalkable(characterPart, talkables);
        if (match.HasValue)
        {
            return await match.Value.talkable.OnBeingTalkedTo(aboutPart, context, client);
        }

        return null;
    }
}
