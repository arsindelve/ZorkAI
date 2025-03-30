namespace Model.Interaction;

/// <summary>
/// Represents a result of an interaction that requires user disambiguation. (i.e Which button?)
/// This result provides a message about the ambiguity, possible clarifying responses,
/// and a string that can be used for replacement based on selected options.
/// </summary>
public record DisambiguationInteractionResult : InteractionResult
{
    public DisambiguationInteractionResult(string message,
        Dictionary<string, string> possibleResponses, string replacementString)
    {
        InteractionMessage = message;
        PossibleResponses = possibleResponses;
        ReplacementString = replacementString;
    }

    /// <summary>
    /// Represents a collection of potential responses for resolving disambiguation in interactions.
    /// The keys of the dictionary represent the identifiers for possible user responses, while the values
    /// represent the corresponding clarifications or descriptions for each response.
    /// </summary>
    public Dictionary<string, string> PossibleResponses { get; init; }

    public string ReplacementString { get; init; }

    public override bool InteractionHappened => true;
}