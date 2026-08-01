using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Model.Intent;
using Model.Movement;

namespace Model;

public static class ParsingHelper
{
    // Issue #423: whole-scene nouns that mean "the room itself", not a specific object. When gpt-4o tags a
    // non-exact room-look ("look at the room", "look around the area") as intent=look with one of these as
    // the noun, it is still a bare room-look and must render the room — NOT be redirected to a targeted
    // examine of a non-object noun. (The common exact forms — "look", "look around", "examine
    // surroundings" — never reach the parser; they are global commands. See GlobalCommandFactory.)
    private static readonly HashSet<string> WholeSceneNouns = new(StringComparer.OrdinalIgnoreCase)
    {
        "room", "area", "surroundings", "surrounding", "here", "everything", "around", "place", "vicinity",
        "scene"
    };

    public static readonly string TakeUserPrompt = """
                                                         The player is in this location:
                                                         -------------------------
                                                         {0}
                                                         -------------------------
                                                         They wrote: "{1}"

                                                         Reply with a JSON object containing an "items" array with the item name(s) they wish to take. Keep compound nouns together as single items (e.g., "id card", "brass lantern", "scrub brush"). Only return multiple array elements if the player wants multiple different items. Example: {{"items": ["id card"]}} or {{"items": ["sword", "shield"]}}
                                                         """;
    
    public static readonly string DropUserPrompt = """
                                                   The player has the following items in their inventory:
                                                   -------------------------
                                                   {0}
                                                   -------------------------
                                                   They wrote: "{1}"

                                                   Reply with a JSON object containing an "items" array with the item name(s) they wish to drop. Keep compound nouns together as single items (e.g., "id card", "brass lantern", "scrub brush"). Only return multiple array elements if the player wants multiple different items. Example: {{"items": ["id card"]}} or {{"items": ["sword", "shield"]}}
                                                   """;
    
