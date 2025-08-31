using ChatLambda;
using Microsoft.Extensions.Logging;
using Model.AIGeneration;
using Model.Interface;
using Model.Item;

namespace GameEngine;

/// <summary>
/// Handles conversation detection and processing between the player and talkable entities.
/// </summary>
public class ConversationHandler
{
    private readonly ILogger? _logger;
    private readonly IParseConversation _parseConversation;
    private readonly IGenerationClient _generationClient;

    public ConversationHandler(ILogger? logger, IParseConversation parseConversation, IGenerationClient generationClient)
    {
        _logger = logger;
        _parseConversation = parseConversation;
        _generationClient = generationClient;
    }

    /// <summary>
    /// Checks if the input represents a conversation with a talkable entity and processes it if so.
    /// </summary>
    /// <param name="input">The player's input</param>
    /// <param name="context">The current game context</param>
    /// <returns>The conversation response if a conversation was detected, null otherwise</returns>
    public async Task<string?> CheckForConversation(string input, IContext context)
    {
        _logger?.LogDebug($"[CONVERSATION DEBUG] Checking input: '{input}'");
        
        var talkers = CollectTalkableEntities(context);
        if (talkers.Count == 0)
        {
            _logger?.LogDebug("[CONVERSATION DEBUG] No talkable entities found, returning null");
            return null;
        }

        var targetCharacter = FindTargetCharacter(input, talkers);
        if (targetCharacter == null)
        {
            _logger?.LogDebug("[CONVERSATION DEBUG] No matching character found in input, returning null");
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
        _logger?.LogDebug($"[CONVERSATION DEBUG] Found {talkers.Count} talkable entities in total");
        foreach (var talkable in talkers)
        {
            if (talkable is IItem item)
            {
                _logger?.LogDebug($"[CONVERSATION DEBUG] - {item.Name} (nouns: {string.Join(", ", item.NounsForMatching)})");
            }
            else
            {
                _logger?.LogDebug($"[CONVERSATION DEBUG] - {talkable.GetType().Name} (not an IItem)");
            }
        }
    }

    /// <summary>
    /// Finds the target character for conversation based on noun matching in the input.
    /// </summary>
    private ICanBeTalkedTo? FindTargetCharacter(string input, List<ICanBeTalkedTo> talkers)
    {
        var inputLower = input.ToLowerInvariant();
        _logger?.LogDebug($"[CONVERSATION DEBUG] Input lowercased: '{inputLower}'");

        var targetCharacter = talkers
            .OfType<IItem>()
            .FirstOrDefault(item => item.NounsForMatching
                .Any(noun => {
                    var nounLower = noun.ToLowerInvariant();
                    var contains = inputLower.Contains(nounLower);
                    _logger?.LogDebug($"[CONVERSATION DEBUG] Checking noun '{nounLower}' in '{inputLower}': {contains}");
                    return contains;
                })) as ICanBeTalkedTo;

        if (targetCharacter != null)
        {
            _logger?.LogDebug($"[CONVERSATION DEBUG] Found target character: {(targetCharacter as IItem)?.Name ?? targetCharacter.GetType().Name}");
        }

        return targetCharacter;
    }

    /// <summary>
    /// Processes the conversation by parsing the input and sending it to the target character.
    /// </summary>
    private async Task<string?> ProcessConversation(string input, ICanBeTalkedTo targetCharacter, IContext context)
    {
        // Use ParseConversation to determine if this is actually communication
        _logger?.LogDebug($"[CONVERSATION DEBUG] Calling ParseConversation.ParseAsync with input: '{input}'");
        var parseResult = await _parseConversation.ParseAsync(input);
        _logger?.LogDebug($"[CONVERSATION DEBUG] ParseConversation result - isNo: {parseResult.isNo}, response: '{parseResult.response}'");
        
        // If ParseConversation says "No", continue with normal processing
        if (parseResult.isNo)
        {
            _logger?.LogDebug("[CONVERSATION DEBUG] ParseConversation returned 'No', continuing with normal processing");
            return null;
        }

        // Send the rewritten message to the character
        _logger?.LogDebug($"[CONVERSATION DEBUG] Sending rewritten message '{parseResult.response}' to character");
        var result = await targetCharacter.OnBeingTalkedTo(parseResult.response, context, _generationClient);
        _logger?.LogDebug($"[CONVERSATION DEBUG] Character response: '{result}'");
        return result;
    }
}