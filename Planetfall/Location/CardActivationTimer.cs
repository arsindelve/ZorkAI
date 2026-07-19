namespace Planetfall.Location;

/// <summary>
/// Shared turn-based expiry for card-activated devices (the teleportation booths and the
/// miniaturization booth). Sliding a valid card calls <see cref="Enable" />, which activates the device
/// and registers it as an actor; <see cref="Tick" /> (invoked from each device's <c>Act</c>) counts the
/// window down and clears the activation when it elapses. This reproduces the original's turn-off
/// daemons — both booths expire after 30 turns (<c>&lt;QUEUE I-TURNOFF-TELEPORTATION 30&gt;</c>,
/// globals.zil:1414; <c>&lt;QUEUE I-TURNOFF-MINI 30&gt;</c>, globals.zil:1424) — instead of the port's
/// former behaviour of staying activated forever (issue #399).
///
/// The lower/upper elevators are deliberately NOT routed through here: they already carry an equivalent
/// self-timer in <see cref="Kalamontee.ElevatorBase" />, entangled with their movement-reset counter.
/// </summary>
internal static class CardActivationTimer
{
    /// <summary>The original's activation window for both the teleportation and miniaturization booths.</summary>
    public const int DefaultWindow = 30;

    /// <summary>
    /// Activates the device and starts (or, on a re-slide, restarts) its expiry countdown.
    /// </summary>
    public static void Enable(ICardActivatedDevice device, IContext context, int window = DefaultWindow)
    {
        device.IsEnabled = true;
        device.ActivationTurnsRemaining = window;
        // RegisterActor de-dupes by type, so re-sliding the card simply restarts the countdown here.
        context.RegisterActor(device);
    }

    /// <summary>
    /// Advances the countdown by one turn. Returns the deactivation announcement on the turn the window
    /// lapses (and only when the player is standing in the device, matching the original); otherwise
    /// returns an empty string.
    /// </summary>
    public static string Tick(ICardActivatedDevice device, IContext context)
    {
        // Defensive: if something already cleared the activation (e.g. a teleport that used the booth),
        // there is nothing left to count down - just stop acting.
        if (!device.IsEnabled)
        {
            context.RemoveActor(device);
            return string.Empty;
        }

        if (device.ActivationTurnsRemaining > 0)
        {
            device.ActivationTurnsRemaining--;
            return string.Empty;
        }

        // The window has elapsed - deactivate. The original announces the lapse only if you are standing
        // in the device when it fires (I-TURNOFF-TELEPORTATION / I-TURNOFF-MINI); elsewhere it is silent.
        device.IsEnabled = false;
        context.RemoveActor(device);
        return context.CurrentLocation == device ? device.DeactivationAnnouncement : string.Empty;
    }

    /// <summary>
    /// Cancels an active countdown because the device was used before it lapsed (mirrors
    /// <c>&lt;DISABLE &lt;INT I-TURNOFF-TELEPORTATION&gt;&gt;</c> on a successful teleport, globals.zil:1522).
    /// </summary>
    public static void Cancel(ICardActivatedDevice device, IContext context)
    {
        device.IsEnabled = false;
        device.ActivationTurnsRemaining = 0;
        context.RemoveActor(device);
    }
}
