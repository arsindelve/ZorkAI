using GameEngine.Location;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location;

public class StreamView : DarkLocation
{
    public override string Name => "Stream View";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.E, new MovementParameters { Location = GetLocation<ReservoirSouth>() } },
            {
                // ZIL STREAM-VIEW: WEST is blocked — the stream emerges from a gap too small to enter.
                Direction.W,
                new MovementParameters
                {
                    CanGo = _ => false,
                    CustomFailureMessage = "The stream emerges from a spot too small for you to enter. "
                }
            }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "You are standing on a path beside a gently flowing stream. The path follows the stream, " +
               "which flows from west to east. ";
    }

    public override void Init()
    {
    }
}
