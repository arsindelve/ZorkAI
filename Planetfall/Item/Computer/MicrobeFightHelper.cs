using Planetfall.Item.Kalamontee.Mech;

namespace Planetfall.Item.Computer;

/// <summary>
/// Shared helpers for disposing of the laser and the microbe during the strip battle, used by both
/// <see cref="Microbe" /> (give-the-laser path) and <see cref="Laser" /> (throw-off-the-strip path).
/// </summary>
public static class MicrobeFightHelper
{
    // Warmth-timing note: the microbe's lash-out reaction evaluates these thresholds against
    // Microbe.WarmthAtHit — the warmth captured in Laser.ShootMicrobe at the instant of the strike,
    // before that shot's own increment. This is "warmth coming into the turn" and matches the
    // original (I-MICROBE reads WARMTH-FLAG before I-WARMTH bumps it). Capturing at strike time
    // rather than reading laser.WarmthLevel live in Act keeps the thresholds independent of
    // actor-registration order. (The give/throw-off-strip paths read warmth synchronously during the
    // player's action, so they're already order-independent.)

    /// <summary>
    /// Laser warmth (WARMTH-FLAG) above which the microbe is drawn to the heat: it lunges after a
    /// laser thrown off the strip, devours one fed to it, and grabs for one you're still holding.
    /// </summary>
    public const int RepelWarmth = 7;

    /// <summary>
    /// Laser warmth above which feeding the laser to the microbe is fatal to it (it overheats and
    /// rolls off the strip); at or below this it just eats the laser and turns on you.
    /// </summary>
    public const int FeedKillWarmth = 10;

    /// <summary>
    /// Laser warmth above which holding the laser when the microbe lashes out is fatal — it lunges at
    /// the pulsing heat and drags you both into the void.
    /// </summary>
    public const int LethalLungeWarmth = 13;

    /// <summary>
    /// Removes the laser from play entirely and stops its warmth daemon (it's gone over the edge or
    /// down the microbe's gullet).
    /// </summary>
    public static void RemoveLaserFromGame(Laser laser, IContext context)
    {
        var holder = laser.CurrentLocation;
        holder?.RemoveItem(laser);
        laser.CurrentLocation = null;
        // Clear the warmth state so a stale value can't linger on the singleton if the game restarts
        // (e.g. after a digest/lunge death) without a full Repository reset.
        laser.WarmthLevel = 0;
        laser.JustShot = false;
        context.RemoveActor(laser);
    }

    /// <summary>
    /// Permanently disposes of the microbe — clears it from the strip so the exits open again
    /// (NO-MICROBE = T) and marks it dispatched so it never respawns (MICROBE-DISPATCHED = T).
    /// </summary>
    public static void Dispatch(Microbe microbe, IContext context)
    {
        microbe.IsActive = false;
        microbe.Dispatched = true;
        microbe.CurrentLocation?.RemoveItem(microbe);
        microbe.CurrentLocation = null;
        context.RemoveActor(microbe);
    }
}
