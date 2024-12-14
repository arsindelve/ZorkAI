using GameEngine.Location;
using Model.AIGeneration;
using Model.Movement;

namespace Planetfall.Location.Kalamontee.Admin;

public class AdminCorridorSouth : LocationWithNoStartingItems
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.S, Go<CorridorJunction>() },
            { Direction.N, Go<AdminCorridor>() },
            { Direction.E, Go<SanfacE>() }
        };

    protected override string ContextBasedDescription =>
        "This section of hallway seems to have suffered some minor structural damage. The walls are cracked, and " +
        "a jagged crevice crosses the floor. An opening leads east and the corridor heads north and south. ";

    public override string Name => "Admin Corridor South";
}