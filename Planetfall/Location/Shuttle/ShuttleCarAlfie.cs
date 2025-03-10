using Model.Movement;

namespace Planetfall.Location.Shuttle;

public class ShuttleCarAlfie : ShuttleCabin
{
    public override string Name => "Shuttle Car Alfie";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.W, Go<AlfieControlWest>() },
            { Direction.E, Go<AlfieControlEast>() },
            // TODO: Where we leave depends on shuttle location. 
            { Direction.N, Go<KalamonteePlatform>()}
        };
    }

    protected override string Exit => "north";
}