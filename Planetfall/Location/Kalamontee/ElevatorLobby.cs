using GameEngine.Location;
using Model;
using Model.AIGeneration;
using Model.Movement;
using Planetfall.Item.Kalamontee.Admin;

namespace Planetfall.Location.Kalamontee;

internal class ElevatorLobby : LocationBase
{
    public override string Name => "Elevator Lobby";

    public override void Init()
    {
        StartWithItem<LowerElevatorDoor>();
        StartWithItem<UpperElevatorDoor>();
    }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.W, Go<CorridorJunction>() },
            {
                Direction.S,
                new MovementParameters
                {
                    CanGo = _ => GetItem<LowerElevatorDoor>().IsOpen, CustomFailureMessage = "The door is closed.",
                    Location = GetLocation<LowerElevator>()
                }
            },
            {
                Direction.N,
                new MovementParameters
                {
                    CanGo = _ => GetItem<UpperElevatorDoor>().IsOpen, CustomFailureMessage = "The door is closed.",
                    Location = GetLocation<UpperElevator>()
                }
            }
        };
    }

    public override async Task<InteractionResult> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (action.Match(Verbs.PushVerbs, ["button", "elevator button"]))
            return new DisambiguationInteractionResult("Which button do you mean, the red button or the blue button",
                new Dictionary<string, string>
                {
                    { "blue", "blue button" },
                    { "red", "red button" },
                    { "blue elevator", "blue button" },
                    { "red elevator", "red button" },
                    { "red elevator button", "red button" },
                    { "blue elevator button", "blue button" },
                    { "blue button", "blue button" },
                    { "red button", "red button" }
                }, "press the {0} button");


        if (action.Match(Verbs.PushVerbs, ["red button", "red elevator", "red elevator button", "red"]))
            return GetLocation<LowerElevator>().SummonElevator(" The red door begins vibrating a bit. ", context);

        if (action.Match(Verbs.PushVerbs, ["blue button", "blue elevator", "blue elevator button"]))
            return GetLocation<UpperElevator>()
                .SummonElevator("You hear a faint whirring noise from behind the blue door. ", context);

        return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            $"This is a wide, brightly lit lobby. A blue metal door to the north is {(GetItem<UpperElevatorDoor>().IsOpen ? "open" : "closed")} and a larger red metal " +
            $"door to the south is {(GetItem<LowerElevatorDoor>().IsOpen == GetItem<UpperElevatorDoor>().IsOpen ? "also " : "")}{(GetItem<LowerElevatorDoor>().IsOpen ? "open" : "closed")}. " +
            $"Beside the blue door is a blue button, and beside the red door is a red button. A corridor leads west. To the east is a small room about the size of a telephone booth. ";
    }
}