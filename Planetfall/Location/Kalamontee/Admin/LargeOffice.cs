using GameEngine.Location;
using Model.Movement;
using Planetfall.Item.Kalamontee.Admin;

namespace Planetfall.Location.Kalamontee.Admin;

internal class LargeOffice : LocationBase
{
    public override string Name => "Large Office";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.E, Go<SmallOffice>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "This is a large, plush office. The far wall is one large picture window, scratched but unbroken, " +
               "offering a view of this installation and the ocean beyond. In front of the window is a wide wooden desk. " +
               "The only exit is east. ";
    }

    public override void Init()
    {
        StartWithItem<LargeDesk>();
    }
}