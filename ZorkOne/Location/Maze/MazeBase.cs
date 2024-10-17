using Model.Movement;

namespace ZorkOne.Location.Maze;

public abstract class MazeBase : DarkLocationWithNoStartingItems
{
    protected override string ContextBasedDescription =>
        "This is part of a maze of twisty little passages, all alike. ";

    public override string Name => "Maze";
}

public class MazeOne : MazeBase
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.E, new MovementParameters { Location = GetLocation<TrollRoom>() } },
            { Direction.N, new MovementParameters { Location = GetLocation<MazeOne>() } },
            { Direction.S, new MovementParameters { Location = GetLocation<MazeTwo>() } }
        };
}

public class MazeTwo : MazeBase
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.E, new MovementParameters { Location = GetLocation<MazeThree>() } },
            { Direction.Down, new MovementParameters { Location = GetLocation<MazeFour>() } }
        };
}

public class MazeThree : MazeBase
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.W, new MovementParameters { Location = GetLocation<MazeTwo>() } },
            { Direction.Up, new MovementParameters { Location = GetLocation<MazeFive>() } }
        };
}

public class MazeFour : MazeBase
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.W, new MovementParameters { Location = GetLocation<MazeThree>() } },
            { Direction.N, new MovementParameters { Location = GetLocation<MazeOne>() } },
            { Direction.E, new MovementParameters { Location = GetLocation<DeadEndOne>() } }
        };
}

public class MazeFive : MazeBase
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.N, new MovementParameters { Location = GetLocation<MazeThree>() } },
            { Direction.E, new MovementParameters { Location = GetLocation<DeadEndTwo>() } },
        };
}