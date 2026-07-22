namespace ChatLambda;

/// <summary>
/// Interface for communicating with the Ambassador's conversational AI (cloud Lambda or a local model).
/// </summary>
public interface IChatWithAmbassador
{
    /// <summary>
    /// Sends the player's words to the Ambassador and returns his response.
    /// </summary>
    /// <param name="prompt">What the player said to the Ambassador</param>
    /// <returns>The Ambassador's response including message and metadata</returns>
    Task<CompanionResponse> AskAmbassadorAsync(string prompt);
}
