using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Model.AIParsing;
using OpenAI.Chat;

namespace ZorkAI.OpenAI;

/// <summary>
/// Resolves pronouns found in a player's input by analyzing the context provided from prior inputs and responses.
/// The resolution process utilizes a language model for improved accuracy, where available, and can gracefully degrade
/// when no API key is provided for interaction with the model.
/// </summary>
public class PronounResolver(ILogger? logger = null) : OpenAIClientBase(logger, requireApiKey: false), IPronounResolver
{
    protected override string ModelName => "gpt-4o-mini"; // Cheaper model for pronoun resolution

    // Pronouns we want to resolve
    private static readonly string[] Pronouns =
        ["it", "them", "that", "this", "those", "these", "him", "her"];

    // Base class handles API key setup with graceful degradation

    public async Task<string?> ResolvePronouns(string playerInput, string? lastInput, string? lastResponse)
    {
        // If no API key (test environment), skip resolution
        if (!HasApiKey)
            return null;

        // Quick check: does input contain pronouns?
        if (!ContainsPronouns(playerInput))
            return null;

        // Need at least one context source to resolve against
        if (string.IsNullOrWhiteSpace(lastInput) && string.IsNullOrWhiteSpace(lastResponse))
            return null;

        // Ask LLM to resolve pronouns
        try
        {
            var rewrittenCommand = await ResolveWithLLM(playerInput, lastInput, lastResponse);
            Logger?.LogDebug($"Pronoun resolved: '{playerInput}' -> '{rewrittenCommand}' (lastInput: '{lastInput}', lastResponse: '{lastResponse}')");
            return rewrittenCommand;
        }
        catch (Exception ex)
        {
            Logger?.LogWarning($"Pronoun resolution failed: {ex.Message}. Using original input.");
            return null; // Fallback to original input
        }
    }

    private static bool ContainsPronouns(string input)
    {
        var lower = input.ToLowerInvariant();
        return Pronouns.Any(pronoun => Regex.IsMatch(lower, $@"\b{pronoun}\b"));
    }

    private async Task<string> ResolveWithLLM(string playerInput, string? lastInput, string? lastResponse)
    {
        var systemPrompt = @"You are a pronoun resolver for a text adventure game.

Your job: If the player's command contains a pronoun (it, them, that, this, those, these, him, her),
identify what noun the pronoun refers to from either the previous player command OR the game's response,
and rewrite the command replacing the pronoun with that specific noun.

Pronouns often refer to nouns in the PLAYER'S PREVIOUS COMMAND rather than the game's response.

If there are no pronouns or they don't clearly refer to anything,
return the original command UNCHANGED.

IMPORTANT: Return ONLY the rewritten command with no explanation, quotes, or extra text.

Examples:

Last player command: ""take lamp""
Game response: ""Taken.""
Current command: ""turn it on""
Output: turn lamp on

Last player command: ""examine door""
Game response: ""The door is locked.""
Current command: ""open it""
Output: open door

Last player command: ""look""
Game response: ""You see a sword and shield here.""
Current command: ""take them""
Output: take sword and shield

Last player command: ""drop lamp""
Game response: ""Dropped.""
Current command: ""take it""
Output: take lamp";

        var context = string.Empty;
        if (!string.IsNullOrWhiteSpace(lastInput))
            context += $"Last player command: \"{lastInput}\"\n";
        if (!string.IsNullOrWhiteSpace(lastResponse))
            context += $"Game response: \"{lastResponse}\"\n";

        var userMessage = $@"{context}Current command: {playerInput}

Rewritten command:";

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userMessage)
        };

        var options = new ChatCompletionOptions
        {
            Temperature = 0f // Deterministic for pronoun resolution
        };

        ChatCompletion completion = await Client!.CompleteChatAsync(messages, options);
        var result = completion.Content[0].Text;

        // Clean up response (remove quotes, trim)
        result = result.Trim().Trim('"').Trim('\'');

        // If LLM couldn't resolve or returned same input, return null for fallback
        if (string.IsNullOrWhiteSpace(result) ||
            result.Equals(playerInput, StringComparison.OrdinalIgnoreCase))
        {
            return playerInput; // No change
        }

        return result;
    }
}
