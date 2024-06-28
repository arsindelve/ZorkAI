using Model.Movement;

namespace ZorkOne.Location.Maze;

public class DeadEndOne : DeadEndBase
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.S, new MovementParameters { Location = GetLocation<MazeFour>() } },
        };
}