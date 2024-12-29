namespace Model.Interaction;

public record DisambiguationInteractionResult : InteractionResult
{
    public DisambiguationInteractionResult(string message,
        Dictionary<string, string> possibleResponses, string replacementString)
    {
        InteractionMessage = message;
        PossibleResponses = possibleResponses;
        ReplacementString = replacementString;
    }

    public Dictionary<string, string> PossibleResponses { get; init; }

    public string ReplacementString { get; init; }

    public override bool InteractionHappened => true;
}