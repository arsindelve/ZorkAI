using GameEngine.Location;
using Model.Interface;
using Model.Movement;

namespace EscapeRoom.Location;

public class Reception : LocationBase
{
    public override string Name => "Reception";

    protected override string GetContextBasedDescription(IContext context)
    {
        return "You are in a small reception area. A welcome desk sits in the center of the room. " +
               "A sign on the wall reads 'Adventurer Training Academy - Escape Room Tutorial'. " +
               "Exits lead north to a storage closet, east to a lounge, west to an office, and south to an exit hallway.";
    }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.N, new MovementParameters { Location = GetLocation<StorageCloset>() } },
            { Direction.S, new MovementParameters { Location = GetLocation<ExitHallway>() } },
            { Direction.E, new MovementParameters { Location = GetLocation<Lounge>() } },
            { Direction.W, new MovementParameters { Location = GetLocation<Office>() } }
        };
    }

    public override void Init()
    {
        StartWithItem<WelcomeDesk>();
    }
}
