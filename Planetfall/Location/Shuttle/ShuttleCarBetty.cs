using Model.Movement;

namespace Planetfall.Location.Shuttle;

public class ShuttleCarBetty : ShuttleCabin
{
    public override string Name => "Shuttle Car Betty";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.W, Go<BettyControlWest>() },
            { Direction.E, Go<BettyControlEast>() },
            // TODO: Where we leave depends on shuttle location. 
            { Direction.S, Go<LawandaPlatform>()}
        };
    }

    protected override string Exit => "south";
}