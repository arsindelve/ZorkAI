using Utilities;

namespace StellarPatrol;

/// <summary>
///     Drives the shared fatigue ladder and the two ways it ends: drifting off voluntarily after
///     lying down, or dropping where you stand once <see cref="TiredLevel.AboutToDrop" /> runs out.
///     As with hunger, the mechanism and its wording are common to both Stellar Patrol games; a game
///     supplies its own intervals by deriving.
/// </summary>
public abstract class SleepNotificationsBase
{
    /// <summary>
    ///     Ticks spent at each fatigue level before escalating. Keyed by the level just reached.
    /// </summary>
    protected abstract IReadOnlyDictionary<TiredLevel, int> Intervals { get; }

    /// <summary>
    ///     Ticks from waking on the given day to that day's first fatigue warning. Games that make the
    ///     days progressively harder vary this; games that do not can ignore the parameter.
    /// </summary>
    protected abstract int InitialWarningTicksForDay(int day);

    /// <summary>
    ///     Ticks between lying down tired and actually falling asleep. Overridable because the two games
    ///     use different delays, and one of them uses a different delay depending on how you got to bed.
    /// </summary>
    protected virtual int FallAsleepDelayTicks => 16;

    [UsedImplicitly] public int NextWarningAt { get; set; }

    [UsedImplicitly] public bool FallAsleepQueued { get; set; }

    [UsedImplicitly] public int FallAsleepAt { get; set; }

    /// <summary>
    ///     Starts the clock on day one. Must run after the chronometer exists.
    /// </summary>
    public void Initialize(int currentTime)
    {
        NextWarningAt = currentTime + InitialWarningTicksForDay(1);
    }

    /// <summary>
    ///     Re-arms the fatigue clock for a new day and clears any pending drift-off.
    /// </summary>
    public void ResetForNewDay(int currentTime, int day)
    {
        FallAsleepQueued = false;
        FallAsleepAt = 0;
        NextWarningAt = currentTime + InitialWarningTicksForDay(day);
    }

    /// <summary>
    ///     The warning to print if one is due, and schedules the following one. Null when nothing is due.
    /// </summary>
    public string? GetNotification(int currentTime, TiredLevel currentTiredLevel)
    {
        if (currentTime < NextWarningAt)
            return null;

        var nextLevel = currentTiredLevel + 1;

        if (Intervals.TryGetValue(nextLevel, out var interval))
            NextWarningAt = currentTime + interval;

        return nextLevel.GetNotification();
    }

    /// <summary>
    ///     The fatigue level the player should now be at, or null if the current one still stands.
    /// </summary>
    public TiredLevel? GetNextTiredLevel(int currentTime, TiredLevel currentLevel)
    {
        if (currentTime >= NextWarningAt && currentLevel < TiredLevel.AboutToDrop)
            return currentLevel + 1;

        return null;
    }

    /// <summary>
    ///     The "already in bed" special case. When a fatigue warning comes due while the player is
    ///     lying down, neither original nags them to go find a bed - it settles them in and starts them
    ///     drifting off instead. Returns that message (having queued the drift-off) when the case
    ///     applies, and null - changing nothing - when it does not, so the caller runs the normal
    ///     escalation.
    /// </summary>
    public string? TrySettleIntoBed(int currentTime, bool isInBed)
    {
        // Only when a warning is actually due, the player is lying down, and they aren't already
        // drifting off - otherwise this would re-queue the timer and double-report every turn.
        if (currentTime < NextWarningAt || !isInBed || FallAsleepQueued)
            return null;

        QueueFallAsleep(currentTime);
        return "You suddenly realize how tired you were and how comfortable the bed is. " +
               "You should be asleep in no time. ";
    }

    /// <summary>
    ///     Starts the player drifting off. Pass <paramref name="delayTicks" /> to override the default
    ///     for a particular route into bed.
    /// </summary>
    public void QueueFallAsleep(int currentTime, int? delayTicks = null)
    {
        FallAsleepQueued = true;
        FallAsleepAt = currentTime + (delayTicks ?? FallAsleepDelayTicks);
    }

    public void CancelFallAsleep()
    {
        FallAsleepQueued = false;
        FallAsleepAt = 0;
    }

    public bool ShouldFallAsleep(int currentTime)
    {
        return FallAsleepQueued && currentTime >= FallAsleepAt;
    }

    /// <summary>
    ///     True once fatigue has run all the way out and the player collapses wherever they are.
    /// </summary>
    public bool ShouldForceSleep(int currentTime, TiredLevel currentLevel)
    {
        return currentLevel == TiredLevel.AboutToDrop && currentTime >= NextWarningAt;
    }
}
