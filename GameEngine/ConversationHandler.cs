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
    IGenerationClient generationClient)
{
    /// <summary>
    /// Checks if the input represents a conversation with a talkable entity and processes it if so.
    /// </summary>
    /// <param name="input">The player's input</param>
    /// <param name="context">The current game context</param>
    /// <returns>The conversation response if a conversation was detected, null otherwise</returns>
    public async Task<string?> CheckForConversation(string input, IContext context)
    {
        if (generationClient.IsDisabled)
        {
            logger?.LogDebug("[CONVERSATION DEBUG] Conversations disabled via NoGeneratedResponses flag");
            return null;
        }

        logger?.LogDebug($"[CONVERSATION DEBUG] Checking input: '{input}'");
        
        var talkers = CollectTalkableEntities(context);
        if (talkers.Count == 0)
        {
            logger?.LogDebug("[CONVERSATION DEBUG] No talkable entities found, returning null");
            return null;
        }

        var targetCharacter = FindTargetCharacter(input, talkers);
        if (targetCharacter == null)
        {
            logger?.LogDebug("[CONVERSATION DEBUG] No matching character found in input, returning null");
            return null;
        }

        return await ProcessConversation(input, targetCharacter, context);
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