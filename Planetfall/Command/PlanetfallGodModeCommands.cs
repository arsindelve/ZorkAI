namespace Planetfall.Command;

/// <summary>
///     Planetfall's game-specific god-mode subcommands, kept in their own class so
///     <see cref="PlanetfallContext" /> stays focused on game state. The engine's god-mode processor
///     delegates unrecognized commands to the context via IGodModeCommandHandler, and the context
///     forwards here. The toggled flags themselves live on the context so they round-trip through
///     the save/restore JSON.
/// </summary>
public static class PlanetfallGodModeCommands
{
    /// <summary>
    ///     The playable calendar. Day 1 is the crash landing; the disease kills on day 9, and both the
    ///     chronometer's morning times and the per-day sleep pressure are only defined through day 8.
    /// </summary>
    private const int FirstDay = 1;

    private const int LastDay = 8;

    /// <summary>
    ///     Ticks until the first hunger warning of a freshly-started day, matching the well-fed branch of
    ///     <see cref="SleepEngine" />'s wake-up routine.
    /// </summary>
    private const int FreshDayHungerWarningTicks = 800;

    public static string? Handle(PlanetfallContext context, string input)
    {
        var words = input.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        // Order is deliberate and load-bearing: e.g. "god mode reset sleep clock" must hit ResetClock
        // ("reset" + "clock") before the survival toggle ever sees "sleep". This preserves the branch
        // order these commands had in the engine's GodModeProcessor before they moved here.
        // SetDay sits next to ResetClock because they're both clock surgery - it matches on "day" plus a
        // number, which none of the other branches' trigger words can collide with.
        return ResetClock(words) ?? SetDay(context, words) ?? ToggleSurvivalClocks(context, words) ??
               ToggleFloydWandering(context, words);
    }

    /// <summary>
    ///     Recognizes "god mode reset time|clock" and resets the chronometer to the walkthrough
    ///     start-of-day time, so playtesting can dodge time-gated blockers (e.g. Alfie's evening
    ///     shuttle cutoff) deterministically.
    /// </summary>
    private static string? ResetClock(string[] words)
    {
        if (!words.Contains("reset") || (!words.Contains("time") && !words.Contains("clock")))
            return null;

        const int walkthroughResetTime = 2000;
        // God-mode commands still take a Planetfall turn, so compensate for the end-of-turn tick.
        Repository.GetItem<Chronometer>().CurrentTime = walkthroughResetTime - PlanetfallContext.TurnTimeIncrement;
        return $"God mode: chronometer reset to {walkthroughResetTime}.";
    }

    /// <summary>
    ///     Recognizes "god mode day &lt;n&gt;" and jumps the calendar to day n, so playtesting can reach
    ///     day-gated content (the sickness escalation, the shrinking carry limit, the later days' tighter
    ///     sleep schedule) without sleeping through the intervening nights.
    ///     The jump lands the player at the START of day n, the same state waking up would leave them in:
    ///     day-n morning on the chronometer, fatigue and hunger reset and rescheduled off that new time,
    ///     and the disease at the level an untreated player would have reached. Doing less would leave an
    ///     incoherent clock - the day-n sickness warning only fires before that day's cutoff time, and a
    ///     stale hunger/sleep warning time carried over from the old day sits thousands of ticks in the
    ///     future once the chronometer rolls back to morning. What it deliberately does NOT do is the rest
    ///     of the wake-up routine (dropping the player's inventory on the floor, spoiling their food):
    ///     that's sleep's cost, not the calendar's, and a tester jumping days shouldn't pay it.
    /// </summary>
    private static string? SetDay(PlanetfallContext context, string[] words)
    {
        var index = Array.IndexOf(words, "day");
        if (index == -1 || index == words.Length - 1)
            return null;

        // A bare "day" with no number isn't ours - fall through to the engine's generic god-mode error
        // rather than swallowing something the player meant as a different command.
        if (!int.TryParse(words[index + 1], out var day))
            return null;

        if (day < FirstDay || day > LastDay)
            return $"God mode: day must be between {FirstDay} and {LastDay}. ";

        context.Day = day;

        // Untreated, the disease tracks the calendar (SicknessCounter starts at 1 == day 1 and advances
        // one level per night). Leaving it behind would silently cure a tester who jumps to day 7.
        context.SicknessCounter = day;

        var chronometer = Repository.GetItem<Chronometer>();
        chronometer.ResetToMorning(day);
        var morningTime = chronometer.CurrentTime;

        context.Tired = TiredLevel.WellRested;
        context.SleepNotifications.ResetForNewDay(morningTime, day);

        // Mirrors the well-fed branch of the wake-up routine in SleepEngine: a fresh day starts fed,
        // with the first hunger warning 800 ticks out.
        context.Hunger = HungerLevel.WellFed;
        context.HungerNotifications.NextWarningAt = morningTime + FreshDayHungerWarningTicks;

        // Let the new day's sickness warning fire even if the player has already been on this day.
        context.SicknessNotifications.DaysNotified.Remove(day);

        // God-mode commands still take a Planetfall turn, so back the clock off by one tick and let the
        // end-of-turn increment land it exactly on the day's morning time.
        chronometer.CurrentTime = morningTime - PlanetfallContext.TurnTimeIncrement;

        return $"God mode: it is now Day {day}. ";
    }

    /// <summary>
    ///     Issue #277: recognizes "god mode [no] sleep|hunger|survival" (with optional "on"/"off") and
    ///     flips the corresponding survival-clock flag. "no"/"off" disables a clock; anything else
    ///     re-enables it. "survival" affects both clocks at once.
    /// </summary>
    private static string? ToggleSurvivalClocks(PlanetfallContext context, string[] words)
    {
        var affectsSleep = words.Contains("sleep") || words.Contains("survival");
        var affectsHunger = words.Contains("hunger") || words.Contains("survival");

        if (!affectsSleep && !affectsHunger)
            return null;

        // "no" / "off" disables the clock; a bare verb (or explicit "on") re-enables it.
        var disable = words.Contains("no") || words.Contains("off");

        var affected = new List<string>();
        var effects = new List<string>();
        if (affectsSleep)
        {
            context.SleepClockDisabled = disable;
            affected.Add("sleep");
            effects.Add("tired");
        }

        if (affectsHunger)
        {
            context.HungerClockDisabled = disable;
            affected.Add("hunger");
            effects.Add("hungry");
        }

        var clocks = string.Join(" and ", affected);
        var noun = affected.Count > 1 ? "clocks" : "clock";
        return disable
            ? $"God mode: {clocks} {noun} disabled. You will no longer get {string.Join(" or ", effects)}. "
            : $"God mode: {clocks} {noun} enabled. ";
    }

    /// <summary>
    ///     Recognizes "god mode [no] wander|wandering" (with optional "on"/"off") and flips
    ///     <see cref="PlanetfallContext.FloydWanderingDisabled" /> - the same effect the walkthrough
    ///     tests get by mocking IRandomChooser to always fail Floyd's wandering rolls, but available
    ///     as a live command.
    /// </summary>
    private static string? ToggleFloydWandering(PlanetfallContext context, string[] words)
    {
        if (!words.Contains("wander") && !words.Contains("wandering"))
            return null;

        // "no" / "off" disables wandering; a bare verb (or explicit "on") re-enables it.
        var disable = words.Contains("no") || words.Contains("off");
        context.FloydWanderingDisabled = disable;

        return disable
            ? "God mode: Floyd's wandering disabled. He will no longer randomly wander off. "
            : "God mode: Floyd's wandering enabled. ";
    }
}
