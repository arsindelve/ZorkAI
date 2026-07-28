using GameEngine.Location;
using Model.AIGeneration;
using Planetfall.Item.Kalamontee.Mech;

namespace Planetfall.Location.Kalamontee.Mech;

internal class ReactorControl : LocationWithNoStartingItems
{
    public override string Name => "Reactor Control";

    // The far side of the Reactor Elevator's door. Nothing in this room's description claims a state for
    // it, so there is nothing here to drift - but the exits still gate on it, via the same Doorway.
    private Doorway Door => new(Repository.GetItem<ReactorElevatorDoor>());

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        var intoTheCar = Door.Passage(GetLocation<ReactorElevator>());

        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.W, Go<MechCorridor>() },
            { Direction.Down, Go<ReactorAccessStairs>() },
            { Direction.E, intoTheCar },
            { Direction.In, intoTheCar }
        };
    }

    public override void Init()
    {
        StartWithItem<ReactorElevatorDoor>();
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This room contains many dials and gauges for controlling a massive planetary power reactor which, according " +
            "to a diagram on the wall, must be buried far below this very complex. The exit is to the west. To the east " +
            "is a metal door, and next to it, a button. A dark stairway winds downward. ";
    }

    public override async Task<InteractionResult> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (action.Match(Verbs.PushVerbs, ["button"]))
            return new PositiveInteractionResult("Nothing happens. ");

        return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }
}