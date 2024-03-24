namespace ZorkOne.Location;

public class ReservoirSouth : DarkLocation
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.SW, new MovementParameters { Location = GetLocation<Chasm>() } },
            { Direction.SE, new MovementParameters { Location = GetLocation<DeepCanyon>() } },
            { Direction.E, new MovementParameters { Location = GetLocation<Dam>() } },
          
        };

    protected override string ContextBasedDescription =>
        "You are in a long room on the south shore of a large lake, far too deep and wide for crossing.\n" +
        "There is a path along the stream to the east or west, a steep pathway climbing southwest along the edge " +
        "of a chasm, and a path leading into a canyon to the southeast.";

    public override string Name => "Reservoir South\n";

    public override void Init()
    {
    }
}