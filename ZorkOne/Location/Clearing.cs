namespace ZorkOne.Location;

public class Clearing : BaseLocation
{
    protected override Dictionary<Direction, MovementParameters> Map => new()
    {
        {
            Direction.S, new MovementParameters { Location = GetLocation<ForestPath>() }
        },
        {
            Direction.E, new MovementParameters { Location = GetLocation<ForestTwo>() }
        },
        {
            Direction.W, new MovementParameters { Location = GetLocation<ForestOne>() }
        },
        {
            Direction.N,
            new MovementParameters
                { CanGo = _ => false, CustomFailureMessage = "The forest becomes impenetrable to the north." }
        }
    };

    public override string Name => "Clearing";

    protected override string ContextBasedDescription =>
        "You are in a clearing, with a forest surrounding you on all sides. A path leads south.";

    // TODO: Count the leaves: There are 69,105 leaves here
    // TODO: > jump in leaves: Wheeeeeeeeee!!!!!

    public override void Init()
    {
    }
}