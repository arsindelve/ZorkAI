using Model.AIGeneration;
using Model.Interface;
using Model.Item;

namespace GameEngine.ConversationPatterns;

/// <summary>
/// Base class for conversation patterns with common utility methods
/// </summary>
public abstract class PatternBase : IConversationPattern
{
    /// <summary>
    /// Attempts to match a user input against this conversation pattern
    /// </summary>
    /// <param name="input">The original user input</param>
    /// <param name="inputLower">The lowercase version of the input</param>
    /// <param name="talkables">List of entities that can be talked to</param>
    /// <param name="context">The game context</param>
    /// <param name="client">The generation client</param>
    /// <returns>The conversation result if this pattern matched, otherwise null</returns>
    public abstract Task<string?> TryMatch(string input, string inputLower, List<ICanBeTalkedTo> talkables, IContext context, IGenerationClient client);

    /// <summary>
    /// Finds a talkable entity that matches a given character name
    /// </summary>
    /// <param name="characterName">The name to match</param>
    /// <param name="talkables">List of entities that can be talked to</param>
    /// <returns>The matching talkable entity and item, or null if none found</returns>
    protected (ICanBeTalkedTo talkable, IItem item)? FindMatchingTalkable(string characterName, List<ICanBeTalkedTo> talkables)
    {
        if (string.IsNullOrEmpty(characterName))
            return null;

        // Try exact match first
        foreach (var talkable in talkables)
        {
            if (talkable is not IItem item)
                continue;

            foreach (var noun in item.NounsForMatching)
            {
                if (string.Equals(noun, characterName, StringComparison.OrdinalIgnoreCase))
                {
                    return (talkable, item);
                }
            }
        }

        // Try substring match if exact match fails
        foreach (var talkable in talkables)
        {
            if (talkable is not IItem item)
                continue;

            if (item.NounsForMatching.Any(noun => 
                noun.Contains(characterName, StringComparison.OrdinalIgnoreCase) || 
                characterName.Contains(noun, StringComparison.OrdinalIgnoreCase)))
            {
                return (talkable, item);
            }
        }

        return null;
    }

    /// <summary>
    /// Removes outer quotes from text
    /// </summary>
    protected static string StripOuterQuotes(string text)
    {
        if (text.Length >= 2)
        {
            if ((text.StartsWith("\"") && text.EndsWith("\"")) ||
                (text.StartsWith("'") && text.EndsWith("'")))
            {
                return text[1..^1];
            }
        }

        return text;
    }
}
