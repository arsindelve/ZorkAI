namespace ZorkOne.Location;

public class ForestThree : BaseLocation
{
    protected override Dictionary<Direction, MovementParameters> Map => new()
    {
        {
            Direction.NW, new MovementParameters { Location = GetLocation<SouthOfHouse>() }
        },
        {
            Direction.N, new MovementParameters { Location = GetLocation<ClearingBehindHouse>() }
        },
        {
            Direction.W, new MovementParameters { Location = GetLocation<ForestOne>() }
        },
        {
            Direction.S, new MovementParameters { CanGo = _ => false, CustomFailureMessage = "Storm-tossed trees block your way. "}
        },
        {
            Direction.E, new MovementParameters { CanGo = _ => false, CustomFailureMessage = "The rank undergrowth prevents eastward movement. "}
        }
    };

    protected override string Name => "Forest";

    protected override string ContextBasedDescription => "This is a dimly lit forest, with trees all around. ";
}