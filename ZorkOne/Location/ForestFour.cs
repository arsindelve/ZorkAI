namespace ZorkOne.Location;

public class ForestFour : BaseLocation
{
    protected override Dictionary<Direction, MovementParameters> Map => new()
    {
        {
            Direction.W, new MovementParameters { Location = GetLocation<ForestTwo>() }
        },
        {
            Direction.N, new MovementParameters { Location = GetLocation<ForestTwo>() }
        },
        {
            Direction.S, new MovementParameters { Location = GetLocation<ForestTwo>() }
        },
        {
            Direction.E, new MovementParameters { CanGo = _ => false, CustomFailureMessage = "The mountains are impassable. "}
        }
    };

    protected override string Name => "Forest";

    protected override string ContextBasedDescription => "The forest thins out, revealing impassible mountains. ";
}