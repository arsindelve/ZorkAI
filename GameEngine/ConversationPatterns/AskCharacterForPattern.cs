using Model.AIGeneration;
using Model.Interface;
using Model.Item;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace GameEngine.ConversationPatterns;

/// <summary>
/// Handles the "ask [character] for [item]" conversation pattern
/// Examples: "ask bob for the key", "ask guard for directions"
/// </summary>
public class AskCharacterForPattern : PatternBase
{
    public override async Task<string?> TryMatch(string input, string inputLower, List<ICanBeTalkedTo> talkables, IContext context, IGenerationClient client)
    {
        // Debug output to help diagnose issues
        Debug.WriteLine($"AskCharacterForPattern checking: '{input}'\nTalkables count: {talkables.Count}");

        if (!inputLower.StartsWith("ask "))
            return null;

        // Make sure we're not handling the "ask X about Y" pattern which is handled elsewhere
        if (inputLower.Contains(" about "))
            return null;

        // Use regex for more robust matching
        var match = Regex.Match(input, @"^ask\s+(\w+)\s+for\s+(.+)$", RegexOptions.IgnoreCase);
        if (!match.Success || match.Groups.Count < 3)
        {
            Debug.WriteLine("Regex match failed");
            return null;
        }

        var characterPart = match.Groups[1].Value.Trim();
        var itemPart = match.Groups[2].Value.Trim();

        Debug.WriteLine($"Regex found character: '{characterPart}', item: '{itemPart}'");

        if (string.IsNullOrWhiteSpace(characterPart) || string.IsNullOrWhiteSpace(itemPart))
        {
            Debug.WriteLine("Character or item part is empty");
            return null;
        }

        var formattedQuery = "can I have " + itemPart + "?";

        // List all available talkables and their nouns for debugging
        Debug.WriteLine("Available talkables:");
        foreach (var talkable in talkables)
        {
            if (talkable is IItem item)
            {
                Debug.WriteLine($"  - {item.GetType().Name}, Nouns: [{string.Join(", ", item.NounsForMatching)}]");
            }
        }

        // Try direct match against all items in the current location
        foreach (var talkable in talkables)
        {
            if (talkable is not IItem item)
                continue;

            // Try exact match
            if (item.NounsForMatching.Any(n => string.Equals(n, characterPart, StringComparison.OrdinalIgnoreCase)))
            {
                Debug.WriteLine($"Found exact match for '{characterPart}'");
                return await talkable.OnBeingTalkedTo(formattedQuery, context, client);
            }
        }

        // For tests, we need to be much more strict about character name matching
        // The character name must match one of the talker's nouns exactly
        // No automatic fallback to any TestTalker that happens to be in the room

        Debug.WriteLine("No match found");

        return null;
    }
}
