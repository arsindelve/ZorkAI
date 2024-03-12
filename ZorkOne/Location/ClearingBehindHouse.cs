namespace ZorkOne.Location;

public class ClearingBehindHouse : BaseLocation
{
    protected override string ContextBasedDescription =>
        "You are in a small clearing in a well marked forest path that extends to the east and west.";

    protected override string Name => "Clearing";

    protected override Dictionary<Direction, MovementParameters> Map => new()
    {
        {
            Direction.W, new MovementParameters { Location = GetLocation<BehindHouse>() }
        },
        {
            Direction.E, new MovementParameters { Location = GetLocation<CanyonView>() }
        }
    };
}