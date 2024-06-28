using Model.Movement;

namespace ZorkOne.Location.MazeLocation;

public class DeadEndFour : DeadEndBase
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.S, new MovementParameters { Location = GetLocation<MazeTwelve>() } },
        };
}