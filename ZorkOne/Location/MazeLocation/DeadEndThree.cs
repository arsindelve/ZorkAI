using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location.MazeLocation;

public class DeadEndThree : DeadEndBase
{
    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.N, new MovementParameters { Location = GetLocation<MazeEight>() } }
        };
    }
}