using GameEngine.Location;

namespace Planetfall.Location.Kalamontee.Mech;

internal class MechCorridor : LocationWithNoStartingItems
{
    public override string Name => "Mech Corridor";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.N, Go<MechCorridorNorth>() },
            { Direction.W, Go<PhysicalPlant>() },
            { Direction.E, Go<ReactorControl>() },
            { Direction.S, Go<MechCorridorSouth>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "Entrances to rooms lie to the east and west from this north-south hall. ";
    }
}