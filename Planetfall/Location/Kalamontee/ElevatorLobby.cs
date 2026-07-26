using GameEngine.Location;
using Model.AIGeneration;
using Planetfall.Item.Kalamontee.Admin;

namespace Planetfall.Location.Kalamontee;

internal class ElevatorLobby : LocationBase
{
    public override string Name => "Elevator Lobby";

    // Every surface in this room - the two entrances, the description, and "examine <colour> door" -
    // must answer from the effective state, never from the shared OPENBIT flag on its own. See
    // ElevatorBase.IsOpenAtTheLobby for why, and issue #505 for what happens when they disagree.
    private bool BlueDoorIsOpen => GetLocation<UpperElevator>().IsOpenAtTheLobby;

    private bool RedDoorIsOpen => GetLocation<LowerElevator>().IsOpenAtTheLobby;

    // ExamineInteractionProcessor answers a wider set than Verbs.ExamineVerbs lists - "check", "look in"
    // and "peek at" appear only in its own switch. Matching just the array would let "check blue door"
    // reach the door item and report the shared flag, reopening the contradiction for that one phrasing.
    private static readonly string[] ExamineDoorVerbs =
        [..Verbs.ExamineVerbs, "check", "look in", "peek at"];

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
            {
                Direction.S,
                new MovementParameters
                {
                    CanGo = _ => RedDoorIsOpen,
                    CustomFailureMessage = "The door is closed.",
                    Location = GetLocation<LowerElevator>()
                }
            },
            {
                Direction.N,
                new MovementParameters
                {
                    CanGo = _ => BlueDoorIsOpen,
                    CustomFailureMessage = "The door is closed.",
                    Location = GetLocation<UpperElevator>()
                }
            }
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
        if (action.MatchNounAndAdjective(GetItem<UpperElevatorDoor>().NounsForMatching))
        {
            var answer = AnswerForDoor(action, GetItem<UpperElevatorDoor>(), BlueDoorIsOpen, context);
            if (answer is not null)
                return answer;
        }

        if (action.MatchNounAndAdjective(GetItem<LowerElevatorDoor>().NounsForMatching))
        {
            var answer = AnswerForDoor(action, GetItem<LowerElevatorDoor>(), RedDoorIsOpen, context);
            if (answer is not null)
                return answer;
        }

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

    /// <summary>
    ///     Re-states what OpenAndCloseInteractionProcessor and the examine processor would say for this
    ///     door, but keyed on <paramref name="isOpenHere" /> instead of the shared flag. The messages stay
    ///     owned by the door, and nothing is lost by bypassing the processors: an elevator door always
    ///     refuses to be opened or closed by hand, so those paths never mutate state for it. Returns null
    ///     for any other verb, which then falls through to normal handling.
    /// </summary>
    private InteractionResult? AnswerForDoor(SimpleIntent action, ElevatorDoorBase door, bool isOpenHere,
        IContext context)
    {
        if (action.MatchVerb(ExamineDoorVerbs))
            return new PositiveInteractionResult(door.DescribeAs(isOpenHere));

        if (action.MatchVerb(Verbs.OpenVerbs))
            return new PositiveInteractionResult(isOpenHere ? door.AlreadyOpen : door.CannotBeOpenedDescription(context));

        if (action.MatchVerb(Verbs.CloseVerbs))
            return new PositiveInteractionResult(
                isOpenHere ? door.CannotBeClosedDescription(context) : door.AlreadyClosed);

        return null;
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        // Read the effective state, not the raw flag: the lobby used to announce a door as open while
        // walking that way answered "The door is closed." The "also" clause means "both doors are in the
        // same state", so it has to compare the effective states too - on the raw flags it could say
        // "also" when they differ and omit it when they match (issue #505).
        var blueOpen = BlueDoorIsOpen;
        var redOpen = RedDoorIsOpen;

        return
            $"This is a wide, brightly lit lobby. A blue metal door to the north is {(blueOpen ? "open" : "closed")} and a larger red metal " +
            $"door to the south is {(redOpen == blueOpen ? "also " : "")}{(redOpen ? "open" : "closed")}. " +
            $"Beside the blue door is a blue button, and beside the red door is a red button. A corridor leads west. To the east is a small room about the size of a telephone booth. ";
    }
}