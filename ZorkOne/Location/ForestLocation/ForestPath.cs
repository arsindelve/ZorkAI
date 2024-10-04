using GameEngine.Location;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location.ForestLocation;

public class ForestPath : LocationWithNoStartingItems
{
    // TODO: You hear in the distance the chirping of a song bird.
    
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

    public override string Name => "Forest Path";

    protected override string ContextBasedDescription =>
        "This is a path winding through a dimly lit forest. The path heads north-south here. " +
        "One particularly large tree with some low branches stands at the edge of the path. ";

    public override InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client)
    {
        string[] nouns = ["tree", "branches"];
        string[] verbs = ["examine", "look"];

        if (action.Match(verbs, nouns))
            return new PositiveInteractionResult("There's nothing special about the tree.");

        return base.RespondToSimpleInteraction(action, context, client);
    }
}