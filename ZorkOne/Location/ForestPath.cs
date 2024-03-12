namespace ZorkOne.Location;

public class ForestPath : BaseLocation
{
    protected override Dictionary<Direction, MovementParameters> Map => new()
    {
        {
            Direction.S, new MovementParameters { Location = GetLocation<NorthOfHouse>() }
        },
        {
            Direction.N, new MovementParameters { Location = GetLocation<Clearing>() }
        },
        {
            Direction.Up, new MovementParameters { Location = GetLocation<UpATree>() }
        }
    };

    protected override string Name => "Forest Path";

    protected override string ContextBasedDescription =>
        "This is a path winding through a dimly lit forest. The path heads north-south here. " +
        "One particularly large tree with some low branches stands at the edge of the path.";
}