using GameEngine.Location;

namespace Planetfall.Location.Kalamontee.Dorm;

internal class DormC : LocationWithNoStartingItems
{
    public override string Name => "Dorm C";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.N, Go<DormCorridor>() },
            { Direction.S, Go<SanfacC>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "This is a very long room lined with multi-tiered bunks. Flimsy partitions between the tiers may have " +
               "provided a modicum of privacy. These spartan living quarters could have once housed many hundreds, but it " +
               "seems quite deserted now. There are openings at the north and south ends of the room.";
    }
}