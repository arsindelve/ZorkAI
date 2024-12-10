using GameEngine;
using GameEngine.Location;
using Model.AIGeneration;
using Model.Interface;
using Model.Movement;
using ZorkOne.Location.RiverLocation;

namespace ZorkOne.Location;

public class DamBase : LocationBase
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            {
                Direction.N,
                new MovementParameters
                {
                    // TODO: This needs to be implemented globally, for every location. 
                    Location = GetLocation<Dam>(), CanGo = _ => SubLocation == null,
                    CustomFailureMessage = "You can't go there in a magic boat. "
                }
            },
            {
                Direction.Up,
                new MovementParameters
                {
                    // TODO: This needs to be implemented globally, for every location. 
                    Location = GetLocation<Dam>(), CanGo = _ => SubLocation == null,
                    CustomFailureMessage = "You can't go there in a magic boat. "
                }
            }
        };

    protected override string ContextBasedDescription =>
        "You are at the base of Flood Control Dam #3, which looms above you and to the north. The river " +
        "Frigid is flowing by here. Along the river are the White Cliffs which seem to form giant walls stretching" +
        " from north to south along the shores of the river as it winds its way downstream. ";

    public override string Name => "Dam Base";

    public override async Task<InteractionResult> RespondToSpecificLocationInteraction(string? input, IContext context, IGenerationClient client)
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
            context.CurrentLocation = Repository.GetLocation<FrigidRiverOne>();
            await context.CurrentLocation.AfterEnterLocation(context, this, null!);
            context.CurrentLocation.SubLocation = Repository.GetItem<PileOfPlastic>();
            ((ICanHoldItems)context.CurrentLocation).ItemPlacedHere(Repository.GetItem<PileOfPlastic>());

            return new PositiveInteractionResult(context.CurrentLocation.Description);
        }

        return await base.RespondToSpecificLocationInteraction(input, context, client);
    }

    public override void Init()
    {
        StartWithItem<PileOfPlastic>();
    }
}