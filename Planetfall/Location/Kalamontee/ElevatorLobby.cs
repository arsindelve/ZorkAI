using GameEngine.Location;
using Model.AIGeneration;
using Planetfall.Item.Kalamontee.Admin;

namespace Planetfall.Location.Kalamontee;

internal class ElevatorLobby : LocationBase
{
    public override string Name => "Elevator Lobby";

    // Every surface in this room - the two entrances, the description, and "examine <colour> door" -
    // must answer from the effective state, never from the shared OPENBIT flag on its own. See
    // ElevatorBase.IsOpenAtTheLobby for why, and issue #505 for what happens when they disagree. Stating
    // each door once as a Doorway is what makes "must" enforceable rather than a convention.
    //
    // These are the lobby's OWN door objects. The rooms at the two ends of each shaft used to share one,
    // which meant its CurrentLocation could only ever be true for one of them - so the lobby's doors
    // fell out of scope and "enter blue door" found nothing to enter (issue #532). A landing door also
    // reports the effective state as its own IsOpen, which is why nothing here needs a predicate.
    private Doorway BlueDoor => new(GetItem<UpperElevatorLobbyDoor>());

    private Doorway RedDoor => new(GetItem<LowerElevatorLobbyDoor>());

    public override void Init()
    {
        StartWithItem<LowerElevatorLobbyDoor>();
        StartWithItem<UpperElevatorLobbyDoor>();
    }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.E, Go<BoothTwo>() },
            { Direction.W, Go<CorridorJunction>() },
            // Both entrances need the car to be standing at the lobby as well as the door being open
            // (compone.zil, ELEVATOR-ENTER-F tests *-ELEVATOR-UP alongside OPENBIT). The door flag is
            // shared by both ends of the shaft, so on its own it cannot say which floor the car is on.
            // That two-part test is the lobby door's own IsOpen, so every surface here shares it.
            { Direction.S, RedDoor.Passage(GetLocation<LowerElevator>(), "The door is closed.") },
            { Direction.N, BlueDoor.Passage(GetLocation<UpperElevator>(), "The door is closed.") }
        };
    }

    public override async Task<InteractionResult> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        // No door interception here any more. It existed because the door object this room reached
        // reported the shaft-wide flag, which is only correct from inside the car - so "examine red door"
        // and "open red door" corroborated a state the room text and the exit both denied (#505). The
        // lobby now has its own door objects whose IsOpen *is* the lobby's answer, so the ordinary
        // examine / open / close processors are correct again and the room says nothing twice (#532).

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
            return GetLocation<LowerElevator>().SummonElevator("The red door begins vibrating a bit. ", context);

        if (action.Match(Verbs.PushVerbs, ["blue button", "blue elevator", "blue elevator button"]))
            return GetLocation<UpperElevator>()
                .SummonElevator("You hear a faint whirring noise from behind the blue door. ", context);

        return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        // The "also" clause means "both doors are in the same state", so it has to compare the effective
        // states too - on the raw flags it could say "also" when they differ and omit it when they match
        // (issue #505). Both Doorways answer effectively, so comparing them is enough.
        var blueOpen = BlueDoor.IsOpen;
        var redOpen = RedDoor.IsOpen;

        return
            $"This is a wide, brightly lit lobby. A blue metal door to the north is {BlueDoor.StateWord} and a larger red metal " +
            $"door to the south is {(redOpen == blueOpen ? "also " : "")}{RedDoor.StateWord}. " +
            "Beside the blue door is a blue button, and beside the red door is a red button. A corridor leads west. To the east is a small room about the size of a telephone booth. ";
    }
}