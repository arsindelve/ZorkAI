using GameEngine.Location;
using Model.Movement;

namespace Planetfall.Location.Kalamontee.Mech;

internal class MechCorridor : LocationWithNoStartingItems
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.N, Go<MechCorridorNorth>() },
            { Direction.W, Go<PhysicalPlant>() },
            { Direction.E, Go<ReactorControl>() },
            { Direction.S, Go<MechCorridorSouth>() }
        };

    protected override string ContextBasedDescription =>
        "Entrances to rooms lie to the east and west from this north-south hall. ";

    public override string Name => "Mech Corridor";
}