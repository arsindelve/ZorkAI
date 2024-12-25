using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Model.Intent;
using Model.Movement;

namespace Model;

public static class ParsingHelper
{
    public static readonly string Prompt =
        """
        You are a parser for an interactive fiction game. The player is in this location: "{0}"

        I need to know the player's intent. Given the sentence "{1}":

        1. Tell me in <intent> tags if:
            a) If the player is expressing a desire to move, enter, go in, or travel somewhere, put "Move"
            b) If the player wants to enter a vehicle or sub-location, put "Board"
            c) If the player wants to exit a vehicle or sub-location, put "Disembark"
            d) Something else, put "Act"
             
        2. In <verb> tags, put the single most important verb I need to know, which best expresses the player's intention. If there is a simpler, more common synonym for the verb, use that instead.
           To avoid confusion, if the player wants to turn something on, or turn on something like a light, use the verb "activate". if the player wants to turn something off, or turn off something light a lamp, use the verb "deactivate"
           To avoid confusion, if the player wants wear or put on clothing, replace their verb with the verb "don". If the player wants take off clothing, or remove clothing, replace their verb with the the verb "doff"

        3. For each noun in the sentence that relates to the main verb, place each noun in a set of <noun> tags. If there are adjectives immediately preceding a noun, include it in front of the noun

        4. If there are two nouns, in separate <preposition> tags outside any other tags, put the preposition which connects the nouns. Otherwise, omit these tags.

        5. If the sentence expresses a desire to move in a certain direction or fo a certain way based on their current location, put in <direction> tags the exact word from this list which 
        best describes where they want to go: "in, out, enter, exit, up, down, east, west, north, south, north-west, north-east, south-west, or south-east." If the sentence includes a term like 
        'follow' or 'go towards' combined with a specific location described in the player's current environment, use the corresponding direction. If you cannot match any of these words, put "other."

        Do not provide any analysis or explanation, just the tags.

        Examples: 

        "prompt": "pull the lever", "completion": "<intent>act</intent>\n<verb>pull</verb>\n<noun>lever</noun>"
        "prompt": "put on the hat", "completion": "<intent>act</intent>\n<verb>don</verb>\n<noun>hat</noun>"
        "prompt": "inflate the pile of plastic with the air pump", "completion":"<intent>act</intent>\n<verb>inflate</verb>\n<noun>pile of plastic</noun>\n<noun>air pump</noun>\n<preposition>with</preposition>"
        "prompt": "put on the jacket", "completion": "<intent>act</intent>\n<verb>don</verb>\n<noun>jacket</noun>"
        "prompt": "turn on lamp", "completion": "<intent>act</intent>\n<verb>activate</verb>\n<noun>lamp</noun>"
        "prompt": "exit the boat", "completion": "<intent>disembark</intent>\n<noun>boat</noun>"
        "prompt": "take off the jacket", "completion": "<intent>act</intent>\n<verb>doff</verb>\n<noun>jacket</noun>"
        "prompt": "tie the rope to the railing", "completion": "<intent>act</intent>\n<verb>tie</verb>\n<noun>rope</noun>\n<noun>railing</noun>\n<preposition>to</preposition>"

        """;

    private static ExitSubLocationIntent? DetermineDisembarkIntent(string? response)
    {
        var intentTag = ExtractElementsByTag(response, "intent").SingleOrDefault();
        if (string.IsNullOrEmpty(intentTag))
            return null;

        if (intentTag != "disembark")
            return null;

        var nouns = ExtractElementsByTag(response, "noun");
        if (!nouns.Any())
            return null;

        return new ExitSubLocationIntent { NounOne = nouns.First(), NounTwo = nouns.LastOrDefault(), Message = response};
    }

