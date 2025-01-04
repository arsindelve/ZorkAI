using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location.MazeLocation;

public class DeadEndFour : DeadEndBase
{
    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.S, new MovementParameters { Location = GetLocation<MazeTwelve>() } }
        };
    }
}