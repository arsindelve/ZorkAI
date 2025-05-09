using GameEngine.Location;
using Model.AIGeneration;
using Planetfall.Location.Lawanda.Lab;

namespace Planetfall.Location.Lawanda;

internal class ComputerRoom : LocationBase
{
    public override string Name => "Computer Room";

    public override void Init()
    {
        StartWithItem<ComputerOutput>();
    }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.N, Go<ProjectCorridorEast>() },
            { Direction.NE, Go<MainLab>() },
            { Direction.S, Go<MiniaturizationBooth>() }
        };
    }

    public override async Task<InteractionResult> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (action.Match(["look at", "examine"], ["light", "red light"]))
            // TODO: Update when the computer is fixed. 
            return new PositiveInteractionResult(
                "The red light would seem to indicate a malfunction in the computer. ");

        return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This is the main computer room for the Project. The only sign of activity is a glowing red light. " +
            "The exits are north, west, and northeast. To the south is a small booth. ";
    }
}