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
    public static string? Handle(PlanetfallContext context, string input)
    {
        var words = input.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        // Order is deliberate and load-bearing: e.g. "god mode reset sleep clock" must hit ResetClock
        // ("reset" + "clock") before the survival toggle ever sees "sleep". This preserves the branch
        // order these commands had in the engine's GodModeProcessor before they moved here.
        return ResetClock(words) ?? ToggleSurvivalClocks(context, words) ?? ToggleFloydWandering(context, words);
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
