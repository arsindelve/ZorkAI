namespace Planetfall;

/// <summary>
///     Planetfall's timings for the shared fatigue ladder (see <see cref="SleepNotificationsBase" /> for
///     the mechanism). Each day allows less waking time than the last, so the disease's progress is felt
///     as mounting time pressure rather than only as a message.
/// </summary>
public class SleepNotifications : SleepNotificationsBase
{
    private const int TiredToVeryTiredTicks = 400;
    private const int VeryTiredToExhaustedTicks = 135;
    private const int ExhaustedToAboutToDropTicks = 60;
    private const int AboutToDropToForcedSleepTicks = 50;

    /// <summary>
    ///     The last day the table defines. Day 9 is death, so the calendar never outruns it in practice.
    /// </summary>
    private const int LastDefinedDay = 8;

    protected override IReadOnlyDictionary<TiredLevel, int> Intervals { get; } =
        new Dictionary<TiredLevel, int>
        {
            { TiredLevel.Tired, TiredToVeryTiredTicks },
            { TiredLevel.VeryTired, VeryTiredToExhaustedTicks },
            { TiredLevel.Exhausted, ExhaustedToAboutToDropTicks },
            { TiredLevel.AboutToDrop, AboutToDropToForcedSleepTicks }
        };

    /// <summary>
    ///     Ticks of waking time each day before fatigue first bites, shortening as the illness advances.
    /// </summary>
    private static readonly Dictionary<int, int> DailyInitialWarningTimes = new()
    {
        { 1, 3600 },
        { 2, 5800 },
        { 3, 5550 },
        { 4, 5200 },
        { 5, 4800 },
        { 6, 4300 },
        { 7, 3700 },
        { 8, 3000 }
    };

    protected override int InitialWarningTicksForDay(int day)
    {
        return DailyInitialWarningTimes.TryGetValue(day, out var ticks)
            ? ticks
            : DailyInitialWarningTimes[LastDefinedDay];
    }
}
