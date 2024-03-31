namespace ZorkOne.Location;

public class OnTheRainbow : LocationWithNoStartingItems
{
    protected override Dictionary<Direction, MovementParameters> Map => new()
    {
        {
            Direction.W, new MovementParameters { Location = GetLocation<EndOfRainbow>() }
        }
    };

    protected override string ContextBasedDescription =>
        "You are on top of a rainbow (I bet you never thought you would walk on a rainbow), " +
        "with a magnificent view of the Falls. The rainbow travels east-west here.";

    public override string Name => "On The Rainbow";

    // TODO: Wave the sceptre while standing on the rainbow.
}