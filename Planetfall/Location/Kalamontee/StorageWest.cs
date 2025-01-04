using GameEngine.Location;
using Model.Movement;
using Planetfall.Item.Kalamontee;

namespace Planetfall.Location.Kalamontee;

internal class StorageWest : LocationBase
{
    public override string Name => "Storage West";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.S, Go<MessCorridor>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "This is a small room obviously intended as a storage area. ";
    }

    protected override void OnFirstTimeEnterLocation(IContext context)
    {
        context.AddPoints(4);
        base.OnFirstTimeEnterLocation(context);
    }

    public override void Init()
    {
        StartWithItem<TinCan>();
        StartWithItem<Ladder>();
    }
}