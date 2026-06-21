using GameEngine.Location;
using Model.AIGeneration;
using Planetfall.Item.Kalamontee.Mech;

namespace Planetfall.Location.Kalamontee.Mech;

internal class ReactorControl : LocationWithNoStartingItems
{
    public override string Name => "Reactor Control";

    private ReactorElevatorDoor Door => Repository.GetItem<ReactorElevatorDoor>();

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.W, Go<MechCorridor>() },
            { Direction.Down, Go<ReactorAccessStairs>() },
            {
                Direction.E, new MovementParameters
                {
                    CanGo = _ => Door.IsOpen,
                    Location = GetLocation<ReactorElevator>(),
                    CustomFailureMessage = "The door is closed. "
                }
            },
            {
                Direction.In, new MovementParameters
                {
                    CanGo = _ => Door.IsOpen,
                    Location = GetLocation<ReactorElevator>(),
                    CustomFailureMessage = "The door is closed. "
                }
            }
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