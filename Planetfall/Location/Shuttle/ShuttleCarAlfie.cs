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
            { Direction.N, Go<KalamonteePlatform>()}
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This is the cabin of a large transport, with seating for around 20 people plus space for freight. There " +
            "are open doors at the eastern and western ends of the cabin, and a doorway leads out to a wide platform to the north.        ";
    }
}