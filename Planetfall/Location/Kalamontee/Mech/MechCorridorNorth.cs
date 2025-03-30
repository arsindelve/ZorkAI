using GameEngine.Location;

namespace Planetfall.Location.Kalamontee.Mech;

internal class MechCorridorNorth : LocationWithNoStartingItems
{
    public override string Name => "Mech Corridor North";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.N, Go<CorridorJunction>() },
            { Direction.W, Go<PhysicalPlant>() },
            { Direction.S, Go<MechCorridor>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "Entrances to rooms lie to the east and west from this north-south hall. ";
    }
}