using Model.AIGeneration;
using Model.Interface;
using Model.Item;

namespace GameEngine.ConversationPatterns;

/// <summary>
/// Handles greeting patterns like "greet [character]", "hello [character]"
/// </summary>
public class GreetCharacterPattern : PatternBase
{
    private static readonly string[] GreetingVerbs = ["greet", "hello", "hi"];

    public override async Task<string?> TryMatch(string input, string inputLower, List<ICanBeTalkedTo> talkables, IContext context, IGenerationClient client)
    {
        foreach (var greeting in GreetingVerbs)
        {
            if (inputLower.StartsWith(greeting + " "))
            {
                var characterPart = inputLower.Substring(greeting.Length + 1).Trim();

                var match = FindMatchingTalkable(characterPart, talkables);
                if (match.HasValue)
                {
                    return await match.Value.talkable.OnBeingTalkedTo("hello", context, client);
                }
            }
        }

        return null;
    }
}
