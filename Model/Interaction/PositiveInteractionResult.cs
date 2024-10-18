namespace Model.Interaction;

/// <summary>
///     We have a positive match for an intent that moves the story forward, or changes state in some way.
///     The interaction will be handled completely by the fictional narrative - we will not generate
///     any artificial response.
/// </summary>
/// <example>
///     The user types "open mailbox". There is a mailbox here, and the mailbox can be opened, so we handle
///     the interaction locally using the story narrative.
/// </example>
public sealed record PositiveInteractionResult : InteractionResult
{
    public PositiveInteractionResult(string message)
    {
        InteractionMessage = message;
    }

    public override bool InteractionHappened => true;
}