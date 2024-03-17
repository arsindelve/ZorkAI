namespace ZorkOne.Location;

public class EastOfChasm : BaseLocation
{
    public override string Name => "East of Chasm";

    protected override string ContextBasedDescription =>
        "You are on the east edge of a chasm, the bottom of which cannot be seen. A narrow passage goes north, " +
        "and the path you are on continues to the east. ";

    public override void Init()
    {
    }

    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.N, new MovementParameters { Location = GetLocation<Cellar>() } },
            { Direction.E, new MovementParameters { Location = GetLocation<Gallery>() } }
        };
}