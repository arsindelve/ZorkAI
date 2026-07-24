namespace Planetfall.Location;

/// <summary>
/// A device that a player activates by sliding an access card through a slot, and whose activation
/// lapses after a fixed number of turns unless the device is used first. Implemented by the
/// teleportation booths (<see cref="BoothBase" />) and the
/// <see cref="Planetfall.Location.Lawanda.MiniaturizationBooth" />. The shared countdown lives in
/// <see cref="CardActivationTimer" />, reproducing the original's I-TURNOFF-* turn-off daemons
/// (planetfall/globals.zil) that the port originally omitted (issue #399).
/// </summary>
internal interface ICardActivatedDevice : ILocation, ITurnBasedActor
{
    /// <summary>Whether the device is currently activated (its button / keys will work).</summary>
    bool IsEnabled { get; set; }

    /// <summary>
    /// Turns left before the activation lapses. Counted down each turn by <see cref="CardActivationTimer.Tick" />.
    /// </summary>
    int ActivationTurnsRemaining { get; set; }

    /// <summary>
    /// The message shown when the activation lapses while the player is standing in the device. The
    /// original stays silent when the timer fires while the player is elsewhere.
    /// </summary>
    string DeactivationAnnouncement { get; }
}
