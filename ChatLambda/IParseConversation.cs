using Microsoft.Extensions.Logging;

namespace ChatLambda;

/// <summary>
/// Interface for parsing conversation input to determine if it's communication and rewrite it
/// </summary>
public interface IParseConversation
{
    /// <summary>
    /// Determines if the input is conversational speech directed at a companion (like "Floyd, go north").
    /// If it IS conversation, returns the rewritten command in second person (e.g., "go north").
    /// If it's NOT conversation (just a regular command), indicates no rewriting is needed.
    /// </summary>
    /// <param name="input">The user's input to analyze (e.g., "floyd, go north" or just "go north")</param>
    /// <returns>
    /// A tuple where:
    /// - isConversational = true: Input IS conversational, use the rewritten response (e.g., "floyd, go north" → "go north")
    /// - isConversational = false: Input is NOT conversational, no rewriting needed (response will be empty)
    /// </returns>
    /// <example>
    /// ParseAsync("floyd, go north") → (true, "go north") // IS conversation, rewritten
    /// ParseAsync("go north") → (false, "") // NOT conversation, use as-is
    /// </example>
    Task<(bool isConversational, string response)> ParseAsync(string input);

    /// <summary>
    /// Gets or sets the logger used for logging messages or events.
    /// This property allows integration with a logging framework to
    /// capture and record relevant information during the application runtime.
    /// </summary>
    ILogger Logger { get; set; }
}
