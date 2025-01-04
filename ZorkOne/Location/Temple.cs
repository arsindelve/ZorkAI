using GameEngine.Location;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location;

// Strangely, this is not a dark location. 
public class Temple : LocationBase
{
    public override string Name => "Temple";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.N, new MovementParameters { Location = GetLocation<TorchRoom>() } },
            { Direction.E, new MovementParameters { Location = GetLocation<EgyptianRoom>() } },
            { Direction.Down, new MovementParameters { Location = GetLocation<EgyptianRoom>() } },
            { Direction.S, new MovementParameters { Location = GetLocation<Altar>() } }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "This is the north end of a large temple. On the east wall is an ancient inscription, " +
               "probably a prayer in a long-forgotten language. Below the prayer is a staircase leading down. " +
               "The west wall is solid granite. The exit to the north end of the room is through huge marble pillars. ";
    }

    public override void Init()
    {
        StartWithItem<BrassBell>();
    }
}