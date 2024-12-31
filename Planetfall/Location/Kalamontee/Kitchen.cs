using GameEngine.Location;
using Model.Movement;
using Planetfall.Item.Kalamontee;
using Planetfall.Item.Kalamontee.Admin;

namespace Planetfall.Location.Kalamontee;

internal class Kitchen : LocationBase
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.N, Go<MessHall>() }
        };

    protected override string ContextBasedDescription =>
        "This is the food production and dispensary area for the dining hall to the north. Of particular interest is a machine near the door. You should probably examine it more closely. " ;

    public override string BeforeEnterLocation(IContext context, ILocation previousLocation)
    {
        context.RemoveActor<KitchenDoor>();
        return base.BeforeEnterLocation(context, previousLocation);
    }

    public override string Name => "Kitchen";

    public override void Init()
    {
        StartWithItem<KitchenDoor>();
        StartWithItem<KitchenMachine>();
    }
}