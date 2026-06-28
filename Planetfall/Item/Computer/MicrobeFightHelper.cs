using Planetfall.Item.Kalamontee.Mech;

namespace Planetfall.Item.Computer;

/// <summary>
/// Shared helpers for disposing of the laser and the microbe during the strip battle, used by both
/// <see cref="Microbe" /> (give-the-laser path) and <see cref="Laser" /> (throw-off-the-strip path).
/// </summary>
public static class MicrobeFightHelper
{
    /// <summary>
    /// Removes the laser from play entirely and stops its warmth daemon (it's gone over the edge or
    /// down the microbe's gullet).
    /// </summary>
    public static void RemoveLaserFromGame(Laser laser, IContext context)
    {
        var holder = laser.CurrentLocation;
        holder?.RemoveItem(laser);
        laser.CurrentLocation = null;
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
