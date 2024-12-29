using GameEngine.Location;
using Model.Movement;
using Planetfall.Item.Mech;

namespace Planetfall.Location.Kalamontee.Mech;

internal class RobotShop : LocationBase
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.NW, Go<MechCorridorSouth>() },
            { Direction.W, Go<MachineShop>() }
        };

    protected override string ContextBasedDescription =>
        "This room, with exits west and northwest, is filled with robot-like devices of every conceivable " +
        "description, all in various states of disassembly. ";

    public override string Name => "Robot Shop";

    public override void Init()
    {
        StartWithItem<Floyd>();
    }
}