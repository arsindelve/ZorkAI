using GameEngine.Location;
using Planetfall.Item.Kalamontee;
using Planetfall.Item.Kalamontee.Admin;

namespace Planetfall.Location.Kalamontee;

internal class Kitchen : LocationBase
{
    public override string Name => "Kitchen";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.N, Go<MessHall>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This is the food production and dispensary area for the dining hall to the north. Of particular interest is a machine near the door. You should probably examine it more closely. ";
    }

    public override string BeforeEnterLocation(IContext context, ILocation previousLocation)
    {
        context.RemoveActor<KitchenDoor>();
        return base.BeforeEnterLocation(context, previousLocation);
    }

    public override void Init()
    {
        StartWithItem<KitchenDoor>();
        StartWithItem<KitchenMachine>();
    }

    protected override void OnFirstTimeEnterLocation(IContext context)
    {
        context.AddPoints(4);
    }
}