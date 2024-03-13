namespace ZorkOne.Location;

public class ForestPath : BaseLocation
{
    protected override Dictionary<Direction, MovementParameters> Map => new()
    {
        {
            Direction.S, new MovementParameters { Location = GetLocation<NorthOfHouse>() }
        },
        {
            Direction.N, new MovementParameters { Location = GetLocation<Clearing>() }
        },
        {
            Direction.E, new MovementParameters { Location = GetLocation<ForestTwo>() }
        },
        {
            Direction.Up, new MovementParameters { Location = GetLocation<UpATree>() }
        },
        {
            Direction.W, new MovementParameters { Location = GetLocation<ForestOne>() }
        }
    };

    protected override string Name => "Forest Path";

    protected override string ContextBasedDescription =>
        "This is a path winding through a dimly lit forest. The path heads north-south here. " +
        "One particularly large tree with some low branches stands at the edge of the path.";
    
    public override InteractionResult RespondToSpecificLocationInteraction(string input, IContext context)
    {
        switch (input.ToLowerInvariant().Trim())
        {
            case "climb":
            case "climb up":
            case "climb tree":
            case "climb the tree":
            case "climb up the tree":

                context.CurrentLocation = Repository.GetLocation<UpATree>();
                string message = Repository.GetLocation<UpATree>().Description;
                return new PositiveInteractionResult(message);
        }

        return base.RespondToSpecificLocationInteraction(input, context);
    }
}