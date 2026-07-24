namespace Stationfall.Location.Duffy;

/// <summary>
///     The junction between Deck Twelve, the Robot Pool, and the cargo bay (ship.zil:221-230).
///     Also where picking Rex proves fatal — he follows you in and forgets to stop (ship.zil:880-885).
///     That death is resolved in <see cref="Rex" /> as he arrives, so it can't depend on the order two
///     actors happen to run in.
/// </summary>
public class CargoBayEntrance : LocationBase
{
    public override string Name => "Cargo Bay Entrance";

    public override string[] NounsForMatching => ["cargo bay entrance", "entrance"];

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.E, Go<CargoBay>() },
            { Direction.W, Go<DeckTwelve>() },
            { Direction.N, Go<RobotPool>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "The Deck Twelve corridor ends here at the entrance to the cargo bay, to starboard. A " +
               "smaller entrance leads fore. ";
    }

    public override void Init()
    {
    }
}
