using GameEngine.Location;
using Model.Movement;
using Planetfall.Item.Kalamontee;

namespace Planetfall.Location.Kalamontee;

internal class StorageWest : LocationBase
{
    protected override Dictionary<Direction, MovementParameters> Map(IContext context) =>
        new()
        {
            { Direction.S, Go<MessCorridor>() }
        };

    protected override string GetContextBasedDescription(IContext context) => "This is a small room obviously intended as a storage area. ";

    public override string Name => "Storage West";

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