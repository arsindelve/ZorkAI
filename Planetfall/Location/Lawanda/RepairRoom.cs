using GameEngine.Location;
using Planetfall.Item.Lawanda;
using Planetfall.SimpleInteraction;

namespace Planetfall.Location.Lawanda;

internal class RepairRoom : LocationBase, ISimpleInteractionHandler
{
    public override string Name => "Repair Room";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.S, Go<SystemsCorridorWest>() },
            { Direction.Up, Go<SystemsCorridorWest>() },
            { Direction.N, new MovementParameters { CanGo = _ => false, CustomFailureMessage = "It is a robot-sized doorway -- a bit too small for you. " } }
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
    
    public bool HandleSimpleInteraction(IContext context, string verb, string noun)
    {
        if (verb == "examine" && (noun == "cabinets" || noun == "cabinet" || noun == "storage cabinets" || noun == "storage cabinet"))
        {
            context.WriteLine("The cabinets are locked. ");
            return true;
        }
        
        return false;
    }
}
