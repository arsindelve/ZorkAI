namespace Model.Interaction;

/// <summary>
///     The noun of the intent matches some object that is available in the current
///     location or context, but that verb does not move the story forward in any way. We
///     will provide a generated narrative response, but no state changes of any kind will occur.
/// </summary>
/// <example>
///     The user types "kick mailbox". There is a mailbox present, but kicking it has no effect.
/// </example>
public record NoVerbMatchInteractionResult : InteractionResult
{
    public override bool InteractionHappened => false;

    public required string Verb { get; init; }

    public required string? Noun { get; init; }
}