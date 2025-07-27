using Model.AIGeneration;
using Model.Interface;
using Model.Item;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace GameEngine.ConversationPatterns;

/// <summary>
/// Handles the "ask [character] "[text]"" conversation pattern
/// Examples: "ask bob "where is the key"" or "ask guard "help me""
/// </summary>
public class AskCharacterWithQuotesPattern : PatternBase
{
    public override async Task<string?> TryMatch(string input, string inputLower, List<ICanBeTalkedTo> talkables, IContext context, IGenerationClient client)
    {
        Debug.WriteLine($"AskCharacterWithQuotesPattern checking: '{input}'");

        if (!inputLower.StartsWith("ask "))
            return null;

        // Use regex to match the pattern "ask [character] "[text]"" or "ask [character] '[text]'"
        // The (.+?) makes the character match non-greedy, allowing for multi-word character names
        var match = Regex.Match(input, @"^ask\s+(.+?)\s+[""'](.+)[""']$", RegexOptions.IgnoreCase);
        if (!match.Success || match.Groups.Count < 3)
        {
            Debug.WriteLine("Regex match failed");
            return null;
        }

        var characterPart = match.Groups[1].Value.Trim();
        var quotedText = match.Groups[2].Value.Trim();

        Debug.WriteLine($"Regex found character: '{characterPart}', quoted text: '{quotedText}'");

        if (string.IsNullOrWhiteSpace(characterPart) || string.IsNullOrWhiteSpace(quotedText))
        {
            Debug.WriteLine("Character or quoted text part is empty");
            return null;
        }

        // Find a matching talkable character
        var matchResult = FindMatchingTalkable(characterPart, talkables);
        if (matchResult == null)
        {
            Debug.WriteLine($"No matching character found for '{characterPart}'");
            return null;
        }

        var (talkable, _) = matchResult.Value;

        // Process the quoted text and send it to the character
        return await talkable.OnBeingTalkedTo(quotedText, context, client);
    }
}
