using GameEngine;
using Model.Interface;

namespace Stationfall;

/// <summary>
///     Game state for Stationfall. The chronometer reading lives on the <see cref="Chronometer" /> item
///     (mirroring Planetfall) and is surfaced here for the engine's time-aware plumbing; the autopilot
///     course is derived from it, so tests can pin the item's value and get a reproducible heading.
///     The hunger and sleep survival clocks are Phase 3, reused from Planetfall.
/// </summary>
public class StationfallContext : Context<StationfallGame>, ITimeBasedContext
{
    /// <summary>
    ///     Millichrons added to the chronometer each turn (C-ELAPSED, misc.zil:601).
    /// </summary>
    internal const int TurnTimeIncrement = 7;

    // Days advance ONLY by sleeping in the original (WAKING-UP, globals.zil:1059). The sleep engine
    // that drives that is Phase 3; until then Day is a plain counter that starts on Day 1.
    [UsedImplicitly]
    public int Day { get; set; } = 1;

    /// <summary>
    ///     Number of times the player has died, preserved across death restarts.
    /// </summary>
    [UsedImplicitly]
    public int DeathCounter { get; set; }

    public int CurrentTime => Repository.GetItem<Chronometer>().CurrentTime;

    public string CurrentTimeResponse
    {
        get
        {
            var chronometer = Repository.GetItem<Chronometer>();

            if (!chronometer.BeingWorn)
                return "You aren't wearing your chronometer. ";

            return chronometer.HasStopped
                ? "Your chronometer appears to have stopped. "
                : $"According to your chronometer, the current time is {CurrentTime}. ";
        }
    }

    public override string CurrentScore =>
        $"Your score would be {Score} (out of 80 points). It is Day {Day} of your adventure. " +
        $"\nThis score gives you the rank of {Game.GetScoreDescription(Score)}. ";

    /// <summary>
    ///     The player begins carrying the three Patrol forms (physical feelies in the original, so they
    ///     are in inventory rather than lying in a room — ship.zil:36-61), plus the worn chronometer and
    ///     uniform.
    /// </summary>
    public override void Init()
    {
        StartWithItem<Chronometer>(this);
        StartWithItem<PatrolUniform>(this);
        StartWithItem<AssignmentCompletionForm>(this);
        StartWithItem<RobotUseAuthorizationForm>(this);
        StartWithItem<ClassThreeSpacecraftActivationForm>(this);
    }

    public override string? ProcessEndOfTurn()
    {
        var chronometer = Repository.GetItem<Chronometer>();

        if (!chronometer.HasStopped)
            chronometer.CurrentTime += TurnTimeIncrement;

        return base.ProcessEndOfTurn();
    }

    public override int GetDeathCount()
    {
        return DeathCounter;
    }

    public override void SetDeathCount(int count)
    {
        DeathCounter = count;
    }
}
