using GameEngine.Location;
using Planetfall.Item.Lawanda;

namespace Planetfall.Location.Lawanda;

internal class RepairRoom : LocationBase
{
    public override string Name => "Repair Room";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.S, Go<SystemsCorridorWest>() },
            { Direction.Up, Go<SystemsCorridorWest>() },
            { Direction.N, new MovementParameters { FailureMessage = "It is a robot-sized doorway -- a bit too small for you. " } }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "You are in a dimly lit room, filled with strange machines and wide storage cabinets, all locked. To the south, a narrow " +
            "stairway leads upward. On the north wall of the room is a very small doorway. ";
    }
    
    public override void Init()
    {
        StartWithItem<BrokenRobot>();
    }
}
