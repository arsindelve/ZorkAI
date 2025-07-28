using Microsoft.Extensions.Logging;

namespace ChatLambda;

/// <summary>
/// Interface for parsing conversation input to determine if it's communication and rewrite it
/// </summary>
public interface IParseConversation
{
    /// <summary>
    /// Parses conversation input and returns whether the response is "No" and the actual response.
    /// </summary>
    /// <param name="input">The conversation input to parse</param>
    /// <returns>A tuple where Item1 is true if response is "No", Item2 is the response (empty if "No")</returns>
    Task<(bool isNo, string response)> ParseAsync(string input);

    /// <summary>
    /// Gets or sets the logger used for logging messages or events.
    /// This property allows integration with a logging framework to
    /// capture and record relevant information during the application runtime.
    /// </summary>
    ILogger Logger { get; set; }
}
