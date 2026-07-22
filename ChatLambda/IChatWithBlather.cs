namespace ChatLambda;

/// <summary>
/// Interface for communicating with Blather's conversational AI (cloud Lambda or a local model).
/// </summary>
public interface IChatWithBlather
{
    /// <summary>
    /// Sends the player's words to Blather and returns his response.
    /// </summary>
    /// <param name="prompt">What the player said to Blather</param>
    /// <returns>Blather's response including message and metadata</returns>
    Task<CompanionResponse> AskBlatherAsync(string prompt);
}
