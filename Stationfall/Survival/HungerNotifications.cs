namespace Stationfall;

/// <summary>
///     Stationfall's timings for the shared hunger/thirst ladder (see <see cref="HungerNotificationsBase" />
///     for the mechanism). These are the original's own values rather than Planetfall's softened ones:
///     at seven millichrons to the turn they give roughly 190 turns before the first pang and another
///     150 before it kills you, which is not a punishing clock on a map this size.
/// </summary>
public class HungerNotifications : HungerNotificationsBase
{
    /// <summary>
    ///     Millichrons from the start of a stretch of not eating to the first pang (misc.zil:197).
    /// </summary>
    private const int InitialTicks = 1330;

    private const int HungryToRavenousTicks = 450;
    private const int RavenousToFaintTicks = 300;
    private const int FaintToAboutToPassOutTicks = 150;
    private const int AboutToPassOutToDeadTicks = 150;

    /// <summary>
    ///     What a meal buys you (verbs.zil:692).
    /// </summary>
    public const int TicksBoughtByAMeal = 2250;

    /// <summary>
    ///     Millichrons of grace on a new morning, depending on whether you went to sleep hungry
    ///     (globals.zil:1124-1128). Going to bed famished costs you most of the following morning.
    /// </summary>
    public const int FamishedMorningTicks = 200;

    public const int RestedMorningTicks = 400;

    protected override int InitialWarningTicks => InitialTicks;

    // 450 -> 300 -> 150 -> 150 -> death (globals.zil:1201-1214). No WellFed entry: the base only looks
    // this up for a level it is escalating *to*, which is never WellFed.
    protected override IReadOnlyDictionary<HungerLevel, int> Intervals { get; } =
        new Dictionary<HungerLevel, int>
        {
            { HungerLevel.Hungry, HungryToRavenousTicks },
            { HungerLevel.Ravenous, RavenousToFaintTicks },
            { HungerLevel.Faint, FaintToAboutToPassOutTicks },
            { HungerLevel.AboutToPassOut, AboutToPassOutToDeadTicks }
        };
}
