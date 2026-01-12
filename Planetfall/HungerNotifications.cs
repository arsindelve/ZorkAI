using Utilities;

namespace Planetfall;

/// <summary>
/// Manages hunger/thirst notifications based on the timer system.
/// The game uses a unified hunger/thirst system with progressive warnings.
/// Time intervals have been doubled from the original Infocom values for a less punishing experience.
/// </summary>
public class HungerNotifications
{
    /// <summary>
    /// Initial time in ticks before first hunger warning appears.
    /// </summary>
    private const int InitialWarningTicks = 4000;

    /// <summary>
    /// Time intervals between hunger level progressions.
    /// </summary>
    private const int HungryToRavenousTicks = 900;
    private const int RavenousToFaintTicks = 300;
    private const int FaintToAboutToPassOutTicks = 200;
    private const int AboutToPassOutToDeadTicks = 100;

    // Notification schedule: (initial: 4000) -> (900) -> (300) -> (200) -> (100) -> death
    private static readonly Dictionary<HungerLevel, int> NextWarningTimes = new()
    {
        { HungerLevel.WellFed, InitialWarningTicks },           // Initial warning time
        { HungerLevel.Hungry, HungryToRavenousTicks },          // Time until ravenous
        { HungerLevel.Ravenous, RavenousToFaintTicks },         // Time until faint
        { HungerLevel.Faint, FaintToAboutToPassOutTicks },      // Time until about to pass out
        { HungerLevel.AboutToPassOut, AboutToPassOutToDeadTicks } // Time until death
    };

    [UsedImplicitly]
    public int NextWarningAt { get; set; }

    /// <summary>
    /// Initializes the hunger notification system with the current game time.
    /// Must be called after the Chronometer is initialized.
    /// </summary>
    internal void Initialize(int currentTime)
    {
        NextWarningAt = currentTime + InitialWarningTicks;
    }

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
