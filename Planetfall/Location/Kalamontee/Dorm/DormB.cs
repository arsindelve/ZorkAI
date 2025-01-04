using GameEngine.Location;
using Model.Movement;

namespace Planetfall.Location.Kalamontee.Dorm;

internal class DormB : LocationWithNoStartingItems
{
    public override string Name => "Dorm B";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.S, Go<RecCorridor>() },
            { Direction.N, Go<SanfacB>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "This is a very long room lined with multi-tiered bunks. Flimsy partitions between the tiers may have " +
               "provided a modicum of privacy. These spartan living quarters could have once housed many hundreds, but it " +
               "seems quite deserted now. There are openings at the north and south ends of the room.";
    }
}