using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location.MazeLocation;

public class DeadEndTwo : DeadEndBase
{
    protected override Dictionary<Direction, MovementParameters> Map(IContext context) =>
        new()
        {
            { Direction.W, new MovementParameters { Location = GetLocation<MazeFive>() } },
        };
}