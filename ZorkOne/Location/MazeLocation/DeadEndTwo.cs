using Model.Movement;

namespace ZorkOne.Location.MazeLocation;

public class DeadEndTwo : DeadEndBase
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.W, new MovementParameters { Location = GetLocation<MazeFive>() } },
        };
}