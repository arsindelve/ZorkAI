namespace Stationfall.Item.Duffy;

/// <summary>
///     The player's wrist chronometer, worn from the start. Reading it gives the Galactic Standard Time
///     the autopilot course is computed from — see <c>Spacetruck</c>. In the original the chronometer
///     mysteriously stops after Day 2 (ship.zil:90-102), which is also why the course must be set on
///     the outbound trip.
/// </summary>
public class Chronometer : ItemBase, ICanBeTakenAndDropped, ICanBeExamined, ICanBeRead, IAmClothing
{
    // "time" is included so "read time" / "check time" work — the original has a dedicated TIME verb
    // (verbs.zil V-TIME), and reading the time is a required step before setting the truck's course.
    public override string[] NounsForMatching =>
        ["chronometer", "watch", "wristwatch", "wrist watch", "time"];

    public override int Size => 8;

    // Worn from the start of the game.
    public bool BeingWorn { get; set; } = true;

    private static readonly Random Random = new();

    /// <summary>
    ///     Galactic Standard Time, advanced each turn by the context. Seeded randomly, as in the
    ///     original (misc.zil:190), so the autopilot course differs between games — the player is meant
    ///     to read the clock and do the arithmetic, not memorize a heading. Tests pin this directly.
    ///     It stays on the raw Random rather than an injected chooser because a property initializer
    ///     runs before anything could be injected.
    /// </summary>
    [UsedImplicitly]
    public int CurrentTime { get; set; } = Random.Next(4431, 5651);

    /// <summary>
    ///     The chronometer mysteriously stops after Day 2 (ship.zil:92-95). Phase 3's sleep engine will
    ///     set this on waking into Day 3.
    /// </summary>
    [UsedImplicitly]
    public bool HasStopped { get; set; }

    public string ExaminationDescription => ReadDescription;

    public string ReadDescription => GetTimeDescription();

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "A wrist chronometer is lying here. ";
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A chronometer" + (BeingWorn ? " (being worn)" : "");
    }

    /// <summary>
    ///     Reading the time is the player's only way to derive the spacetruck's course heading, so this
    ///     has to report the exact number the formula consumes.
    /// </summary>
    private string GetTimeDescription()
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
