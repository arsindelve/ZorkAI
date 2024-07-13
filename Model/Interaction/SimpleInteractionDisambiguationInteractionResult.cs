namespace Model.Interaction;

/// <summary>
/// 
/// </summary>
public sealed record SimpleInteractionDisambiguationInteractionResult : InteractionResult
{
    public string Verb { get; }
    public string[] PossibleResponses { get; }

    public SimpleInteractionDisambiguationInteractionResult(string message, string verb, string[] possibleResponses)
    {
        Verb = verb;
        PossibleResponses = possibleResponses;
        InteractionMessage = message;
    }

    public override bool InteractionHappened => true;
}