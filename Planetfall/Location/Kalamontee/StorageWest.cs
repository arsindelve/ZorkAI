using GameEngine.Location;
using Model.Movement;
using Planetfall.Item.Kalamontee;

namespace Planetfall.Location.Kalamontee;

internal class StorageWest : LocationBase
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.S, Go<MessCorridor>() }
        };

    protected override string GetContextBasedDescription() => "This is a small room obviously intended as a storage area. ";

    public override string Name => "Storage West";

    public override void Init()
    {
        StartWithItem<TinCan>();
        StartWithItem<Ladder>();
    }
}