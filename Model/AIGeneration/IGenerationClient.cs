using Model.AIGeneration.Requests;

namespace Model.AIGeneration;

/// <summary>
///     Interface for AI generation clients which can generate text for different scenarios in the game that don't have
///     pre-determined outcomes, and which typically do not move the story forward or change the game state.
/// </summary>
public interface IGenerationClient
{
    public Action? OnGenerate { get; set; }

    List<(string, string, bool)> LastFiveInputOutputs { get; set; }
    
    /// <summary>
    ///     Completes a chat operation based on the given request.
    /// </summary>
    /// <param name="request">The request object containing the necessary information for the chat operation.</param>
    /// <returns>The generated chat response as a string.</returns>
    Task<string> CompleteChat(Request request);
}