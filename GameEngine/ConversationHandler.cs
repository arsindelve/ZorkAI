using ChatLambda;
using Microsoft.Extensions.Logging;
using Model.AIGeneration;
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
            // The addressed character (if any) is not in scope. If the player nonetheless directly
            // addressed a KNOWN talkable character by name — the vocative "Name, ..." form — answer
            // "X isn't here." instead of letting the leftover command fall through to normal player
            // parsing. Falling through is the #264 bug: it would silently move the player
            // ("floyd, go up"), drop their items ("floyd, drop diary"), or let the narrator
            // hallucinate the absent NPC acting. This guard is deterministic (no AI), so it runs
            // even when generation is disabled and is fully unit-testable.
            var absentTarget = FindTargetCharacter(input, CollectAllKnownTalkers());
            if (absentTarget != null && IsGenuineDirectAddress(input, absentTarget))
            {
                var notHere = absentTarget.NotHereDescription;
                logger?.LogDebug($"[CONVERSATION DEBUG] Addressed absent talker; responding: '{notHere}'");
                return notHere;
            }

            logger?.LogDebug("[CONVERSATION DEBUG] No matching character found in input, returning null");
            return null;
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
    /// Collects every talkable character the game knows about, regardless of where they currently
    /// are, by resolving the game's declared roster. Lazily instantiates each so an NPC who has not
    /// been touched yet is still "known" — without this, addressing an absent, not-yet-loaded NPC
    /// (e.g. Floyd while still on Deck Nine) would slip through to player parsing.
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
    /// A conservative, deterministic test for whether the player genuinely addressed the character.
    /// Catches the vocative "Name, ..." form and the imperative "tell/ask/talk to ... Name ..."
    /// forms, but only when the character's name appears immediately after the address verb. Real
    /// commands that merely mention the name ("examine floyd", "look at the robot", "ask about the
    /// alien") are NOT hijacked and correctly fall through to normal parsing.
    /// </summary>
    private static bool IsGenuineDirectAddress(string input, ICanBeTalkedTo target)
    {
        // Vocative: "Name, ...".
        if (TryStripDirectAddress(input, target, out _))
            return true;

        if (target is not IItem item)
            return false;

        var trimmed = input.TrimStart();

        // Imperative: "<address verb> [the] Name ...".
        foreach (var lead in AddressLeadIns)
        {
            if (!trimmed.StartsWith(lead, StringComparison.OrdinalIgnoreCase))
                continue;

            var rest = trimmed[lead.Length..].TrimStart();
            if (rest.StartsWith("the ", StringComparison.OrdinalIgnoreCase))
                rest = rest["the ".Length..].TrimStart();

            // Prefer the longest matching noun so "ensign blather" wins over "blather".
            if (item.NounsForMatching
                .OrderByDescending(n => n.Length)
                .Any(noun => StartsWithWholeWord(rest, noun)))
                return true;
        }

        return false;
    }

    /// <summary>
    /// True when <paramref name="text"/> begins with <paramref name="word"/> at a word boundary, so
    /// "robot" matches "robot to go" but not "robotics".
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
    /// Finds the target character for conversation based on noun matching in the input.
    /// </summary>
    private ICanBeTalkedTo? FindTargetCharacter(string input, List<ICanBeTalkedTo> talkers)
    {
        var inputLower = input.ToLowerInvariant();
        logger?.LogDebug($"[CONVERSATION DEBUG] Input lowercased: '{inputLower}'");

        var targetCharacter = talkers
            .OfType<IItem>()
            .FirstOrDefault(item => item.NounsForMatching
                .Any(noun => {
                    var nounLower = noun.ToLowerInvariant();
                    var contains = inputLower.Contains(nounLower);
                    logger?.LogDebug($"[CONVERSATION DEBUG] Checking noun '{nounLower}' in '{inputLower}': {contains}");
                    return contains;
                })) as ICanBeTalkedTo;

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