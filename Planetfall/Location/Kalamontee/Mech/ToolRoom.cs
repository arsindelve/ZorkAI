using GameEngine.Location;
using Planetfall.Item.Kalamontee.Mech;

namespace Planetfall.Location.Kalamontee.Mech;

internal class ToolRoom : LocationBase
{
    public override string Name => "Tool Room";

    // No "workshop" alias here: it is shared with two sibling shops off Mech Corridor South and only
    // created a dead 3-way "which one?" (issue #268 review). "toolroom" stays — it is unique to here.
    public override string[] NounsForMatching => ["toolroom"];

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
        StartWithItem<Laser>();
    }
}