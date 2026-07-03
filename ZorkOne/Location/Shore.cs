using GameEngine.Location;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location;

public class Shore : LocationWithNoStartingItems
{
    public override string Name => "Shore";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.N, Go<SandyBeach>() },
            { Direction.S, Go<AragainFalls>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "You are on the east shore of the river. The water here seems somewhat treacherous. A path travels from north " +
            "to south here, the south end quickly turning around a sharp corner. ";
    }

    protected override IReadOnlyList<SceneryItem> Scenery => [RiverBankGlobalObjects.River];

    public override Task<InteractionResult> RespondToSpecificLocationInteraction(string? input, IContext context,
        IGenerationClient client)
    {
        return RiverBankGlobalObjects.IsSwimAttempt(input)
            ? Task.FromResult<InteractionResult>(new PositiveInteractionResult(RiverBankGlobalObjects.SwimmingMessage))
            : base.RespondToSpecificLocationInteraction(input, context, client);
    }

    public override async Task<InteractionResult> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        var waterResult = await RiverBankGlobalObjects.RespondToWater(action, context, client, itemProcessorFactory);
        if (waterResult is not null)
            return waterResult;

        return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }
}