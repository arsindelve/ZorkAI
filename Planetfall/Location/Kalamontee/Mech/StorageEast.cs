using GameEngine.Location;
using Planetfall.Item.Kalamontee.Mech;

namespace Planetfall.Location.Kalamontee.Mech;

internal class StorageEast : LocationBase
{
    public override string Name => "Storage East";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.W, Go<MechCorridorNorth>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "A small room for storage. The exit is to the west. ";
    }

    public override void Init()
    {
        StartWithItem<OilCan>();
    }
}
