namespace Model.Interaction;

/// <summary>
/// The noun of the intent does not match the any items available in the location or
/// the current inventory. For example, the noun was "horse" and the result of the interaction
/// is that there is no horse that can be interacted with. We will provide a generated narrative response,
/// but no state changes of any kind will occur. 
/// </summary>
/// <example>
/// The user types "ride the horse". The parser know what you want to do, but there
/// is no horse here, (or anywhere in the story) 
/// </example>
public record NoNounMatchInteractionResult : InteractionResult
{
    public override bool InteractionHappened => false;
}