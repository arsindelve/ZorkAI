using GameEngine.Location;
using Model.Movement;
using Planetfall.Item.Mech;

namespace Planetfall.Location.Kalamontee.Mech;

internal class ToolRoom : LocationBase
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.NE, Go<MechCorridorSouth>() },
            { Direction.E, Go<MachineShop>() }
        };

    protected override string ContextBasedDescription =>
        "This is apparently a storage room for tools. Exits lead northeast and east. ";

    public override string Name => "Tool Room";

    public override void Init()
    {
        StartWithItem<Flask>();
        StartWithItem<Pliers>();
    }
}