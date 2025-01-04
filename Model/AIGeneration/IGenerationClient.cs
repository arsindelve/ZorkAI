using CloudWatch;
using CloudWatch.Model;
using Model.AIGeneration.Requests;

namespace Model.AIGeneration;

/// <summary>
///     Interface for AI generation clients which can generate text for different scenarios in the game that don't have
///     pre-determined outcomes, and which typically do not move the story forward or change the game state.
/// </summary>
public interface IGenerationClient
{
    public Action? OnGenerate { get; set; }

    /// <summary>
    /// Sets the system prompt for AI generation.
    /// </summary>
    string SystemPrompt { set; }

    List<(string, string, bool)> LastFiveInputOutputs { get; set; }

    Guid TurnCorrelationId { get; set; }

    ICloudWatchLogger<GenerationLog>? CloudWatchLogger { get; set; }

    /// <summary>
    /// Provides generated text for the narrator because whatever the adventurer asked to
    /// do is not provided for by the game, and does not change the state of the game.
    /// </summary>
    /// <param name="request">The request object containing the necessary information for the chat operation.</param>
    /// <returns>The generated chat response as a string.</returns>
    Task<string> GenerateNarration(Request request);

    /// <summary>
    /// Provides generated text for an NPC character such as Floyd or Blather. 
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<string> GenerateCompanionSpeech(CompanionRequest request);
}