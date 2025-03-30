using Model.AIGeneration;
using Planetfall.Item.Kalamontee.Mech;

namespace Planetfall.Location.Kalamontee.Tower;

public class Helicopter : FloydSpecialInteractionLocation
{
    public override string Name => "Helicopter";

    public override string FloydPrompt => FloydPrompts.Helicopter;

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.Out, Go<Helipad>() }
        };
    }

    public override void Init()
    {
    }

    public override async Task<InteractionResult> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (action.Match(["examine"], ["controls", "control panel"]))
            return new PositiveInteractionResult("The controls are covered and locked.");

        return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This is a large vehicle with a lot of cargo space. A complex control panel is closed " +
            "and locked. Everything is covered with a thick layer of rust. Through the windows of " +
            "the vehicle you can see a wide Helipad, and beyond that, endless ocean far below. " +
            "Several doors lead out to the Helipad. ";
    }
}