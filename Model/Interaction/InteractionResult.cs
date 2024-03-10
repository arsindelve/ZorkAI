namespace Model.Interaction;

/// <summary>
///     This is the base class for intents that have interacted with items and locations. Was it successful? Was it a
///     match?
///     What happened as a result?
/// </summary>
public record InteractionResult
{
    /// <summary>
    ///     Gets or sets a value indicating whether an interaction in the game has occurred.
    ///     This is typically set to true after a player's action results in a specific interaction.
    /// </summary>
    public virtual bool InteractionHappened => false;

    /// <summary>
    ///     Gets or sets the message associated with the current interaction in the game context.
    ///     This message contains detailed information about the interaction, such as the result of a player's action.
    /// </summary>
    /// <example>
    ///     Ok. The mailbox is now closed.
    /// </example>
    public string InteractionMessage { get; protected init; } = "";
}