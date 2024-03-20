namespace ZorkOne.Location;

public class Temple : LocationWithNoStartingItems
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.N, new MovementParameters { Location = GetLocation<TorchRoom>() } },
            { Direction.E, new MovementParameters { Location = GetLocation<EgyptianRoom>() } },
            { Direction.Down, new MovementParameters { Location = GetLocation<EgyptianRoom>() } },
            { Direction.S, new MovementParameters { Location = GetLocation<Altar>() } }
        };

    public override string Name => "Temple";

    protected override string ContextBasedDescription =>
        "This is the north end of a large temple. On the east wall is an ancient inscription, " +
        "probably a prayer in a long-forgotten language. Below the prayer is a staircase leading down. " +
        "The west wall is solid granite. The exit to the north end of the room is through huge marble pillars.";


}