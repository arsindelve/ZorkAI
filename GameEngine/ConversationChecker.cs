using Model.AIGeneration;

namespace GameEngine;

using Model.Item;
using Model.Interface;
using System.Linq;

internal static class ConversationChecker
{
    internal static async Task<string?> CheckForConversation(
        string input,
        IContext context,
        IGenerationClient client)
    {
        var talkables = new List<ICanBeTalkedTo>();

        talkables.AddRange(context.Items.OfType<ICanBeTalkedTo>());
        if (context.CurrentLocation is ICanContainItems container)
            talkables.AddRange(container.Items.OfType<ICanBeTalkedTo>());

        // Convert input to lowercase once for all checks
        string inputLower = input.ToLowerInvariant();

        // Check for "ask [character] about [topic]" pattern
        if (inputLower.StartsWith("ask "))
        {
            var parts = inputLower.Split(new[] { " about " }, StringSplitOptions.None);
            if (parts.Length >= 2)
            {
                var characterPart = parts[0].Substring(4).Trim(); // Remove "ask "
                var aboutPart = "what about " + parts[1].Trim() + "?";

                foreach (var talkable in talkables)
                {
                    if (talkable is not IItem item)
                        continue;

                    if (item.NounsForMatching.Any(noun => string.Equals(noun, characterPart, StringComparison.OrdinalIgnoreCase)))
                    {
                        return await talkable.OnBeingTalkedTo(aboutPart, context, client);
                    }
                }
            }
        }

        // Check for "query [character] for information about [topic]" pattern
        if (inputLower.StartsWith("query "))
        {
            var parts = inputLower.Split(new[] { " for " }, StringSplitOptions.None);
            if (parts.Length >= 2)
            {
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

                foreach (var talkable in talkables)
                {
                    if (talkable is not IItem item)
                        continue;

                    if (item.NounsForMatching.Any(noun => string.Equals(noun, characterPart, StringComparison.OrdinalIgnoreCase)))
                    {
                        return await talkable.OnBeingTalkedTo(formattedQuery, context, client);
                    }
                }
            }
        }

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

                foreach (var verb in Verbs.SayVerbs)
                {
                    var verbLower = verb.ToLowerInvariant();

                    // verb noun
                    var prefix = verbLower + " " + lowerNoun;
                    if (inputLower.StartsWith(prefix))
                    {
                        var text = input.Substring(prefix.Length).TrimStart(' ', '.', ',', ':').Trim();
                        text = StripOuterQuotes(text);

                        // Special case for "tell [character] to [action]"
                        if (verbLower == "tell" && text.StartsWith("to "))
                        {
                            text = text.Substring(3).Trim(); // Remove the "to " part
                        }

                        return await talkable.OnBeingTalkedTo(text, context, client);
                    }

                    // verb [to|at] noun
                    foreach (var prep in new[] { "to", "at" })
                    {
                        var prefixWithPrep = verbLower + " " + prep + " " + lowerNoun;
                        if (inputLower.StartsWith(prefixWithPrep))
                        {
                            var text = input.Substring(prefixWithPrep.Length).TrimStart(' ', '.', ',', ':').Trim();
                            text = StripOuterQuotes(text);
                            return await talkable.OnBeingTalkedTo(text, context, client);
                        }
                    }
                }

                // verb text [to|at] noun  (say "hi" to bob)
                var words = inputLower.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (words.Length >= 2)
                {
                    var firstWord = words[0];
                    var lastWord = words[^1].TrimEnd('.', '!', '?', '\'', '"');

                    if (Verbs.SayVerbs.Any(v => string.Equals(v, firstWord, StringComparison.OrdinalIgnoreCase)) &&
                        string.Equals(lastWord, lowerNoun, StringComparison.OrdinalIgnoreCase))
                    {
                        var middleWords = words.Skip(1).Take(words.Length - 2).ToList();
                        if (middleWords.Count > 0 && (middleWords[^1] == "to" || middleWords[^1] == "at"))
                            middleWords.RemoveAt(middleWords.Count - 1);

                        var text = string.Join(" ", middleWords).TrimStart(' ', '.', ',', ':').Trim();
                        text = StripOuterQuotes(text);
                        return await talkable.OnBeingTalkedTo(text, context, client);
                    }
                }
            }
        }

        return null;
    }

    private static string StripOuterQuotes(string text)
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
