using GameEngine;
using Model.Interface;

namespace Stationfall;

/// <summary>
///     Game state for Stationfall. Phase 1 is deliberately minimal: the day counter, a pinnable
///     chronometer reading (the determinism hook the ship-departure walkthrough needs), and the
///     score/time strings. The hunger and sleep survival clocks — and the <c>Chronometer</c> item —
///     are Phase 3, reused from Planetfall (see <c>PlanetfallContext</c>).
/// </summary>
public class StationfallContext : Context<StationfallGame>, ITimeBasedContext
{
    // Days advance ONLY by sleeping in the original (WAKING-UP, globals.zil:1059). The sleep/hunger
    // engines that drive that are Phase 3; until then Day is a plain counter that starts on Day 1.
    public int Day { get; set; } = 1;

    // The chronometer reading. The original seeds it 4430 + RANDOM(1220) (misc.zil:190), advances it
    // ~7 per turn (C-ELAPSED, misc.zil:601), and FREEZES it after Day 2. Kept as a plain, settable
    // counter so walkthrough tests can pin it — the spacetruck course is computed from it (Phase 2,
    // see Walkthroughs/Walkthrough-ShipDeparture.md).
    internal const int TurnTimeIncrement = 7;

    public int CurrentTime { get; set; } = 4430;

    public string CurrentTimeResponse =>
        Day <= 2
            ? $"According to the chronometer, the current time is {CurrentTime}. "
            : "The chronometer appears to have stopped. ";

    public override string CurrentScore =>
        $"Your score would be {Score} (out of 80 points). It is Day {Day} of your adventure. " +
        $"\nThis score gives you the rank of {Game.GetScoreDescription(Score)}. ";

    public override string? ProcessEndOfTurn()
    {
        // The chronometer freezes after Day 2 in the original (misc.zil); mirror that here.
        if (Day <= 2)
            CurrentTime += TurnTimeIncrement;

        return base.ProcessEndOfTurn();
    }
}
