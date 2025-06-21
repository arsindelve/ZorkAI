using GameEngine;
using GameEngine.Location;
using Model.AIGeneration;
using Model.Interface;
using Model.Movement;
using ZorkOne.Location.RiverLocation;

namespace ZorkOne.Location;

public class SandyBeach : DarkLocation // Explain to me how a beach can be a dark location? Well, it is. 
{
    public override string Name => "Sandy Beach";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.NE, Go<SandyCave>() },
            { Direction.S, Go<Shore>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "You are on a large sandy beach on the east shore of the river, which is flowing quickly by. " +
               "A path runs beside the river to the south here, and a passage is partially buried in sand to the northeast. ";
    }
    
    public override async Task<InteractionResult> RespondToSpecificLocationInteraction(string? input, IContext context,
        IGenerationClient client)
    {
        if (string.IsNullOrEmpty(input))
            return await base.RespondToSpecificLocationInteraction(input, context, client);

        var preppedInput = input.ToLowerInvariant().Trim();

        if (SubLocation is null)
            return await base.RespondToSpecificLocationInteraction(input, context, client);

        if (preppedInput.StartsWith("launch"))
        {
            // TODO: move some of this logic to the move engine, but only after tests are passing. 
            context.CurrentLocation.SubLocation = null;
            context.CurrentLocation = Repository.GetLocation<FrigidRiverFour>();
            await context.CurrentLocation.AfterEnterLocation(context, this, null!);
            context.CurrentLocation.SubLocation = Repository.GetItem<PileOfPlastic>();
            ((ICanContainItems)context.CurrentLocation).ItemPlacedHere(Repository.GetItem<PileOfPlastic>());

            return new PositiveInteractionResult(context.CurrentLocation.GetDescription(context));
        }

        return await base.RespondToSpecificLocationInteraction(input, context, client);
    }

    public override void Init()
    {
        StartWithItem<Shovel>();
    }
}