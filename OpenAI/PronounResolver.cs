using System.Text.RegularExpressions;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Logging;
using Model.AIParsing;

namespace OpenAI;

public class PronounResolver(ILogger? logger = null) : OpenAIClientBase(logger, requireApiKey: false), IPronounResolver
{
    protected override string DeploymentName => "gpt-4o-mini"; // Cheaper model for pronoun resolution

    // Pronouns we want to resolve
    private static readonly string[] Pronouns =
        ["it", "them", "that", "this", "those", "these", "him", "her"];

    // Base class handles API key setup with graceful degradation

    public async Task<string?> ResolvePronouns(string playerInput, IEnumerable<string> recentResponses)
    {
        // If no API key (test environment), skip resolution
        if (!HasApiKey)
            return null;

        // Quick check: does input contain pronouns?
        if (!ContainsPronouns(playerInput))
            return null;

        // Build context from recent responses
        var responseContext = string.Join("\n", recentResponses.Reverse().Take(1));
        if (string.IsNullOrWhiteSpace(responseContext))
            return null; // No responses to reference

        // Ask LLM to resolve pronouns
        try
        {
            var rewrittenCommand = await ResolveWithLLM(playerInput, responseContext);
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

    private async Task<string> ResolveWithLLM(string playerInput, string responseContext)
    {
        var systemPrompt = @"You are a pronoun resolver for a text adventure game.

Your job: If the player's command contains a pronoun (it, them, that, this, those, these),
identify what noun from the recent game responses the pronoun refers to, and rewrite the
command replacing the pronoun with that specific noun.

If there are no pronouns or they don't clearly refer to anything in the responses,
return the original command UNCHANGED.

IMPORTANT: Return ONLY the rewritten command with no explanation, quotes, or extra text.

Examples:

Game responses: ""The door is locked.""
Player command: ""open it""
Output: open door

Game responses: ""You see a sword and shield here.""
Player command: ""take them""
Output: take sword and shield

Game responses: ""The pod door is closed.""
Player command: ""open it""
Output: open door

Game responses: ""There is nothing special about the lamp.""
Player command: ""drop it""
Output: drop lamp";

        var userMessage = $@"Recent game responses:
{responseContext}

Player command: {playerInput}

Rewritten command:";

        var options = new ChatCompletionsOptions
        {
            DeploymentName = DeploymentName,
            Temperature = 0f, // Deterministic for pronoun resolution
            Messages =
            {
                new ChatRequestSystemMessage(systemPrompt),
                new ChatRequestUserMessage(userMessage)
            }
        };

        var response = await Client!.GetChatCompletionsAsync(options);
        var result = response.Value.Choices[0].Message.Content;

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
