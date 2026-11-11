namespace Stationfall;

/// <summary>
///     Stationfall's timings for the shared fatigue ladder (see <see cref="SleepNotificationsBase" /> for
///     the mechanism).
/// </summary>
public class SleepNotifications : SleepNotificationsBase
{
    /// <summary>
    ///     The first day is scheduled at an absolute time of day, not an offset: the original queues the
    ///     first warning for <c>8100 - start time</c> (misc.zil:196), which is the same thing as saying
    ///     it lands at Galactic Standard Time 8100 however early or late the game happened to begin.
    ///     Since the starting time is randomised, this is what makes day one's stamina vary between
    ///     games while bedtime stays put.
    /// </summary>
    public const int FirstWarningAtTime = 8100;

    /// <summary>
    ///     Millichrons of waking time each later day allows before fatigue first bites
    ///     (globals.zil:1075). Unlike Planetfall, the days here do not get progressively shorter.
    /// </summary>
    private const int LaterDayTicks = 5900;

    /// <summary>
    ///     Millichrons between climbing into a bed while tired and actually dropping off
    ///     (globals.zil:853). Longer than the drift-off that follows a fatigue warning, which is the
    ///     base class's default.
    ///     Unused until somewhere implements <see cref="IAmABed" />; the action that puts the player
    ///     into that bed is what should pass this to
    ///     <see cref="SleepNotificationsBase.QueueFallAsleep" />. See the remarks on
    ///     <see cref="IAmABed" /> for why passing it explicitly matters.
    /// </summary>
    public const int TicksToDriftOffInBed = 22;

    private const int TiredToVeryTiredTicks = 320;
    private const int VeryTiredToExhaustedTicks = 160;
    private const int ExhaustedToAboutToDropTicks = 80;
    private const int AboutToDropToCollapseTicks = 40;

    protected override IReadOnlyDictionary<TiredLevel, int> Intervals { get; } =
        new Dictionary<TiredLevel, int>
        {
            { TiredLevel.Tired, TiredToVeryTiredTicks },
            { TiredLevel.VeryTired, VeryTiredToExhaustedTicks },
            { TiredLevel.Exhausted, ExhaustedToAboutToDropTicks },
            { TiredLevel.AboutToDrop, AboutToDropToCollapseTicks }
        };

    protected override int InitialWarningTicksForDay(int day)
    {
        return LaterDayTicks;
    }

    /// <summary>
    ///     Day one alone is pinned to a wall-clock time rather than an elapsed count - see
    ///     <see cref="FirstWarningAtTime" />.
    /// </summary>
    public override void Initialize(int currentTime)
    {
        NextWarningAt = FirstWarningAtTime;
    }
}
