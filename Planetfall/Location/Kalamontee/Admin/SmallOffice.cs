using GameEngine.Location;
using Model.Movement;
using Planetfall.Item.Kalamontee.Admin;

namespace Planetfall.Location.Kalamontee.Admin;

internal class SmallOffice : LocationBase
{
    public override string Name => "Small Office";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.E, Go<AdminCorridorNorth>() },
            { Direction.W, Go<LargeOffice>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "You have entered a small office of some sort. A small desk faces the main doorway which lies to the " +
               "east. Another exit leads west. ";
    }

    public override void Init()
    {
        StartWithItem<SmallDesk>();
    }
}