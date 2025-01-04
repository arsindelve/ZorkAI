using GameEngine.Location;
using Model.Movement;

namespace Planetfall.Location.Kalamontee.Mech;

internal class PhysicalPlant : LocationWithNoStartingItems
{
    public override string Name => "Physical Plant";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.NE, Go<MechCorridorNorth>() },
            { Direction.SE, Go<MechCorridor>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This is a huge, dim room with exits in the northeast and southeast corners. The room is criss-crossed with " +
            "catwalks and is filled with heavy equipment presumably intended to heat and ventilate this complex. Hardly " +
            "any of the equipment is still operating.";
    }
}