namespace ChatLambda;

/// <summary>
/// Interface for communicating with Floyd's AI Lambda function.
/// </summary>
public interface IChatWithFloyd
{
    /// <summary>
    /// Sends a question to the Floyd Lambda function and returns the response.
    /// </summary>
    /// <param name="prompt">The question to ask Floyd</param>
    /// <returns>Floyd's response including message and metadata</returns>
    Task<CompanionResponse> AskFloydAsync(string prompt);
}
