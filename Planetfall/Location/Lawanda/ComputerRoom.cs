using GameEngine.Location;
using Model.AIGeneration;
using Model.Movement;

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

    public override InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client)
    {
        if (action.Match(["look at", "examine"], ["light", "red light"]))
            // TODO: Update when the computer is fixed. 
            return new PositiveInteractionResult(
                "The red light would seem to indicate a malfunction in the computer. ");

        return base.RespondToSimpleInteraction(action, context, client);
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This is the main computer room for the Project. The only sign of activity is a glowing red light. " +
            "The exits are north, west, and northeast. To the south is a small booth. ";
    }
}