namespace ZorkOne.Location;

public class Reservoir : DarkLocation
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.S, new MovementParameters { Location = GetLocation<ReservoirSouth>() } }
        };

    protected override string ContextBasedDescription =>
        "You are on what used to be a large lake, but which is now a large mud pile. There are \"shores\" to the north and south.";

    public override string Name => "Reservoir";

    public override void Init()
    {
    }
}