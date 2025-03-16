using Model.Movement;

namespace Planetfall.Location.Shuttle;

public class ShuttleCarBetty : ShuttleCabin
{
    public override string Name => "Shuttle Car Betty";

    protected override string Exit => "south";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.W, Go<BettyControlWest>() },
            { Direction.E, Go<BettyControlEast>() },
            {
                Direction.N,
                Repository.GetLocation<AlfieControlEast>().TunnelPosition == 0
                    ? Go<LawandaPlatform>()
                    : Go<KalamonteePlatform>()
            }
        };
    }
}