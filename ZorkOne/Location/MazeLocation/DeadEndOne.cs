using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location.MazeLocation;

public class DeadEndOne : DeadEndBase
{
    protected override Dictionary<Direction, MovementParameters> Map(IContext context) =>
        new()
        {
            { Direction.S, new MovementParameters { Location = GetLocation<MazeFour>() } },
        };
}