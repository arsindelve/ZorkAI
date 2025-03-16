using Model.Movement;

namespace Planetfall.Location.Shuttle;

public class ShuttleCarAlfie : ShuttleCabin
{
    public override string Name => "Shuttle Car Alfie";

    protected override string Exit => "north";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.W, Go<AlfieControlWest>() },
            { Direction.E, Go<AlfieControlEast>() },
            {
                Direction.N,
                Repository.GetLocation<AlfieControlEast>().TunnelPosition == 0
                    ? Go<KalamonteePlatform>()
                    : Go<LawandaPlatform>()
            }
        };
    }
}