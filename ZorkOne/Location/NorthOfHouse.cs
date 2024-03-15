namespace ZorkOne.Location;

public class NorthOfHouse : BaseLocation
{
    protected override string ContextBasedDescription =>
        "You are facing the north side of a white house. There is no door here, " +
        "and all the windows are boarded up. To the north a narrow path winds through the trees.";

    public override string Name => "North Of House";

    protected override Dictionary<Direction, MovementParameters> Map => new()
    {
        {
            Direction.E, new MovementParameters { Location = GetLocation<BehindHouse>() }
        },
        {
            Direction.W, new MovementParameters { Location = GetLocation<WestOfHouse>() }
        },
        {
            Direction.N, new MovementParameters { Location = GetLocation<ForestPath>() }
        },

        {
            Direction.S,
            new MovementParameters { CanGo = _ => false, CustomFailureMessage = "The windows are all boarded" }
        }
    };
}