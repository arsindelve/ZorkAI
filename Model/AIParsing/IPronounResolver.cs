namespace Model.AIParsing;

public interface IPronounResolver
{
    /// <summary> 
    /// Resolves pronouns in player input using the last player input and game response
    /// </summary>
    /// <param name="playerInput">The current player's command (e.g., "turn it on")</param>
    /// <param name="lastInput">The previous player input (e.g., "take lamp")</param>
    /// <param name="lastResponse">The previous game response (e.g., "Taken")</param>
    /// <returns>Rewritten command with pronouns replaced, or null if no pronouns found</returns>
    Task<string?> ResolvePronouns(string playerInput, string? lastInput, string? lastResponse);
}
