namespace Planetfall.Item.Feinstein;

/// <summary>
///     Planetfall's wrist chronometer. Adds to the shared <see cref="ChronometerBase" /> the one thing
///     only this game does: each morning starts later than the last, so the clock itself reports the
///     disease wearing the player down.
/// </summary>
public class Chronometer : ChronometerBase
{
    private static readonly Random Random = new();

    /// <summary>
    ///     Morning wake-up times per day (see Docs/SLEEP_MECHANICS.md, reset_time).
    /// </summary>
    private static readonly Dictionary<int, int> MorningTimesByDay = new()
    {
        { 1, 4600 }, // Game start time (not used for wake-up)
        { 2, 1600 },
        { 3, 1750 },
        { 4, 1950 },
        { 5, 2150 },
        { 6, 2450 },
        { 7, 2800 },
        { 8, 3200 }
    };

    /// <summary>
    ///     Spread of the wake-up time past the day's base morning time, so no two mornings land on
    ///     exactly the same tick.
    /// </summary>
    public const int MorningJitterTicks = 80;

    /// <summary>
    ///     The last day the morning-time table defines. The playable calendar can't outrun this: waking
    ///     on a day with no entry would leave the chronometer on the previous day's time.
    /// </summary>
    public static int LastDefinedDay => MorningTimesByDay.Keys.Max();

    /// <summary>
    ///     The game starts at a random time between 4500 and 4700. This stays on the raw Random rather
    ///     than the injected chooser because it runs during construction, before anything could be
    ///     injected - tests pin the start time by assigning CurrentTime directly.
    /// </summary>
    public Chronometer()
    {
        CurrentTime = Random.Next(4500, 4700);
    }

    public override string[] NounsForMatching => ["chronometer", "watch", "wrist-watch"];

    protected override string TimeDescription =>
        "It is a standard wrist chronometer with a digital display. According to the chronometer, the " +
        $"current time is {CurrentTime}. The back is engraved with the message \"Good luck in the " +
        "Patrol! Love, Mom and Dad.\"";

    public override string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a chronometer here. ";
    }

    /// <summary>
    ///     Resets the time to morning for the given day (called on wake-up).
    /// </summary>
    public void ResetToMorning(int day)
    {
        if (MorningTimesByDay.TryGetValue(day, out var baseTime))
            // RollDice is 1-based (1..sides); subtract one to keep the original 0-based spread, so the
            // player can still wake at the day's exact base time.
            CurrentTime = baseTime + Chooser.RollDice(MorningJitterTicks) - 1;
    }
}
