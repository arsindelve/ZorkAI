namespace Model.Interface;

/// <summary>
///     Marker interface for game contexts that run early-game, location-blind timed-death mechanics
///     (e.g. Planetfall's Feinstein explosion clock) which only check where the player currently is,
///     not how they got there. "god mode go &lt;place&gt;" is a raw location swap for testing/debugging -
///     it does not run the normal OnLeaveLocation/AfterEnterLocation hooks - so a tester who teleports
///     away from the danger zone never trips the hook that would otherwise disarm the clock. It keeps
///     counting turns in the background and can silently kill the tester once the move count rolls
///     into its death window, far from whatever they were actually testing. Games that implement this
///     get a chance to disarm those clocks when a god-mode teleport happens. Games without such
///     mechanics do not implement this.
/// </summary>
public interface IGodModeTeleportAware
{
    /// <summary>
    ///     Returns a note to append to the teleport confirmation, or null when there is nothing to say.
    ///     A game may want to warn that the raw swap has landed the player somewhere its own rules would
    ///     never allow - god mode still obeys the request, but says so rather than leaving it silent.
    /// </summary>
    string? OnGodModeTeleport();
}
