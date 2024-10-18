using Model.Item;

namespace Model.Interaction;

/// <summary>
/// Our simple interaction (one noun, one verb) requires disambiguation.
/// The verb and possible response nouns are contained in this object's properties. 
/// </summary>
public sealed record SimpleInteractionDisambiguationInteractionResult : InteractionResult
{
    public SimpleInteractionDisambiguationInteractionResult(string message, string verb, Dictionary<string, string> possibleResponses)
    {
        Verb = verb;
        PossibleResponses = possibleResponses;
        InteractionMessage = message;
    }

    public string Verb { get; set; }
    
    public Dictionary<string, string> PossibleResponses { get; set; }

    public override bool InteractionHappened => true;
}