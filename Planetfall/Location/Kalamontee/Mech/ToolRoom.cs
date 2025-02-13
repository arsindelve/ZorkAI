using GameEngine.Location;
using Model.Movement;
using Planetfall.Item.Kalamontee.Mech;

namespace Planetfall.Location.Kalamontee.Mech;

internal class ToolRoom : LocationBase
{
    public override string Name => "Tool Room";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.NE, Go<MechCorridorSouth>() },
            { Direction.E, Go<MachineShop>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "This is apparently a storage room for tools. Exits lead northeast and east. ";
    }

    public override void Init()
    {
        StartWithItem<Flask>();
        StartWithItem<Pliers>();
        StartWithItem<Magnet>();
    }
}