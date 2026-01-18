using GameEngine.Location;
using Model.Interface;
using Model.Movement;

namespace EscapeRoom.Location;

public class ExitHallway : LocationBase
{
    public override string Name => "Exit Hallway";

    protected override string GetContextBasedDescription(IContext context)
    {
        return "A short hallway ending at a heavy metal door. " +
               "A sign above the door reads 'EXIT'. The door has a prominent lock. " +
               "The only way back is north to the reception area.";
    }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.N, new MovementParameters { Location = GetLocation<Reception>() } }
        };
    }

    public override void Init()
    {
        StartWithItem<ExitDoor>();
    }
}
