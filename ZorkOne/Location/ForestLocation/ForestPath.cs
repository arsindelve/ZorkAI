using GameEngine.Location;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location.ForestLocation;

public class ForestPath : LocationWithNoStartingItems, ITurnBasedActor
{
    public override string Name => "Forest Path";

    public Task<string> Act(IContext context, IGenerationClient client)
    {
        if (context.CurrentLocation is not ForestPath)
        {
            context.RemoveActor(this);
            return Task.FromResult(string.Empty);
        }

        var random = new Random();
        var randomNumber = random.Next(0, 4);
        if (randomNumber == 0) return Task.FromResult("\nIn the distance you hear the chirping of a song bird. ");

        return Task.FromResult(string.Empty);
    }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
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
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "This is a path winding through a dimly lit forest. The path heads north-south here. " +
               "One particularly large tree with some low branches stands at the edge of the path. ";
    }

    public override Task<string> AfterEnterLocation(IContext context, ILocation previousLocation,
        IGenerationClient generationClient)
    {
        var random = new Random();
        var randomNumber = random.Next(0, 5);
        if (randomNumber == 0) return Task.FromResult("\nIn the distance you hear the chirping of a song bird. ");

        return base.AfterEnterLocation(context, previousLocation, generationClient);
    }

    public override InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client)
    {
        string[] nouns = ["tree", "branches"];
        string[] verbs = ["examine", "look"];

        if (action.Match(verbs, nouns))
            return new PositiveInteractionResult("There's nothing special about the tree.");

        return base.RespondToSimpleInteraction(action, context, client);
    }

    public override string BeforeEnterLocation(IContext context, ILocation previousLocation)
    {
        context.RegisterActor(this);
        return base.BeforeEnterLocation(context, previousLocation);
    }

    public override void OnLeaveLocation(IContext context, ILocation newLocation, ILocation previousLocation)
    {
        context.RemoveActor(this);
        base.OnLeaveLocation(context, newLocation, previousLocation);
    }
}