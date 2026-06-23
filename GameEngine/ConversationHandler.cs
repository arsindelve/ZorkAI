using ChatLambda;
using Microsoft.Extensions.Logging;
using Model.AIGeneration;
using Model.AIGeneration.Requests;
using Model.Interface;
using Model.Item;

namespace GameEngine;

/// <summary>
/// Handles conversation detection and processing between the player and talkable entities.
/// </summary>
public class ConversationHandler(
    ILogger? logger,
    IParseConversation parseConversation,
    IGenerationClient generationClient,
    IReadOnlyList<Type>? knownTalkerTypes = null)
{
    /// <summary>
    /// Checks if the input represents a conversation with a talkable entity and processes it if so.
    /// </summary>
    /// <param name="input">The player's input</param>
    /// <param name="context">The current game context</param>
    /// <returns>The conversation response if a conversation was detected, null otherwise</returns>
    public async Task<string?> CheckForConversation(string input, IContext context)
    {
        logger?.LogDebug($"[CONVERSATION DEBUG] Checking input: '{input}'");

        var present = CollectTalkableEntities(context);
        var targetCharacter = FindTargetCharacter(input, present);

        if (targetCharacter == null)
        {
            // No talkable NPC is present BY NAME. If the player nonetheless addressed a KNOWN but
            // absent one, tell them the character isn't here instead of letting the leftover command
            // fall through to normal player parsing. Falling through is the #264 bug: it would
            // silently move the player ("floyd, go up"), drop their items ("floyd, drop diary"), or
            // let the narrator hallucinate the absent NPC acting. Detection mirrors the present path:
            // a deterministic fast path catches the common forms (and works offline), and anything
            // else defers to the same conversation classifier, so any phrasing works. See
            // FindAddressedAbsentCharacter.
            var absentTarget = await FindAddressedAbsentCharacter(input);
            if (absentTarget != null)
                return await NarrateAbsence(absentTarget, context);

            // #284: the player named no one, but if the utterance is plainly conversational speech
            // (bare quoted "…", or an untargeted "say …"/greeting) and EXACTLY ONE talkable NPC is
            // present, route it to them — the same outcome "blather, …" already produces. Requiring a
            // single present talker means we never have to guess WHOM a nameless line addresses;
            // requiring unambiguous speech (never a real command) means we never hijack puzzle input
            // like Zork's "say treasure" (Zork has no talkable NPCs, so this branch can't fire there
            // anyway). See TryRouteNamelessSpeech.
            return await TryRouteNamelessSpeech(input, present, context);
        }

        // A talkable character is present. Routing the player's utterance to them relies on the AI
        // rewriter, so respect the generation kill-switch exactly as before (#182 behavior).
        if (generationClient.IsDisabled)
        {
            logger?.LogDebug("[CONVERSATION DEBUG] Conversations disabled via NoGeneratedResponses flag");
            return null;
        }

        return await ProcessConversation(input, targetCharacter, context);
    }

    /// <summary>
    /// Routes plainly-conversational speech that names no one to the sole present talkable NPC
    /// (#284). Two forms were falling through to the narrator instead of reaching the NPC in the
    /// room: bare quoted speech (<c>"you are a fool"</c>) and an untargeted speak-aloud verb
    /// (<c>say hello</c>). Both contain no name, so neither the present-name nor the absent-name
    /// lookup matches them, and the input used to leak to the third-person narrator.
    ///
    /// The guards are deliberately conservative:
    /// <list type="bullet">
    /// <item>EXACTLY ONE talker must be present. With no name to disambiguate, a single present
    /// talker is the only case where we know who is being addressed (Feinstein can hold Blather AND
    /// the Ambassador at once; that ambiguous case falls through, as the issue allows).</item>
    /// <item>The input must read as unambiguous speech, never a real command — so puzzle input such
    /// as Zork's "say treasure" is never hijacked. Zork has no talkable NPCs, so the single-talker
    /// gate already keeps this branch dormant there; the speech test is a second line of defense.</item>
    /// <item>Routing relies on the NPC's AI conversation backend, so it honors the generation
    /// kill-switch exactly like the present-name path, staying deterministic in NoGeneratedResponses
    /// mode.</item>
    /// </list>
    /// Returns the NPC's reply, or null to fall through to normal parsing.
    /// </summary>
    private async Task<string?> TryRouteNamelessSpeech(string input, List<ICanBeTalkedTo> present, IContext context)
    {
        if (present.Count != 1)
        {
            logger?.LogDebug($"[CONVERSATION DEBUG] Nameless speech: {present.Count} present talker(s), not routing");
            return null;
        }

        if (!TryExtractNamelessSpeech(input, out var speech))
        {
            logger?.LogDebug("[CONVERSATION DEBUG] Input is not nameless conversational speech; returning null");
            return null;
        }

        if (generationClient.IsDisabled)
        {
            logger?.LogDebug("[CONVERSATION DEBUG] Conversations disabled via NoGeneratedResponses flag");
            return null;
        }

        var target = present[0];
        logger?.LogDebug($"[CONVERSATION DEBUG] Routing nameless speech '{speech}' to the sole present talker");
        return await target.OnBeingTalkedTo(speech, context, generationClient);
    }

    /// <summary>
    /// The "speak aloud" verbs that introduce untargeted speech ("say hello", "shout hi"). Derived
    /// from the central <see cref="Verbs.SayVerbs" /> list minus "tell", which is a *named*-address
    /// lead-in ("tell floyd …") handled by the name-based paths rather than untargeted speech.
    /// </summary>
    private static readonly string[] NamelessSpeechVerbs =
        Verbs.SayVerbs.Where(verb => verb != "tell").ToArray();

    /// <summary>
    /// Casual one-shot greetings that count as conversation when addressed to no one in particular.
    /// A bare greeting with a single present talker is routed to them ("hi" -> the NPC). Kept tight
    /// to unambiguous greetings so an ordinary command is never mistaken for one.
    /// </summary>
    private static readonly string[] BareGreetings =
        ["hello", "hi", "hey", "yo", "greetings", "howdy", "hiya", "hey there", "hello there", "hi there"];

    /// <summary>
    /// Prepositions that, right after a speak-aloud verb, name a *recipient* ("say TO guard …",
    /// "whisper TO ghost …", "yell AT them", "speak WITH her"). That is directed address to a
    /// specific (here, unknown or absent) party, not untargeted speech, so it must not be put into
    /// the present NPC's mouth. When the recipient is the present NPC, the name-based path already
    /// handled it and we never reach the nameless route.
    /// </summary>
    private static readonly string[] DirectedAddressPrepositions = ["to", "at", "with"];

    /// <summary>
    /// Recognizes the nameless conversational forms and yields the words to hand to the NPC: bare
    /// quoted speech ("…"), an untargeted speak-aloud verb ("say …"), or a bare greeting ("hi").
    /// Anything else (a real command) yields false so the input falls through to normal parsing.
    /// </summary>
    private static bool TryExtractNamelessSpeech(string input, out string speech)
    {
        speech = string.Empty;
        var trimmed = input.Trim();
        if (trimmed.Length == 0)
            return false;

        // 1. Bare quoted speech: "you are a fool" -> you are a fool. The surrounding double quotes
        //    are a deterministic, collision-free signal — no game command is written inside quotes.
        if (TryStripQuotedSpeech(trimmed, out var quoted))
        {
            speech = quoted;
            return true;
        }

        // 2. Untargeted speak-aloud verb: "say hello" -> hello. ("say to floyd …" names a target and
        //    is handled by the name paths, so by the time we get here no name was found.) Quotes
        //    around the remainder are stripped too, so 'say "hello"' reaches the NPC as hello.
        foreach (var verb in NamelessSpeechVerbs)
        {
            if (!StartsWithWholeWord(trimmed, verb))
                continue;

            var rest = trimmed[verb.Length..].Trim();

            // "say to guard …" / "whisper to ghost …" / "yell at X" name a recipient — directed
            // address to someone else, not untargeted speech — so leave it for normal parsing
            // instead of routing it to the present NPC.
            if (DirectedAddressPrepositions.Any(prep => StartsWithWholeWord(rest, prep)))
                return false;

            if (TryStripQuotedSpeech(rest, out var innerQuoted))
                rest = innerQuoted;

            if (rest.Length > 0)
            {
                speech = rest;
                return true;
            }
        }

        // 3. A bare greeting on its own ("hello", "hey there").
        if (BareGreetings.Any(greeting => trimmed.Equals(greeting, StringComparison.OrdinalIgnoreCase)))
        {
            speech = trimmed;
            return true;
        }

        return false;
    }

    /// <summary>
    /// If <paramref name="text" /> begins with a double quote, returns the text inside the quotes (a
    /// trailing quote is optional, so an unterminated <c>"hello</c> reaches the NPC too). Only double
    /// quotes — straight or smart — count; an apostrophe is too common in ordinary words to treat as
    /// the start of speech. Returns false (with an empty result) when there is no opening quote or
    /// nothing inside.
    /// </summary>
    private static bool TryStripQuotedSpeech(string text, out string inner)
    {
        inner = string.Empty;
        if (text.Length == 0 || !IsDoubleQuote(text[0]))
            return false;

        var body = text[1..];
        if (body.Length > 0 && IsDoubleQuote(body[^1]))
            body = body[..^1];

        inner = body.Trim();
        return inner.Length > 0;
    }

    private static bool IsDoubleQuote(char c) => c is '"' or '“' or '”';

    /// <summary>
    /// Produces the response when the player addressed an absent known character. The narrator tells
    /// them the character isn't here in its own voice (the owner wants this generated, not a fixed
    /// line). The static <see cref="ICanBeTalkedTo.NotHereDescription"/> is used only as a fallback
    /// when generation is unavailable (NoGeneratedResponses mode) or returns nothing — so the guard
    /// is still deterministic in those modes and never leaks back into player parsing.
    /// </summary>
    private async Task<string> NarrateAbsence(ICanBeTalkedTo target, IContext context)
    {
        var fallback = target.NotHereDescription;

        if (generationClient.IsDisabled)
        {
            logger?.LogDebug($"[CONVERSATION DEBUG] Generation disabled; absent talker fallback: '{fallback}'");
            return fallback;
        }

        var characterName = (target as IItem)?.Name ?? "that character";
        var request = new TalkingToAbsentCharacterRequest(
            context.CurrentLocation.GetDescriptionForGeneration(context), characterName);

        var narration = await generationClient.GenerateNarration(request, context.SystemPromptAddendum);
        logger?.LogDebug($"[CONVERSATION DEBUG] Narrated absent talker '{characterName}': '{narration}'");

        return string.IsNullOrWhiteSpace(narration) ? fallback : narration;
    }

    /// <summary>
    /// Decides whether the player addressed a KNOWN but absent talkable character, and returns that
    /// character if so. Only considers characters whose name or synonym is actually mentioned (so we
    /// know who is being addressed). A deterministic fast path (<see cref="IsGenuineDirectAddress"/>)
    /// recognizes the common explicit forms and works even when generation is disabled; for anything
    /// else, it defers to the same <c>ParseConversation</c> classifier used for present NPCs, so
    /// arbitrary phrasing ("could you let floyd know to move") is handled too. Real commands that
    /// merely mention the name ("examine floyd") are rejected by both checks and fall through.
    /// </summary>
    private async Task<ICanBeTalkedTo?> FindAddressedAbsentCharacter(string input)
    {
        var referenced = CollectAllKnownTalkers()
            .Where(talker => talker is IItem item && Mentions(input, item))
            .ToList();
        if (referenced.Count == 0)
            return null;

        // Fast, deterministic, offline-safe path. Scanning all referenced talkers means the one
        // actually addressed wins when a sentence names more than one.
        var direct = referenced.FirstOrDefault(talker => IsGenuineDirectAddress(input, talker));
        if (direct != null)
            return direct;

        // Otherwise let the conversation classifier decide. Skipped when generation is disabled so
        // the guard stays deterministic in NoGeneratedResponses mode.
        if (generationClient.IsDisabled)
            return null;

        var parseResult = await parseConversation.ParseAsync(input);
        logger?.LogDebug($"[CONVERSATION DEBUG] Absent-talker classifier isConversational: {parseResult.isConversational}");
        if (!parseResult.isConversational)
            return null;

        // The classifier confirms the input is address but not WHOM it addresses. When more than one
        // absent talker is named, attribute to the one whose name appears earliest in the input (the
        // addressed party in "tell floyd that blather left" phrasings) rather than arbitrary roster
        // order.
        return referenced
            .OrderBy(talker => FirstMentionIndex(input, (IItem)talker))
            .First();
    }

    /// <summary>
    /// True when any of the item's nouns appears in the input as a whole word (so "robot" is a hit
    /// in "the robot, go" but not in "robotics").
    /// </summary>
    private static bool Mentions(string input, IItem item) =>
        item.NounsForMatching.Any(noun => ContainsWholeWord(input, noun));

    /// <summary>
    /// The index of the earliest whole-word mention of any of the item's nouns in the input, or
    /// <see cref="int.MaxValue"/> if none — used to attribute an ambiguous classifier-detected
    /// address to the talker named first.
    /// </summary>
    private static int FirstMentionIndex(string input, IItem item)
    {
        var earliest = int.MaxValue;
        foreach (var noun in item.NounsForMatching)
        {
            var index = WholeWordIndex(input, noun);
            if (index >= 0 && index < earliest)
                earliest = index;
        }

        return earliest;
    }

    private static bool ContainsWholeWord(string text, string word) => WholeWordIndex(text, word) >= 0;

    /// <summary>
    /// The index of the first whole-word occurrence of <paramref name="word"/> in
    /// <paramref name="text"/> (case-insensitive), or -1 if not present.
    /// </summary>
    private static int WholeWordIndex(string text, string word)
    {
        var index = 0;
        while ((index = text.IndexOf(word, index, StringComparison.OrdinalIgnoreCase)) >= 0)
        {
            var startOk = index == 0 || !char.IsLetterOrDigit(text[index - 1]);
            var end = index + word.Length;
            var endOk = end == text.Length || !char.IsLetterOrDigit(text[end]);
            if (startOk && endOk)
                return index;
            index = end;
        }

        return -1;
    }

    /// <summary>
    /// Collects every talkable character the game knows about, regardless of where they currently
    /// are, by resolving the game's declared roster. Lazily instantiates each so an NPC who has not
    /// been touched yet is still "known" — without this, addressing an absent, not-yet-loaded NPC
    /// (e.g. Floyd while still on Deck Nine) would slip through to player parsing.
    ///
    /// Note: this runs whenever no talker is present in scope (most turns), so every roster NPC is
    /// force-instantiated into the <see cref="Repository"/> early — and its <c>Init()</c> runs then.
    /// That is fine for the current roster (Init only seeds an item inside the not-yet-placed NPC),
    /// but any future talkable NPC whose Init() has an observable side effect on the context/score
    /// must not assume "not yet in the Repository" means "not yet introduced by the story".
    /// </summary>
    private List<ICanBeTalkedTo> CollectAllKnownTalkers()
    {
        var talkers = new List<ICanBeTalkedTo>();
        foreach (var type in knownTalkerTypes ?? [])
        {
            if (Repository.GetItem(type) is ICanBeTalkedTo talker)
                talkers.Add(talker);
        }

        return talkers;
    }

    /// <summary>
    /// The imperative verbs that introduce a character when the player addresses them by name
    /// ("tell Floyd to ...", "ask the ambassador ...", "talk to Blather"). Kept here so that
    /// recognizing an absent NPC is deterministic and does not require the AI rewriter.
    /// </summary>
    private static readonly string[] AddressLeadIns =
    [
        "tell ", "ask ", "order ", "command ", "instruct ", "remind ",
        "say to ", "talk to ", "speak to ", "speak with ", "yell at ", "yell to ",
        "shout at ", "shout to ", "greet ", "call to "
    ];

    /// <summary>
    /// Casual openers that can precede a name in direct address ("hey floyd", "yo robot"). Longest
    /// first so "hey there" is stripped before "hey".
    /// </summary>
    private static readonly string[] InterjectionOpeners =
    [
        "hey there ", "hey ", "yo ", "hi ", "hello "
    ];

    /// <summary>
    /// A conservative, deterministic test for whether the player directly addressed this character.
    /// Recognizes the name at the start of the command — bare ("floyd go up"), vocative ("floyd, go
    /// up"), alone ("floyd"), or behind a casual/article opener ("hey robot", "the ambassador, ...")
    /// — and the imperative forms ("tell/ask/talk to ... floyd"). Almost nothing legitimately starts
    /// a command with a character's name, so the leading-name form counts as address even without a
    /// comma (this closes the bare "floyd go up" leak). Any of the character's nouns count, including
    /// synonyms like "robot"/"alien" (Floyd IS the robot). Names must match at a word boundary, so
    /// commands that merely mention the name ("examine floyd", "ask about the ambassador") are NOT
    /// hijacked here; the classifier in <see cref="FindAddressedAbsentCharacter"/> is the backstop
    /// for less explicit phrasings.
    /// </summary>
    private static bool IsGenuineDirectAddress(string input, ICanBeTalkedTo target)
    {
        if (target is not IItem item)
            return false;

        var trimmed = input.TrimStart();

        // Longest noun first so "ensign blather" wins over "blather".
        var names = item.NounsForMatching.OrderByDescending(n => n.Length).ToArray();

        // Leading address: optional opener ("hey"/"yo"/...) and/or article ("the"), then the name.
        var leading = StripLeadingAddressPrefix(trimmed);
        if (names.Any(name => StartsWithWholeWord(leading, name)))
            return true;

        // Imperative: "<address verb> [the] Name ...".
        foreach (var lead in AddressLeadIns)
        {
            if (!trimmed.StartsWith(lead, StringComparison.OrdinalIgnoreCase))
                continue;

            var rest = StripLeadingArticle(trimmed[lead.Length..].TrimStart());

            if (names.Any(name => StartsWithWholeWord(rest, name)))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Strips one leading casual opener ("hey "/"yo "/...) and then one leading article ("the "), so
    /// "hey the robot" / "the ambassador, ..." are recognized as addressing the character.
    /// </summary>
    private static string StripLeadingAddressPrefix(string text)
    {
        foreach (var opener in InterjectionOpeners)
        {
            if (text.StartsWith(opener, StringComparison.OrdinalIgnoreCase))
            {
                text = text[opener.Length..].TrimStart();
                break;
            }
        }

        return StripLeadingArticle(text);
    }

    /// <summary>
    /// Removes a single leading "the " so "the ambassador, go up" is recognized as addressing the
    /// ambassador, mirroring how "tell the ambassador ..." is handled.
    /// </summary>
    private static string StripLeadingArticle(string text) =>
        text.StartsWith("the ", StringComparison.OrdinalIgnoreCase)
            ? text["the ".Length..].TrimStart()
            : text;

    /// <summary>
    /// True when <paramref name="text"/> begins with <paramref name="word"/> at a word boundary, so
    /// "floyd" matches "floyd go up" and "floyd, go" but not "floydian".
    /// </summary>
    private static bool StartsWithWholeWord(string text, string word)
    {
        if (!text.StartsWith(word, StringComparison.OrdinalIgnoreCase))
            return false;

        return text.Length == word.Length || !char.IsLetterOrDigit(text[word.Length]);
    }

    /// <summary>
    /// Collects all talkable entities from the player's inventory and current location.
    /// </summary>
    private List<ICanBeTalkedTo> CollectTalkableEntities(IContext context)
    {
        var talkers = new List<ICanBeTalkedTo>();
        talkers.AddRange(context.Items.OfType<ICanBeTalkedTo>());

        if (context.CurrentLocation is ICanContainItems container)
        {
            talkers.AddRange(container.Items.OfType<ICanBeTalkedTo>());
        }

        LogTalkableEntities(talkers);
        return talkers;
    }

    /// <summary>
    /// Logs information about all discovered talkable entities for debugging.
    /// </summary>
    private void LogTalkableEntities(List<ICanBeTalkedTo> talkers)
    {
        logger?.LogDebug($"[CONVERSATION DEBUG] Found {talkers.Count} talkable entities in total");
        foreach (var talkable in talkers)
        {
            if (talkable is IItem item)
            {
                logger?.LogDebug($"[CONVERSATION DEBUG] - {item.Name} (nouns: {string.Join(", ", item.NounsForMatching)})");
            }
            else
            {
                logger?.LogDebug($"[CONVERSATION DEBUG] - {talkable.GetType().Name} (not an IItem)");
            }
        }
    }

    /// <summary>
    /// Finds the target character for conversation based on noun matching in the input. Matches on a
    /// whole-word boundary (consistent with the absent-NPC path) so a present NPC is not matched by a
    /// partial-word hit (e.g. "robot" inside "robotics").
    /// </summary>
    private ICanBeTalkedTo? FindTargetCharacter(string input, List<ICanBeTalkedTo> talkers)
    {
        var targetCharacter = talkers
            .OfType<IItem>()
            .FirstOrDefault(item => item.NounsForMatching.Any(noun => ContainsWholeWord(input, noun)))
            as ICanBeTalkedTo;

        if (targetCharacter != null)
        {
            logger?.LogDebug($"[CONVERSATION DEBUG] Found target character: {(targetCharacter as IItem)?.Name ?? targetCharacter.GetType().Name}");
        }

        return targetCharacter;
    }

    /// <summary>
    /// Processes the conversation by parsing the input and sending it to the target character.
    /// </summary>
    private async Task<string?> ProcessConversation(string input, ICanBeTalkedTo targetCharacter, IContext context)
    {
        // Use ParseConversation to determine if this is actually communication
        logger?.LogDebug($"[CONVERSATION DEBUG] Calling ParseConversation.ParseAsync with input: '{input}'");
        var parseResult = await parseConversation.ParseAsync(input);
        logger?.LogDebug($"[CONVERSATION DEBUG] ParseConversation result - isConversational: {parseResult.isConversational}, response: '{parseResult.response}'");

        string textForCharacter;
        if (parseResult.isConversational)
        {
            // The rewriter recognized a command/speech directed at the character; use its
            // second-person rewrite (e.g. "floyd, go north" -> "go north").
            textForCharacter = parseResult.response;
        }
        else if (TryStripDirectAddress(input, targetCharacter, out var remainder))
        {
            // The player explicitly addressed the character by name (e.g. "blather, ...").
            // That is an unambiguous signal of conversation even when the rewriter doesn't
            // recognize a command, so route what they said (minus the leading name) to the
            // character rather than dropping it back into normal command parsing.
            logger?.LogDebug($"[CONVERSATION DEBUG] Direct address detected; using remainder: '{remainder}'");
            textForCharacter = remainder;
        }
        else
        {
            logger?.LogDebug("[CONVERSATION DEBUG] Not conversational, continuing with normal processing");
            return null;
        }

        logger?.LogDebug($"[CONVERSATION DEBUG] Sending message '{textForCharacter}' to character");
        var result = await targetCharacter.OnBeingTalkedTo(textForCharacter, context, generationClient);
        logger?.LogDebug($"[CONVERSATION DEBUG] Character response: '{result}'");
        return result;
    }

    /// <summary>
    /// Detects the vocative "Name, ..." direct-address pattern (e.g. "blather, you clean the
    /// floor") and returns the text that follows the name. This is a strong, deterministic
    /// signal that the player is speaking to the character, independent of the AI rewriter,
    /// which only reliably recognizes imperative commands.
    /// </summary>
    private static bool TryStripDirectAddress(string input, ICanBeTalkedTo targetCharacter, out string remainder)
    {
        remainder = input;

        if (targetCharacter is not IItem item)
            return false;

        var trimmed = input.TrimStart();

        // Prefer the longest matching noun so "ensign blather" wins over "blather".
        foreach (var noun in item.NounsForMatching.OrderByDescending(n => n.Length))
        {
            if (!trimmed.StartsWith(noun, StringComparison.OrdinalIgnoreCase))
                continue;

            var rest = trimmed[noun.Length..];

            // Require the name to be followed by a comma (or to be the whole input) so we
            // only catch genuine direct address and never hijack a real command.
            if (!rest.StartsWith(',') && rest.Trim().Length != 0)
                continue;

            remainder = rest.TrimStart(',', ' ', '.', '!', '?').Trim();

            // "blather" / "blather," on its own: pass the whole utterance through.
            if (string.IsNullOrWhiteSpace(remainder))
                remainder = input.Trim();

            return true;
        }

        return false;
    }
}