    private static EnterSubLocationIntent? DetermineBoardIntent(string? response)
    {
        var intentTag = ExtractElementsByTag(response, "intent").SingleOrDefault();
        if (string.IsNullOrEmpty(intentTag))
            return null;

        if (intentTag != "board")
            return null;

        var nouns = ExtractElementsByTag(response, "noun");
        if (!nouns.Any())
            return null;

        return new EnterSubLocationIntent { Noun = nouns.First(), Message = response};
    }

    private static IntentBase? DetermineActionIntent(string? response, string originalInput, ILogger? logger)
    {
        var intentTag = ExtractElementsByTag(response, "intent").SingleOrDefault();
        if (string.IsNullOrEmpty(intentTag))
        {
            logger?.LogDebug("No intent tag was found trying to make an act intent");
            return null;
        }

        if (intentTag != "act")
        {
            logger?.LogDebug("The intent tag was not 'act' trying to make an act intent");
            return null;
        }

        var verbTag = ExtractElementsByTag(response, "verb").SingleOrDefault();
        if (string.IsNullOrEmpty(verbTag))
        {
            logger?.LogDebug("No verb was found trying to make an act intent");
            return null;
        }

        var prepositionTag = ExtractElementsByTag(response, "preposition").SingleOrDefault();

        var nouns = ExtractElementsByTag(response, "noun");
        if (!nouns.Any())
            return null;

        if (nouns.Count == 1)
        {
            var adjectives = ExtractElementsByTag(response, "adjective").FirstOrDefault();
            return new SimpleIntent
            {
                Verb = verbTag,
                Noun = nouns.Single(),
                Adverb = prepositionTag,
                Adjective = adjectives,
                OriginalInput = originalInput,
                Message = response
            };
        }

        if (nouns.Count == 2)
        {
            if (string.IsNullOrEmpty(prepositionTag))
            {
                logger?.LogDebug("No preposition was found trying to make a MultiNoun intent");
                // TODO: Claude is inconsistent giving us the preposition. For now, hardcode the most common one if we don't get it. 
                prepositionTag = "with";
            }

            return new MultiNounIntent
            {
                NounOne = nouns[0],
                NounTwo = nouns[1],
                Verb = verbTag,
                Preposition = prepositionTag,
                OriginalInput = originalInput,
                Message = response
            };
        }

        return null;
    }

    private static MoveIntent? DetermineMoveIntent(string? response)
    {
        var intentTag = ExtractElementsByTag(response, "intent").SingleOrDefault();
        if (string.IsNullOrEmpty(intentTag))
            return null;

        if (intentTag != "move")
            return null;

        var directionTag = ExtractElementsByTag(response, "direction").SingleOrDefault();
        if (string.IsNullOrEmpty(directionTag))
            directionTag = ExtractElementsByTag(response, "verb").SingleOrDefault();
        if (string.IsNullOrEmpty(directionTag))
            return null;

        var direction = DirectionParser.ParseDirection(directionTag);
        return direction == Direction.Unknown ? null : new MoveIntent { Direction = direction, Message = response};
    }

    private static List<string> ExtractElementsByTag(string? response, string tag)
    {
        var list = new List<string>();
        var doc = new HtmlDocument();
        doc.LoadHtml(response);

        var nodes = doc.DocumentNode.SelectNodes($"//{tag}");
        if (nodes != null)
            foreach (var node in nodes)
                list.Add(node.InnerText.Trim());

        return list;
    }

    public static IntentBase GetIntent(string input, string? response, ILogger? logger)
    {
        var boardIntent = DetermineBoardIntent(response?.ToLower());
        if (boardIntent != null)
            return boardIntent;

        var disembarkIntent = DetermineDisembarkIntent(response?.ToLower());
        if (disembarkIntent != null)
            return disembarkIntent;

        var moveIntent = DetermineMoveIntent(response?.ToLowerInvariant());
        if (moveIntent != null)
            return moveIntent;

        var actionIntent = DetermineActionIntent(response?.ToLowerInvariant(), input, logger);
        if (actionIntent != null)
            return actionIntent;

        return new NullIntent();
    }
}