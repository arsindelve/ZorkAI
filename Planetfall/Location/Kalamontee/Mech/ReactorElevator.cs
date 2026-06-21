using GameEngine.Location;
using Model.AIGeneration;
using Planetfall.Item.Kalamontee.Mech;

namespace Planetfall.Location.Kalamontee.Mech;

/// <summary>
///     A flavor dead-end elevator off Reactor Control (compone.zil, REACTOR-ELEVATOR). Its movement
///     table is all zeros in the original, so the Up and Down buttons travel nowhere. The only way
///     out is back west to Reactor Control.
/// </summary>
internal class ReactorElevator : LocationWithNoStartingItems
{
    public override string Name => "Reactor Elevator";

    private ReactorElevatorDoor Door => Repository.GetItem<ReactorElevatorDoor>();

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            {
                Direction.W, new MovementParameters
                {
                    CanGo = _ => Door.IsOpen,
                    Location = GetLocation<ReactorControl>(),
                    CustomFailureMessage = "The door is closed. "
                }
            },
            {
                Direction.Out, new MovementParameters
                {
                    CanGo = _ => Door.IsOpen,
                    Location = GetLocation<ReactorControl>(),
                    CustomFailureMessage = "The door is closed. "
                }
            }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This is an elevator with a door to the west, currently open. A control panel contains " +
            "an Up button, a Down button, and a small slot. ";
    }

    public override void Init()
    {
        StartWithItem<ReactorElevatorDoor>();
    }

    public override async Task<InteractionResult> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        // The movement table is all zeros: the buttons don't take you anywhere.
        if (action.Match(Verbs.PushVerbs, ["up button", "up", "down button", "down"]))
            return new PositiveInteractionResult("Nothing happens. ");

        if (action.Match(Verbs.PushVerbs, ["button", "control panel", "panel", "controls"]))
            return new PositiveInteractionResult(
                "You must specify whether you want to push the Up button or the Down button. ");

        if (action.Match(Verbs.ExamineVerbs, ["slot", "small slot"]))
            return new PositiveInteractionResult("It's a small slot, set into the control panel. ");

        if (action.Match(Verbs.ExamineVerbs, ["control panel", "panel", "controls"]))
            return new PositiveInteractionResult(
                "The control panel contains an Up button, a Down button, and a small slot. ");

        if (action.Match(Verbs.ExamineVerbs, ["up button", "down button", "button", "buttons"]))
            return new PositiveInteractionResult("The buttons are unremarkable. ");

        return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }
}
