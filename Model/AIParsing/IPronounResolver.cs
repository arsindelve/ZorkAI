namespace Model.AIParsing;

public interface IPronounResolver
{
    /// <summary>
    /// Resolves pronouns in player input using recent game responses
    /// </summary>
    /// <param name="playerInput">The player's command (e.g., "open it")</param>
    /// <param name="recentResponses">Last 3 game responses</param>
    /// <returns>Rewritten command with pronouns replaced, or null if no pronouns found</returns>
    Task<string?> ResolvePronouns(string playerInput, IEnumerable<string> recentResponses);
}
