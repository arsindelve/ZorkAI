using Utilities;

namespace Planetfall;

/// <summary>
/// Manages sleep/fatigue notifications based on the timer system.
/// The game uses a progressive warning system that increases time pressure each day.
/// Time intervals are based on the original Infocom implementation.
/// </summary>
public class SleepNotifications
{
    /// <summary>
    /// Initial time in ticks before first sleep warning appears (Day 1).
    /// </summary>
    private const int InitialWarningTicks = 3600;

    /// <summary>
    /// Time intervals between sleep level progressions.
    /// </summary>
    private const int TiredToVeryTiredTicks = 400;
    private const int VeryTiredToExhaustedTicks = 135;
    private const int ExhaustedToAboutToDropTicks = 60;
    private const int AboutToDropToForcedSleepTicks = 50;

    // Notification schedule: (varies by day) -> (400) -> (135) -> (60) -> (50) -> forced sleep
    private static readonly Dictionary<TiredLevel, int> NextWarningTimes = new()
    {
        { TiredLevel.Tired, TiredToVeryTiredTicks },
        { TiredLevel.VeryTired, VeryTiredToExhaustedTicks },
        { TiredLevel.Exhausted, ExhaustedToAboutToDropTicks },
        { TiredLevel.AboutToDrop, AboutToDropToForcedSleepTicks }
    };

    /// <summary>
    /// Ticks until first warning per day (progressively shorter each day).
    /// </summary>
    private static readonly Dictionary<int, int> DailyInitialWarningTimes = new()
    {
        { 1, InitialWarningTicks },  // Day 1: 3600 ticks
        { 2, 5800 },                 // Day 2: 5800 ticks
        { 3, 5550 },                 // Day 3: 5550 ticks
        { 4, 5200 },                 // Day 4: 5200 ticks
        { 5, 4800 },                 // Day 5: 4800 ticks
        { 6, 4300 },                 // Day 6: 4300 ticks
        { 7, 3700 },                 // Day 7: 3700 ticks
        { 8, 3000 }                  // Day 8: 3000 ticks (Day 9 = death)
    };

    [UsedImplicitly]
    public int NextWarningAt { get; set; }

    [UsedImplicitly]
    public bool FallAsleepQueued { get; set; }

    [UsedImplicitly]
    public int FallAsleepAt { get; set; }

    /// <summary>
    /// Initializes the sleep notification system with the current game time.
    /// Must be called after the Chronometer is initialized.
    /// </summary>
    internal void Initialize(int currentTime)
    {
        NextWarningAt = currentTime + InitialWarningTicks;
    }

    /// <summary>
    /// Resets the sleep timer for a new day with progressively shorter intervals.
    /// </summary>
    internal void ResetForNewDay(int currentTime, int day)
    {
        FallAsleepQueued = false;
        FallAsleepAt = 0;

        if (DailyInitialWarningTimes.TryGetValue(day, out var initialTicks))
        {
            NextWarningAt = currentTime + initialTicks;
        }
        else
        {
            // Shouldn't reach day 9+, but default to day 8 timing if we do
            NextWarningAt = currentTime + DailyInitialWarningTimes[8];
        }
    }

    /// <summary>
    /// Gets the notification message if it's time for the next sleep warning.
    /// Returns null if no warning should be shown yet.
    /// </summary>
    internal string? GetNotification(int currentTime, TiredLevel currentTiredLevel)
    {
        // Check if it's time for a warning
        if (currentTime < NextWarningAt)
            return null;

        // Return the notification for the next level
        var nextLevel = currentTiredLevel + 1;

        // Schedule the next warning
        if (NextWarningTimes.ContainsKey(nextLevel))
        {
            NextWarningAt = currentTime + NextWarningTimes[nextLevel];
        }

        return nextLevel.GetNotification();
    }

    /// <summary>
    /// Gets the next tired level that should be applied based on current time and warning schedule.
    /// This is used to determine if the player has progressed to the next tired level.
    /// </summary>
    internal TiredLevel? GetNextTiredLevel(int currentTime, TiredLevel currentLevel)
    {
        if (currentTime >= NextWarningAt && currentLevel < TiredLevel.AboutToDrop)
        {
            return currentLevel + 1;
        }
        return null;
    }

    /// <summary>
    /// Queues the fall asleep interrupt (16 ticks after entering bed while tired).
    /// </summary>
    internal void QueueFallAsleep(int currentTime)
    {
        FallAsleepQueued = true;
        FallAsleepAt = currentTime + 16;
    }

    /// <summary>
    /// Cancels the queued fall asleep interrupt.
    /// </summary>
    internal void CancelFallAsleep()
    {
        FallAsleepQueued = false;
        FallAsleepAt = 0;
    }

    /// <summary>
    /// Checks if it's time to fall asleep (when queued).
    /// </summary>
    internal bool ShouldFallAsleep(int currentTime)
    {
        return FallAsleepQueued && currentTime >= FallAsleepAt;
    }

    /// <summary>
    /// Checks if forced sleep should occur (reached maximum tiredness).
    /// </summary>
    internal bool ShouldForceSleep(int currentTime, TiredLevel currentLevel)
    {
        return currentLevel == TiredLevel.AboutToDrop && currentTime >= NextWarningAt;
    }
}
