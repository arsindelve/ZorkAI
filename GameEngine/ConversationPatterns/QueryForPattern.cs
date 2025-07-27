using Model.AIGeneration;
using Model.Interface;
using Model.Item;

namespace GameEngine.ConversationPatterns;

/// <summary>
/// Handles the "query [character] for [information]" conversation pattern
/// </summary>
public class QueryForPattern : PatternBase
{
    public override async Task<string?> TryMatch(string input, string inputLower, List<ICanBeTalkedTo> talkables, IContext context, IGenerationClient client)
    {
        if (!inputLower.StartsWith("query "))
            return null;

        var parts = inputLower.Split(new[] { " for " }, StringSplitOptions.None);
        if (parts.Length < 2)
            return null;

        var characterPart = parts[0].Substring(6).Trim(); // Remove "query "
        var restOfQuery = parts[1].Trim();
        string topicPart;

        // Handle different variations like "query bob for information about X" or just "query bob for X"
        if (restOfQuery.StartsWith("information about "))
        {
            topicPart = restOfQuery.Substring("information about ".Length).Trim();
        }
        else if (restOfQuery.Contains(" about "))
        {
            var aboutParts = restOfQuery.Split(new[] { " about " }, StringSplitOptions.None);
            topicPart = aboutParts[1].Trim();
        }
        else
        {
            topicPart = restOfQuery;
        }

        var formattedQuery = "can you tell me about " + topicPart + "?";

        var match = FindMatchingTalkable(characterPart, talkables);
        if (match.HasValue)
        {
            return await match.Value.talkable.OnBeingTalkedTo(formattedQuery, context, client);
        }

        return null;
    }
}
