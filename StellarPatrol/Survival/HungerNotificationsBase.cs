using Utilities;

namespace StellarPatrol;

/// <summary>
///     Drives the shared hunger/thirst ladder: a tick-scheduled chain of warnings that escalates one
///     <see cref="HungerLevel" /> at a time and ends in death if the player never eats.
///     The mechanism is identical in both Stellar Patrol games; only the intervals differ, so a game
///     supplies those by deriving and leaves everything else alone.
/// </summary>
public abstract class HungerNotificationsBase
{
    /// <summary>
    ///     Ticks from the start of a stretch of not-eating to the first warning.
    /// </summary>
    protected abstract int InitialWarningTicks { get; }

    /// <summary>
    ///     Ticks spent at each level before escalating to the next. Keyed by the level just reached, so
    ///     the value is how long the player has to fix it before the next warning lands.
    /// </summary>
    protected abstract IReadOnlyDictionary<HungerLevel, int> Intervals { get; }

    /// <summary>
    ///     Game time at which the next warning comes due. Public and settable because it is part of the
    ///     serialized save state.
    /// </summary>
    [UsedImplicitly]
    public int NextWarningAt { get; set; }

    /// <summary>
    ///     Starts the clock. Must run after the chronometer exists, since it is relative to current time.
    /// </summary>
    public void Initialize(int currentTime)
    {
        NextWarningAt = currentTime + InitialWarningTicks;
    }

    /// <summary>
    ///     The warning to print if one is due, and schedules the following one. Null when nothing is due.
    /// </summary>
    public string? GetNotification(int currentTime, HungerLevel currentHungerLevel)
    {
        if (currentHungerLevel == HungerLevel.Dead)
            return null;

        if (currentTime < NextWarningAt)
            return null;

        var nextLevel = currentHungerLevel + 1;

        if (nextLevel < HungerLevel.Dead && Intervals.TryGetValue(nextLevel, out var interval))
            NextWarningAt = currentTime + interval;

        return nextLevel.GetNotification();
    }

    /// <summary>
    ///     Pushes the next warning out after a meal. Different foods buy different amounts of time.
    /// </summary>
    public void ResetAfterEating(int currentTime, int resetDuration)
    {
        NextWarningAt = currentTime + resetDuration;
    }

    /// <summary>
    ///     The level the player should now be at, or null if the current one still stands.
    /// </summary>
    public HungerLevel? GetNextHungerLevel(int currentTime, HungerLevel currentLevel)
    {
        if (currentTime >= NextWarningAt && currentLevel < HungerLevel.Dead)
            return currentLevel + 1;

        return null;
    }
}
