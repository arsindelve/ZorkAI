namespace ZorkOne.Location;

public class SouthOfHouse : BaseLocation
{
    protected override string ContextBasedDescription =>
        "You are facing the south side of a white house. There is no door here, and all the windows are boarded.";

    protected override string Name => "South of House";

    protected override Dictionary<Direction, MovementParameters> Map => new()
    {
        {
            Direction.W, new MovementParameters { Location = GetLocation<WestOfHouse>() }
        },
        {
            Direction.E, new MovementParameters { Location = GetLocation<BehindHouse>() }
        },
        {
            Direction.N,
            new MovementParameters
            {
                CanGo = _ => false,
                CustomFailureMessage = "The windows are all boarded."
            }
        }
    };
}