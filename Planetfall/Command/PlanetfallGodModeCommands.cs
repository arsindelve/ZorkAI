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
    ///     Day 1 is the crash landing. The upper bound is derived from the chronometer's morning-time
    ///     table rather than restated here, so extending the calendar only means adding data there.
    /// </summary>
    private const int FirstDay = 1;

    private static int LastDay => Chronometer.LastDefinedDay;

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
    ///     Recognizes "god mode day &lt;n&gt;" and advances the calendar to day n, so playtesting can reach
    ///     day-gated content (the sickness escalation, the shrinking carry limit, the later days' tighter
    ///     sleep schedule) without sleeping through the intervening nights.
    ///     It ADVANCES rather than assigns: it replays the game's own day transition
    ///     (<see cref="SleepEngine.StartNewDay" />) once per day crossed, so every clock that is a function
    ///     of the day moves by the real rules and stays in step with each other. Assigning the state
    ///     directly would mean maintaining a second copy of "what a new day means" that silently drifts
    ///     from the wake-up routine - and would flatten the disease onto the calendar, undoing the
    ///     medicine for anyone who had treated themselves.
    ///     What it deliberately does NOT replay is the price of having slept - dropping the player's
    ///     inventory on the floor, spoiling their food, the nightly death rolls. That's sleep's cost, not
    ///     the calendar's, and a tester advancing days shouldn't pay it.
    ///     The calendar only moves forward: the transition has no inverse, so a request for the current
    ///     day or an earlier one is refused rather than faked.
    /// </summary>
    private static string? SetDay(PlanetfallContext context, string[] words)
    {
        if (!words.Contains("day"))
            return null;

        // Take the first number anywhere in the command, so every reasonable phrasing works ("day 5",
        // "set day to 5"). A "day" with no number at all isn't ours - fall through to the engine's
        // generic god-mode error rather than swallowing something meant as a different command.
        var argument = words.Select(w => int.TryParse(w, out var n) ? n : (int?)null)
            .FirstOrDefault(n => n.HasValue);
        if (argument is not { } day)
            return null;

        if (day < FirstDay || day > LastDay)
            return $"God mode: day must be between {FirstDay} and {LastDay}. ";

        if (day <= context.Day)
            return $"God mode: the calendar only moves forward, and it's already Day {context.Day}. ";

        while (context.Day < day)
        {
            // Unreachable in a consistent game - the sickness counter never leads the calendar, so
            // advancing to a valid day can't reach the fatal level. Guarded anyway rather than letting
            // god mode quietly strand a tester at counter 9, which the game considers dead.
            if (context.SicknessCounter + 1 >= SleepEngine.SicknessDeathLevel)
                return $"God mode: stopped on Day {context.Day} - one more day and the disease " +
                       "would have killed you. ";

            SleepEngine.StartNewDay(context);
        }

        // God-mode commands still take a Planetfall turn, so back the clock off by one tick and let the
        // end-of-turn increment land it exactly on the day's morning time.
        Repository.GetItem<Chronometer>().CurrentTime -= PlanetfallContext.TurnTimeIncrement;

        // Skipping the night also skipped its hazards, so the world may have moved on without the
        // player - let the context reconcile where they're standing against the day they're now in.
        return $"God mode: it is now Day {day}. " + context.OnGodModeCalendarJump();
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
