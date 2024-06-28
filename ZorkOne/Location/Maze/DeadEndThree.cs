using Model.Movement;

namespace ZorkOne.Location.Maze;

public class DeadEndThree : DeadEndBase
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.N, new MovementParameters { Location = GetLocation<MazeEight>() } },
        };
}