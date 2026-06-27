namespace Model.Interface;

/// <summary>
///     Marker interface for game contexts that maintain "survival" clocks - the sleep/fatigue and
///     hunger/thirst timers that advance every turn. Games with these mechanics (Planetfall) implement
///     this so god mode can switch the clocks off for deterministic playtesting; games without them
///     (Zork) do not. See issue #277.
/// </summary>
public interface ISurvivalClockContext
{
    /// <summary>
    ///     When true, the sleep/fatigue clock is suspended: no tiredness escalation, no sleep warnings,
    ///     and no forced sleep. Purely a debug/testing affordance.
    /// </summary>
    bool SleepClockDisabled { get; set; }

    /// <summary>
    ///     When true, the hunger/thirst clock is suspended: no hunger escalation, no hunger warnings,
    ///     and no starvation death. Purely a debug/testing affordance.
    /// </summary>
    bool HungerClockDisabled { get; set; }
}
