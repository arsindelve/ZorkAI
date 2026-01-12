using Utilities;

namespace Planetfall;

/// <summary>
/// Manages hunger/thirst notifications based on the timer system.
/// The game uses a unified hunger/thirst system with progressive warnings.
/// Initial warning at 2000 ticks, then escalating warnings at 450, 150, 100, 50 tick intervals.
/// </summary>
public class HungerNotifications
{
    // Notification schedule: (initial: 2000) -> (450) -> (150) -> (100) -> (50) -> death
    private static readonly Dictionary<HungerLevel, int> NextWarningTimes = new()
    {
        { HungerLevel.WellFed, 2000 },    // Initial warning time
        { HungerLevel.Hungry, 450 },      // Time until ravenous
        { HungerLevel.Ravenous, 150 },    // Time until faint
        { HungerLevel.Faint, 100 },       // Time until about to pass out
        { HungerLevel.AboutToPassOut, 50 } // Time until death
    };

    [UsedImplicitly]
    public int NextWarningAt { get; set; } = 2000; // Initial warning at 2000 ticks

    /// <summary>
    /// Gets the notification message if it's time for the next hunger warning.
    /// Returns null if no warning should be shown yet.
    /// </summary>
    internal string? GetNotification(int currentTime, HungerLevel currentHungerLevel)
    {
        // If already dead, no notification
        if (currentHungerLevel == HungerLevel.Dead)
            return null;

        // Check if it's time for a warning
        if (currentTime < NextWarningAt)
            return null;

        // Return the notification for the next level
        var nextLevel = currentHungerLevel + 1;

        // Schedule the next warning
        if (nextLevel < HungerLevel.Dead && NextWarningTimes.ContainsKey(nextLevel))
        {
            NextWarningAt = currentTime + NextWarningTimes[nextLevel];
        }

        return nextLevel.GetNotification();
    }

    /// <summary>
    /// Resets the hunger notification system after eating.
    /// Different foods provide different reset durations.
    /// </summary>
    internal void ResetAfterEating(int currentTime, int resetDuration)
    {
        NextWarningAt = currentTime + resetDuration;
    }

    /// <summary>
    /// Gets the hunger level that should be applied based on current time and warning schedule.
    /// This is used to determine if the player has progressed to the next hunger level.
    /// </summary>
    internal HungerLevel? GetNextHungerLevel(int currentTime, HungerLevel currentLevel)
    {
        if (currentTime >= NextWarningAt && currentLevel < HungerLevel.Dead)
        {
            return currentLevel + 1;
        }
        return null;
    }
}
