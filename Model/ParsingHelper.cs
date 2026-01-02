using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Model.Intent;
using Model.Movement;

namespace Model;

public static class ParsingHelper
{
    public static readonly string TakeUserPrompt = """
                                                         The player is in this location: 
                                                         -------------------------
                                                         {0}
                                                         -------------------------
                                                         They wrote: "{1}"
                                                         
                                                         Reply with a json array, containing a list of nouns they wish to take. Provide single nouns only, no adjectives or descriptive words. Respond only in JSON array format, and do not preface your response with 'json'. Example: [\"item1\", \"item2\", \"item3\"]"
                                                         """;
    
    public static readonly string DropUserPrompt = """
                                                   The player has the following items in their inventory 
                                                   -------------------------
                                                   {0}
                                                   -------------------------
                                                   They wrote: "{1}"

                                                   Reply with a json array, containing a list of single nouns they wish to drop. Provide single nouns only, no adjectives or descriptive words. Respond only in JSON array format, and do not preface your response with 'json'. Example: [\"item1\", \"item2\", \"item3\"]"
                                                   """;
    
    public static readonly string SystemPrompt =
        """
        You are a parser for an interactive fiction game. The player is in this location: "{0}"

        I need to know the player's intent. Given the sentence "{1}":

        1. Tell me in <intent> tags if:
            a) If the player is expressing a desire to move, enter, exit, go in, or travel somewhere, put "move"
            b) If the player wants to enter a vehicle or sub-location, put "board"
            c) If the player wants to exit a vehicle or sub-location, put "disembark"
            d) If the player wants to take or pick up one or more items, put "take" (EXCEPTION: if "take" is used WITH a tool or another object using prepositions like "with" or "using", put "act" instead)
            e) If the player wants to drop one or more items, put "drop"
            f) If the players want to "look" or "look around" or asks "where am I?", put "look"
            g) If the player wants to know what they are carrying, what is in their inventory or what items they have, put "inventory"
            h) Something else, put "Act"
             
        2. In <verb> tags, put the single most important verb I need to know, which best expresses the player's intention. If there is a simpler, more common synonym for the verb, use that instead.
           To avoid confusion, if the player wants to turn something on, or turn on something like a light, use the verb "activate". if the player wants to turn something off, or turn off something light a lamp, use the verb "deactivate"
           To avoid confusion, if the player wants wear or put on clothing, replace their verb with the verb "don". If the player wants take off clothing, or remove clothing, replace their verb with the the verb "doff"

        3. For each noun phrase that is an argument or modifier of the main verb, wrap the head noun (and any immediately preceding adjectives) in <noun>…</noun> tags. Do NOT include any preceding preposition (e.g., 'with', 'under') inside the tags

        4. If there are two nouns, in separate <preposition> tags outside any other tags, put the preposition which connects the nouns. Otherwise, omit these tags.

        5. If the sentence expresses a desire to move in a certain direction or go a certain way based on their current location, put in <direction> tags the exact word from this list which 
        best describes where they want to go: "in, out, enter, exit, up, down, east, west, north, south, north-west, north-east, south-west, or south-east." If the sentence includes a term like 
        'follow' or 'go towards' combined with a specific location described in the player's current environment, use the corresponding direction. If you cannot match any of these words, put "other."

        Do not provide any analysis or explanation, just the tags.

        Examples: 

        "prompt": "type 1", "completion": "<intent>act</intent>\n<verb>type</verb>\n<noun>1</noun>"
        "prompt": "press 0", "completion": "<intent>act</intent>\n<verb>press</verb>\n<noun>0</noun>"
        "prompt": "drop the sword", "completion": "<intent>drop</intent>\n<verb>drop</verb>\n<noun>sword</noun>"
        "prompt": "look under the rug", "completion": "<intent>act</intent>\n<verb>look</verb>\n<noun>rug</noun>"
        "prompt": "take the sword", "completion": "<intent>take</intent>\n<verb>take</verb>\n<noun>sword</noun>"
        "prompt": "pull the lever", "completion": "<intent>act</intent>\n<verb>pull</verb>\n<noun>lever</noun>"
        "prompt": "put on the hat", "completion": "<intent>act</intent>\n<verb>don</verb>\n<noun>hat</noun>"
        "prompt": "inflate the pile of plastic with the air pump", "completion":"<intent>act</intent>\n<verb>inflate</verb>\n<noun>pile of plastic</noun>\n<noun>air pump</noun>\n<preposition>with</preposition>"
        "prompt": "put on the jacket", "completion": "<intent>act</intent>\n<verb>don</verb>\n<noun>jacket</noun>"
        "prompt": "turn on lamp", "completion": "<intent>act</intent>\n<verb>activate</verb>\n<noun>lamp</noun>"
        "prompt": "exit the boat", "completion": "<intent>disembark</intent>\n<noun>boat</noun>"
        "prompt": "take off the jacket", "completion": "<intent>act</intent>\n<verb>doff</verb>\n<noun>jacket</noun>"
        "prompt": "tie the rope to the railing", "completion": "<intent>act</intent>\n<verb>tie</verb>\n<noun>rope</noun>\n<noun>railing</noun>\n<preposition>to</preposition>"
        "prompt": "take the bedistor with pliers", "completion": "<intent>act</intent>\n<verb>take</verb>\n<noun>bedistor</noun>\n<noun>pliers</noun>\n<preposition>with</preposition>"
        "prompt": "remove the bedistor using pliers", "completion": "<intent>act</intent>\n<verb>remove</verb>\n<noun>bedistor</noun>\n<noun>pliers</noun>\n<preposition>using</preposition>"
        "prompt": "where am I?", "completion": "<intent>look</intent>"
        "prompt": "what is this place?", "completion": "<intent>look</intent>"
        "prompt": "what items do I have on me?", "completion": "<intent>inventory</intent>"
        "prompt": "tell me all the items I am carrying", "completion": "<intent>inventory</intent>"

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

        return new ExitSubLocationIntent
            { NounOne = nouns.First(), NounTwo = nouns.LastOrDefault(), Message = response };
    }
    
    private static TakeIntent? DetermineTakeIntent(string? response, string input)
    {
        var intentTag = ExtractElementsByTag(response, "intent").SingleOrDefault();
        if (string.IsNullOrEmpty(intentTag))
            return null;

        if (intentTag != "take")
            return null;

        var nouns = ExtractElementsByTag(response, "noun");
        
        return new TakeIntent
        {
            Message = response,
            OriginalInput = input,
            Noun = nouns.FirstOrDefault()
        };
    }
    
    private static DropIntent? DetermineDropIntent(string? response, string input)
    {
        var intentTag = ExtractElementsByTag(response, "intent").SingleOrDefault();
        if (string.IsNullOrEmpty(intentTag))
            return null;

        if (intentTag != "drop")
            return null;

        var nouns = ExtractElementsByTag(response, "noun");

        return new DropIntent
        {
            Message = response,
            OriginalInput = input,
            Noun = nouns.FirstOrDefault()
        };
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

        return new EnterSubLocationIntent { Noun = nouns.First(), Message = response };
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
        return direction == Direction.Unknown ? null : new MoveIntent { Direction = direction, Message = response };
    }
    
    private static T? DetermineSimpleIntent<T>(string? response) where T : IntentBase, new()
    {
        var tag = typeof(T).GetProperty("TagName")?.GetValue(null) as string;
        var intentTag = ExtractElementsByTag(response, "intent").SingleOrDefault();
        if (string.IsNullOrEmpty(intentTag))
            return null;

        return intentTag != tag ? null : new T();
    }

    private static List<string> ExtractElementsByTag(string? response, string tag)
    {
        var list = new List<string>();
        if (string.IsNullOrEmpty(response))
            return list;
            
        var doc = new HtmlDocument();
        doc.LoadHtml(response);

        HtmlNodeCollection? nodes = doc.DocumentNode.SelectNodes($"//{tag}");
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (nodes != null)
            foreach (var node in nodes)
                list.Add(node.InnerText.Trim());

        return list;
    }

    public static IntentBase GetIntent(string input, string? response, ILogger? logger)
    {
        var takeIntent = DetermineTakeIntent(response?.ToLowerInvariant(), input);
        if (takeIntent != null)
            return takeIntent;
        
        var dropIntent = DetermineDropIntent(response?.ToLowerInvariant(), input);
        if (dropIntent != null)
            return dropIntent;
            
        var boardIntent = DetermineBoardIntent(response?.ToLower());
        if (boardIntent != null)
            return boardIntent;

        var disembarkIntent = DetermineDisembarkIntent(response?.ToLower());
        if (disembarkIntent != null)
            return disembarkIntent;

        var moveIntent = DetermineMoveIntent(response?.ToLowerInvariant());
        if (moveIntent != null)
            return moveIntent;
        
        var inventoryIntent = DetermineSimpleIntent<InventoryIntent>(response?.ToLowerInvariant());
        if (inventoryIntent != null)
            return inventoryIntent;
        
        var lookIntent = DetermineSimpleIntent<LookIntent>(response?.ToLowerInvariant());
        if (lookIntent != null)
            return lookIntent;

        var actionIntent = DetermineActionIntent(response?.ToLowerInvariant(), input, logger);
        if (actionIntent != null)
            return actionIntent;

        return new NullIntent();
    }
}