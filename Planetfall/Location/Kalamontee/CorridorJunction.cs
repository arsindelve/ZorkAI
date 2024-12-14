using GameEngine.Location;
using Model.Movement;
using Planetfall.Location.Kalamontee.Admin;
using Planetfall.Location.Kalamontee.Mech;

namespace Planetfall.Location.Kalamontee;

internal class CorridorJunction : LocationWithNoStartingItems
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.W, Go<DormCorridor>() },
            { Direction.S, Go<MechCorridorNorth>() },
            { Direction.N, Go<AdminCorridorSouth>() }
        };

    protected override string ContextBasedDescription =>
        "A north-south corridor intersects the main corridor here. To the west, the main corridor extends as far as " +
        "you can see; a nonworking walkway from that direction ends here. To the east, the corridor widens into a well-lit area. ";

    public override string Name => "Corridor Junction";
}