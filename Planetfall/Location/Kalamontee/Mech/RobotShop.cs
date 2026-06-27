using GameEngine.Location;
using Planetfall.Item.Kalamontee.Mech;
using Planetfall.Item.Kalamontee.Mech.FloydPart;

namespace Planetfall.Location.Kalamontee.Mech;

internal class RobotShop : LocationBase
{
    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new()
        {
            { Direction.NW, Go<MechCorridorSouth>() },
            { Direction.W, Go<MachineShop>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "This room, with exits west and northwest, is filled with robot-like devices of every conceivable " +
            "description, all in various states of disassembly. ";
    }

    protected override IReadOnlyList<SceneryItem> Scenery =>
    [
        new(["device", "devices", "robot-like devices", "robotlike devices", "disassembled robots"],
            "They're the remains of disassembled robots, far beyond any repair. ",
            "It's a heap of broken robot parts, not worth carrying off. ")
    ];

    public override string Name => "Robot Shop";

    public override void Init()
    {
        StartWithItem<Floyd>();
    }
}