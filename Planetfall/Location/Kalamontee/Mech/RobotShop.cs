using GameEngine.Location;
using Model.Movement;

namespace Planetfall.Location.Kalamontee.Mech;

internal class RobotShop : LocationBase
{
    protected override Dictionary<Direction, MovementParameters> Map(IContext context) =>
        new()
        {
            { Direction.NW, Go<MechCorridorSouth>() },
            { Direction.W, Go<MachineShop>() }
        };

    protected override string GetContextBasedDescription(IContext context) =>
        "This room, with exits west and northwest, is filled with robot-like devices of every conceivable " +
        "description, all in various states of disassembly. ";

    public override string Name => "Robot Shop";
    public override void Init()
    {

    }
}