    public static readonly string SystemPrompt =
        """
        You are a parser for an interactive fiction game. The player is in this location: "{0}"

        I need to know the player's intent. Given the sentence "{1}":

        1. Tell me in <intent> tags if:
            a) If the player is expressing a desire to move, go, or travel in a cardinal or relative DIRECTION (e.g. north, south, up, down, in, out), put "move"
            b) If the player wants to travel to a SPECIFIC NAMED place or room rather than a direction (e.g. "go to the kitchen", "walk into the shuttle", "head to the reactor", "enter the mess"), put "goto". In the <noun> tags, put the destination, NORMALIZING a colloquial or slang place-name to the common room noun it refers to (e.g. "the galley" -> "kitchen"; "the train" -> "shuttle"; "the cafeteria" -> "mess hall"; "the loo" -> "bathroom"). Keep a plain room name as-is.
            c) If the player wants to enter a vehicle or sub-location, put "board"
            d) If the player wants to exit a vehicle or sub-location, put "disembark"
            e) If the player wants to take or pick up one or more items, put "take" (EXCEPTION: if "take" is used WITH a tool or another object using prepositions like "with" or "using", put "act" instead)
            f) If the player wants to drop one or more items, put "drop"
            g) If the players want to "look" or "look around" or asks "where am I?", put "look"
            h) If the player wants to know what they are carrying, what is in their inventory or what items they have, put "inventory"
            i) Something else, put "Act"
             
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
        "prompt": "go to the kitchen", "completion": "<intent>goto</intent>\n<noun>kitchen</noun>"
        "prompt": "walk into the shuttle", "completion": "<intent>goto</intent>\n<noun>shuttle</noun>"
        "prompt": "enter the mess", "completion": "<intent>goto</intent>\n<noun>mess</noun>"
        "prompt": "go to the galley", "completion": "<intent>goto</intent>\n<noun>kitchen</noun>"
        "prompt": "head to the reactor", "completion": "<intent>goto</intent>\n<noun>reactor</noun>"
        "prompt": "enter the dome room", "completion": "<intent>goto</intent>\n<noun>dome room</noun>"
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

        // Issue #538: the system prompt's own rule 1e carves out "take <thing> with <tool>" as an
        // "act", not a "take" — precisely because the model tends to bucket it as a take anyway. When
        // it does slip (intermittently, for the same input), the two-noun command collapsed into a
        // TakeIntent that keeps only the FIRST noun, so the tool is silently dropped, the multi-noun
        // handler (e.g. FusedBedistor's "take fused with pliers") never runs, and the turn falls
        // through to the AI narrator with no engine response. A two-noun take joined by a TOOL
        // preposition is always the action form, so let it fall through to DetermineActionIntent.
        // Scoped to the tool prepositions on purpose: "take the sword and the shield" is a genuine
        // multi-item take and must keep reaching TakeProcessor. Scoped to exactly two nouns because
        // that is the only count DetermineActionIntent can turn into a MultiNounIntent — any other
        // count must stay a take rather than degrade into a NullIntent.
        if (nouns.Count == 2 && UsesAToolPreposition(response, input))
            return null;

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

        // Issue #423: "act" is the normal action bucket, but gpt-4o also drops a *targeted* look
        // ("look through the window", "peer through the crack") into intent=look — the bare "look around"
        // bucket — while still emitting the <verb>/<noun> tags. GetIntent only reaches here for a "look"
        // intent once it has confirmed a noun is present (an object-less look already returned a
        // LookIntent), so treat that noun-bearing look as an action too, rather than a bare room-look
        // that silently swallows the noun.
        // Issue #538: a "take" only reaches here when DetermineTakeIntent has already declined it as a
        // tool-assisted two-noun command ("take the bedistor with the pliers") — the shape the system
        // prompt's rule 1e defines as an action. Accept it here so it becomes the MultiNounIntent its
        // handler is waiting for instead of collapsing to a NullIntent and the narrator.
        if (intentTag != "act" && intentTag != "look" && intentTag != "take")
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

                // Issue #538: the model is inconsistent about emitting <preposition> — the SAME
                // two-noun command produces the tag on one turn and omits it on the next. The old
                // fallback hardcoded "with", which is correct only for the "with" family; every other
                // prepositional command ("set dial to 419", "slide card through slot", "put bedistor
                // in panel") then reached its handler carrying a preposition the player never typed,
                // so every MatchPreposition guard missed and the turn fell through to the AI narrator
                // with no engine response at all. That is why two identical consecutive commands could
                // have opposite outcomes, and why "unlock padlock with key" never showed the bug.
                // The player's own words are ground truth for which preposition joins the two nouns,
                // so recover it from them; "with" stays the fallback only when the input genuinely has
                // no preposition to recover ("unlock door key").
                prepositionTag = RecoverPrepositionFromInput(originalInput, nouns[1]) ?? "with";
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

    private static GoToDestinationIntent? DetermineGoToIntent(string? response)
    {
        // Issue #268: the player named a place/room to travel to ("go to the kitchen"). The AI tags
        // this "goto" with the destination in <noun> tags. The engine resolves it against the current
        // room's exits — see DestinationNavigationEngine.
        var intentTag = ExtractElementsByTag(response, "intent").SingleOrDefault();
        if (intentTag != "goto")
            return null;

        var nouns = ExtractElementsByTag(response, "noun");
        if (!nouns.Any())
            return null;

        return new GoToDestinationIntent { Destination = nouns.First(), Message = response };
    }

    private static IntentBase? DetermineMoveIntent(string? response)
    {
        var intentTag = ExtractElementsByTag(response, "intent").SingleOrDefault();
        if (string.IsNullOrEmpty(intentTag))
            return null;

        if (intentTag != "move")
            return null;

        var directionTag = ExtractElementsByTag(response, "direction").SingleOrDefault();
        if (string.IsNullOrEmpty(directionTag))
            directionTag = ExtractElementsByTag(response, "verb").SingleOrDefault();

        var direction = DirectionParser.ParseDirection(directionTag ?? string.Empty);
        if (direction != Direction.Unknown)
            return new MoveIntent { Direction = direction, Message = response };

        // Issue #268 deterministic safety net: the model tagged this "move" but the direction did not
        // resolve to a real direction (typically "other"). If it also named a place, the player wants
        // destination navigation ("move to the dome room") — emit that rather than dropping the command.
        var noun = ExtractElementsByTag(response, "noun").FirstOrDefault();
        return string.IsNullOrEmpty(noun)
            ? null
            : new GoToDestinationIntent { Destination = noun, Message = response };
    }
    
    private static T? DetermineSimpleIntent<T>(string? response) where T : IntentBase, new()
    {
        var tag = typeof(T).GetProperty("TagName")?.GetValue(null) as string;
        var intentTag = ExtractElementsByTag(response, "intent").SingleOrDefault();
        if (string.IsNullOrEmpty(intentTag))
            return null;

        return intentTag != tag ? null : new T();
    }

    // Issue #538. The prepositions a player can realistically use to join two nouns in a command.
    // Matched whole-word, so this is only ever used to read the preposition back out of the player's
    // own sentence when the AI parser failed to tag it — never to decide whether a command is valid.
    private static readonly string[] KnownPrepositions =
    [
        "underneath", "throughout", "through", "between", "beneath", "against", "towards", "outside",
        "inside", "beside", "behind", "across", "around", "toward", "under", "using", "above", "along",
        "about", "after", "onto", "into", "over", "with", "from", "down", "upon", "past", "off", "out",
        "for", "at", "by", "in", "on", "to", "up"
    ];

    private static readonly char[] WordSeparators = [' ', '\t', '\r', '\n', ',', '.', ';', ':', '!', '?', '"', '\''];

    /// <summary>
    ///     Issue #538. Reads the preposition that joins the two nouns out of the player's original
    ///     input, for the turns where the AI parser omitted the &lt;preposition&gt; tag. Returns null
    ///     when the player used no preposition at all ("unlock door key"), leaving the caller's
    ///     historical "with" default in place.
    /// </summary>
    /// <remarks>
    ///     The search runs BACKWARDS from the second noun so it finds the preposition that actually
    ///     binds the pair rather than any earlier one in the sentence ("with the pliers, slide the card
    ///     through the slot" is "through", not "with"). Index 0 is excluded because that position is
    ///     the verb, not a connector.
    /// </remarks>
    private static string? RecoverPrepositionFromInput(string? originalInput, string secondNoun)
    {
        if (string.IsNullOrWhiteSpace(originalInput))
            return null;

        var words = originalInput
            .Split(WordSeparators, StringSplitOptions.RemoveEmptyEntries)
            .Select(word => word.ToLowerInvariant())
            .ToList();

        // Start just before the second noun where we can find it. The model sometimes normalizes or
        // re-words a noun, in which case we fall back to scanning from the end of the sentence.
        var startIndex = words.Count - 1;
        var headOfSecondNoun = secondNoun
            .Split(WordSeparators, StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault()?.ToLowerInvariant();

        if (!string.IsNullOrEmpty(headOfSecondNoun))
        {
            var nounIndex = words.LastIndexOf(headOfSecondNoun);
            if (nounIndex > 0)
                startIndex = nounIndex - 1;
        }

        for (var i = startIndex; i >= 1; i--)
            if (KnownPrepositions.Contains(words[i]))
                return words[i];

        return null;
    }

    /// <summary>
    ///     Issue #538. True when a two-noun command joins its nouns with a TOOL preposition — the
    ///     "take the bedistor with/using the pliers" shape that the system prompt's rule 1e says must
    ///     be an action, not a take. Checks the parser's own tag first, then the player's words, since
    ///     the mis-bucketed turns are exactly the ones where a tag goes missing.
    /// </summary>
    private static bool UsesAToolPreposition(string? response, string? originalInput)
    {
        string[] toolPrepositions = ["with", "using"];

        var prepositionTag = ExtractElementsByTag(response, "preposition").FirstOrDefault();
        if (!string.IsNullOrEmpty(prepositionTag) &&
            toolPrepositions.Contains(prepositionTag, StringComparer.OrdinalIgnoreCase))
            return true;

        if (string.IsNullOrWhiteSpace(originalInput))
            return false;

        return originalInput
            .Split(WordSeparators, StringSplitOptions.RemoveEmptyEntries)
            .Skip(1)
            .Any(word => toolPrepositions.Contains(word, StringComparer.OrdinalIgnoreCase));
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
        // #256: More than one <verb> or <intent> tag means the player ran several commands
        // together on one line without periods (e.g. "look examine bulkhead open bulkhead").
        // A well-formed single command has exactly one of each (two *nouns* is still fine — that
        // is a normal multi-noun command like "tie rope to railing"). We do not execute these;
        // we ask the player to separate them with periods. Detecting it here also keeps us out of
        // the determiners below, whose .SingleOrDefault() calls throw on duplicate tags — the
        // original uncaught exception that surfaced as an HTTP 500.
        var lowered = response?.ToLowerInvariant();
        if (ExtractElementsByTag(lowered, "verb").Count > 1 || ExtractElementsByTag(lowered, "intent").Count > 1)
        {
            logger?.LogDebug("Multiple verbs/intents detected - treating as multiple commands on one line");
            return new MultipleCommandsIntent { Message = response };
        }

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

        var goToIntent = DetermineGoToIntent(response?.ToLowerInvariant());
        if (goToIntent != null)
            return goToIntent;

        var moveIntent = DetermineMoveIntent(response?.ToLowerInvariant());
        if (moveIntent != null)
            return moveIntent;
        
        var inventoryIntent = DetermineSimpleIntent<InventoryIntent>(response?.ToLowerInvariant());
        if (inventoryIntent != null)
            return inventoryIntent;
        
        // Issue #423: a "look" intent that ALSO names a specific object is a *targeted* look ("look
        // through the window", "peer through the crack") that gpt-4o mis-bucketed into the bare look-around
        // intent. Returning a bare LookIntent there drops the noun and re-renders the whole ROOM
        // (LookProcessor), so a room handler's look-verb gate — e.g. Bio Lock East's window / the Radiation
        // Lab crack — never sees the object and the view "through" it silently degrades to the room
        // description. Only an object-less look ("look", "where am I?") OR a look whose noun is the whole
        // scene ("look at the room", "look around the area") stays a LookIntent; when a real object is
        // named, fall through to the action path so it becomes an examine of that object.
        var lookIntent = DetermineSimpleIntent<LookIntent>(response?.ToLowerInvariant());
        if (lookIntent != null && ExtractElementsByTag(lowered, "noun").All(WholeSceneNouns.Contains))
            return lookIntent;

        var actionIntent = DetermineActionIntent(response?.ToLowerInvariant(), input, logger);
        if (actionIntent != null)
            return actionIntent;

        return new NullIntent();
    }
}