using GameEngine.Location;
using Model.Interface;
using Model.Movement;

namespace EscapeRoom.Location;

public class Lounge : LocationBase
{
    public override string Name => "Lounge";

    protected override string GetContextBasedDescription(IContext context)
    {
        return "A cozy lounge with a worn couch and a small coffee table. " +
               "A vending machine hums quietly in the corner. " +
               "Exits lead west to the reception area and south through a door marked 'DANGER - DO NOT ENTER'.";
    }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.W, new MovementParameters { Location = GetLocation<Reception>() } },
            { Direction.S, new MovementParameters { Location = GetLocation<MaintenanceShaft>() } }
        };
    }

    public override void Init()
    {
        StartWithItem<Couch>();
    }
}
