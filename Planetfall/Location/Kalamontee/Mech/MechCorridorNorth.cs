using GameEngine.Location;
using Model.Movement;

namespace Planetfall.Location.Kalamontee.Mech;

internal class MechCorridorNorth : LocationWithNoStartingItems
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.N, Go<CorridorJunction>() },
            { Direction.W, Go<PhysicalPlant>() },
            { Direction.S, Go<MechCorridor>() }
        };

    protected override string GetContextBasedDescription() =>
        "Entrances to rooms lie to the east and west from this north-south hall. ";

    public override string Name => "Mech Corridor North";
}