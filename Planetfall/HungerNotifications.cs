namespace Planetfall;

/// <summary>
///     Planetfall's timings for the shared hunger/thirst ladder (see <see cref="HungerNotificationsBase" />
///     for the mechanism). The intervals are deliberately double the original Infocom values, which made
///     for a punishing clock on a map this size.
/// </summary>
public class HungerNotifications : HungerNotificationsBase
{
    /// <summary>
    ///     Ticks before the first warning of a stretch of not eating.
    /// </summary>
    private const int InitialTicks = 4000;

    private const int HungryToRavenousTicks = 900;
    private const int RavenousToFaintTicks = 300;
    private const int FaintToAboutToPassOutTicks = 200;
    private const int AboutToPassOutToDeadTicks = 100;

    protected override int InitialWarningTicks => InitialTicks;

    // (initial: 4000) -> 900 -> 300 -> 200 -> 100 -> death. There is deliberately no WellFed entry:
    // the base only ever looks this up for a level it is escalating *to*, which is never WellFed.
    protected override IReadOnlyDictionary<HungerLevel, int> Intervals { get; } =
        new Dictionary<HungerLevel, int>
        {
            { HungerLevel.Hungry, HungryToRavenousTicks },
            { HungerLevel.Ravenous, RavenousToFaintTicks },
            { HungerLevel.Faint, FaintToAboutToPassOutTicks },
            { HungerLevel.AboutToPassOut, AboutToPassOutToDeadTicks }
        };
}
