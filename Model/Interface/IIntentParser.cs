using CloudWatch;
using CloudWatch.Model;
using Model.Intent;

namespace Model.Interface;

/// <summary>
///     Interface for a parser that can take user's input and determine their intent. Do they want to move? Quit
///     Pick something up? The parser will hopefully tell us.
/// </summary>
public interface IIntentParser
{
    Guid? TurnCorrelationId { get; set; }

    ICloudWatchLogger<GenerationLog>? Logger { get; set; }

    /// <summary>
    ///     Use a simple, word-match based parser to determines the type of intent.
    /// Global commands work anywhere and everywhere, like look, inventory, wait, and do
    /// count as a turn.
    /// </summary>
    IntentBase? DetermineGlobalIntentType(string? input);

    /// <summary>
    ///     Use a simple, word-match based parser to determines the type of intent
    ///     System commands are meta, like quit, save, restore, and do not
    ///  count as a turn.
    /// </summary>
    IntentBase? DetermineSystemIntentType(string? input);

    /// <summary>
    ///     Use a complex AI parser to determines the type of intent based on the input and session ID.
    /// </summary>
    /// <param name="input">The user input.</param>
    /// <param name="sessionId">The session ID uniquely identifies this session and adventurer to the parser. </param>
    /// ///
    /// <param name="locationDescription">
    ///     This can be instrumental in determining what the user wants to do. If they
    ///     say "follow the path", we need the location description to tell us which way the path goes.
    /// </param>
    /// <returns>The intent type as an instance of IntentBase.</returns>
    Task<IntentBase> DetermineComplexIntentType(string? input, string locationDescription, string sessionId);

    /// <summary>
    ///     Resolves pronouns in player input using the last player input and game response.
    /// </summary>
    /// <param name="input">The current player's command (e.g., "turn it on")</param>
    /// <param name="lastInput">The previous player input (e.g., "take lamp")</param>
    /// <param name="lastResponse">The previous game response (e.g., "Taken")</param>
    /// <returns>Rewritten command with pronouns replaced, or null if no pronouns found</returns>
    Task<string?> ResolvePronounsAsync(string input, string? lastInput, string? lastResponse);
}