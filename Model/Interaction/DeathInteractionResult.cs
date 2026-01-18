namespace Model.Interaction;

/// <summary>
///     Represents the result of a player death in the game. This signals to the GameEngine
///     that a complete game restart should occur, preserving only the death counter.
/// </summary>
public sealed record DeathInteractionResult : InteractionResult
{
    public DeathInteractionResult(string message, int deathCount)
    {
        InteractionMessage = message;
        DeathCount = deathCount;
    }

    public override bool InteractionHappened => true;

    /// <summary>
    ///     The number of times the player has died (including this death).
    /// </summary>
    public int DeathCount { get; }
}
