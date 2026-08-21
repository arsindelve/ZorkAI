namespace Stationfall.Item.Duffy;

/// <summary>
///     The player's wrist chronometer, worn from the first turn. Reading it gives the Galactic Standard
///     Time that every survival clock is scheduled against — and, early on, the number the autopilot
///     course is derived from (see <c>Spacetruck</c>), which is why it has to report the exact value the
///     rest of the game consumes.
/// </summary>
public class Chronometer : ChronometerBase
{
    private static readonly Random Random = new();

    /// <summary>
    ///     Where a new morning starts. The original re-seeds the clock to 1600 plus a small spread on
    ///     waking (globals.zil:1067), so no two mornings begin on exactly the same tick.
    /// </summary>
    private const int MorningBaseTime = 1600;

    public const int MorningJitterTicks = 80;

    /// <summary>
    ///     Galactic Standard Time at the start of the game: 4430 plus a spread (misc.zil:190). The
    ///     spread is the point — the autopilot course is computed from this reading, so a randomised
    ///     start is what stops the player memorising a heading between games. It stays on the raw Random
    ///     rather than the injected chooser because it runs during construction, before anything could
    ///     be injected; tests pin it by assigning CurrentTime directly.
    /// </summary>
    public Chronometer()
    {
        CurrentTime = Random.Next(4431, 5651);
    }

    // "time" is included so "read time" / "check time" work — the original has a dedicated TIME verb
    // (verbs.zil V-TIME), and reading the time is a required step before setting the truck's course.
    public override string[] NounsForMatching =>
        ["chronometer", "watch", "wristwatch", "wrist watch", "time"];

    public override int Size => 8;

    /// <summary>
    ///     The chronometer mysteriously stops once the player is past their second day
    ///     (ship.zil:91-95). Set by the day transition rather than derived, so it survives a save.
    /// </summary>
    [UsedImplicitly]
    public bool HasStopped { get; set; }

    protected override string TimeDescription
    {
        get
        {
            // No reading unless it's on your wrist (the original's V-TIME gates on WORNBIT).
            if (!BeingWorn)
                return "It is a standard wrist chronometer with a digital display. You'd have to be " +
                       "wearing it to read the time. ";

            if (HasStopped)
                return "You glance at your chronometer and realize, with some annoyance, that it has " +
                       "stopped. You can't recall doing anything that would have broken it. ";

            return "It is a standard wrist chronometer with a digital display. Current Galactic Standard " +
                   $"Time, adjusted to your local day-cycle, is {CurrentTime}. The back is engraved with a " +
                   "fond message from your parents. ";
        }
    }

    public override string OnTheGroundDescription(ILocation currentLocation)
    {
        return "A wrist chronometer is lying here. ";
    }

    /// <summary>
    ///     Re-seeds the clock to a new morning. This happens every night, including after the
    ///     chronometer has stopped: in the original it is only the *reading* that stops
    ///     (ship.zil:91-95), while the underlying time carries on being reset and advanced
    ///     (globals.zil:1067, misc.zil:362). Skipping the reset here would silently change how much of
    ///     each later day the survival clocks think has elapsed.
    /// </summary>
    public void ResetToMorning()
    {
        CurrentTime = MorningBaseTime + Chooser.RollDice(MorningJitterTicks);
    }
}
