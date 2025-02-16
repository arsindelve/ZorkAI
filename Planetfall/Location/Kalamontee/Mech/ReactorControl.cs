using GameEngine.Location;
using Model.AIGeneration;
using Model.Movement;

namespace Planetfall.Location.Kalamontee.Mech;

internal class ReactorControl : LocationWithNoStartingItems
{
    public override string Name => "Reactor Control";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.W, Go<MechCorridor>() },
            { Direction.Down, Go<ReactorAccessStairs>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This room contains many dials and gauges for controlling a massive planetary power reactor which, according " +
            "to a diagram on the wall, must be buried far below this very complex. The exit is to the west. To the east " +
            "is a metal door, and next to it, a button. A dark stairway winds downward. ";
    }

    public override InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (action.Match(["push", "press", "activate"], ["button"]))
            return new PositiveInteractionResult("Nothing happens. ");

        return base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }
}