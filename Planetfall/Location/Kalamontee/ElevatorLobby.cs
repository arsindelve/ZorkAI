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
    private Doorway BlueDoor =>
        new(GetItem<UpperElevatorDoor>(), () => GetLocation<UpperElevator>().IsOpenAtTheLobby);

    private Doorway RedDoor =>
        new(GetItem<LowerElevatorDoor>(), () => GetLocation<LowerElevator>().IsOpenAtTheLobby);

    public override void Init()
    {
        StartWithItem<LowerElevatorDoor>();
        StartWithItem<UpperElevatorDoor>();
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
            // That two-part test lives in the Doorway, so the exit and the description share it.
            { Direction.S, RedDoor.Passage(GetLocation<LowerElevator>(), "The door is closed.") },
            { Direction.N, BlueDoor.Passage(GetLocation<UpperElevator>(), "The door is closed.") }
        };
    }

    public override async Task<InteractionResult> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        // The door object reports the shaft-wide flag, which is only correct from inside the car, so
        // every verb that reads a door's state has to be answered here instead - otherwise "examine red
        // door" and "open red door" corroborate a state the room text and the exit both deny (#505).
        // The bare nouns ("door", "elevator door") never reach this: both doors match them, so
        // SimpleInteractionEngine disambiguates first.
        var doorAnswer = BlueDoor.Answer(action, context) ?? RedDoor.Answer(action, context);
        if (doorAnswer is not null)
            return doorAnswer;

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