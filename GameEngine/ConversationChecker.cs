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

        foreach (var talkable in talkables)
        {
            if (talkable is not IItem item)
                continue;

            foreach (var noun in item.NounsForMatching)
            {
                var lowerInput = input.ToLowerInvariant();
                var lowerNoun = noun.ToLowerInvariant();

                if (lowerInput.StartsWith(lowerNoun + ","))
                {
                    var text = input.Substring(noun.Length + 1).Trim();
                    return await talkable.OnBeingTalkedTo(text, context, client);
                }

                foreach (var verb in Verbs.SayVerbs)
                {
                    var verbLower = verb.ToLowerInvariant();

                    // verb noun
                    var prefix = verbLower + " " + lowerNoun;
                    if (lowerInput.StartsWith(prefix))
                    {
                        var text = input.Substring(prefix.Length).TrimStart(' ', '.', ',', ':').Trim();
                        text = StripOuterQuotes(text);
                        return await talkable.OnBeingTalkedTo(text, context, client);
                    }

                    // verb [to|at] noun
                    foreach (var prep in new[] { "to", "at" })
                    {
                        var prefixWithPrep = verbLower + " " + prep + " " + lowerNoun;
                        if (lowerInput.StartsWith(prefixWithPrep))
                        {
                            var text = input.Substring(prefixWithPrep.Length).TrimStart(' ', '.', ',', ':').Trim();
                            text = StripOuterQuotes(text);
                            return await talkable.OnBeingTalkedTo(text, context, client);
                        }
                    }
                }

                // verb text [to|at] noun  (say "hi" to bob)
                var words = lowerInput.Split(' ', StringSplitOptions.RemoveEmptyEntries);
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
