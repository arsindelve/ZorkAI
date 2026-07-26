using Model.AIGeneration;
using Planetfall.Item.Kalamontee.Admin;
using Planetfall.Item.Kalamontee.Mech;
using Planetfall.Item.Kalamontee.Mech.FloydPart;

namespace Planetfall.Location.Kalamontee;

internal abstract class ElevatorBase<TDoor, TSlot, TCard> : FloydSpecialInteractionLocation, ITurnBasedActor,
    IFloydSpecialInteractionLocation
    where TDoor : ElevatorDoorBase, IItem, new()
    where TSlot : SlotBase<TCard, TSlot>, IItem, new()
    where TCard : AccessCard, new()
{
    [UsedImplicitly] public bool HasBeenSummoned { get; set; }

    [UsedImplicitly] public bool InLobby { get; set; }

    /// <summary>
    ///     Whether this car's door is open <i>as seen from the Elevator Lobby</i>. The OPENBIT flag is
    ///     shared by both ends of the shaft, and the arrival path leaves it open at whichever end the car
    ///     parked at, so the bare flag cannot say which floor the car is on. Every lobby-side surface -
    ///     the entrance in <see cref="ElevatorLobby" />'s map, the lobby's description, and
    ///     "examine blue/red door" - has to agree on this one predicate or the room contradicts itself
    ///     (issues #456, #450, #505). compone.zil's ELEVATOR-LOBBY-F and ELEVATOR-ENTER-F both test the
    ///     flag alongside *-ELEVATOR-UP for the same reason.
    /// </summary>
    public bool IsOpenAtTheLobby => GetItem<TDoor>().IsOpen && InLobby;

    /// <summary>
    ///     The same question asked from the shaft's other end - the Tower Core for the upper car, the
    ///     Waiting Area for the lower one (compone.zil, OTHER-ELEVATOR-ENTER-F). Without the position
    ///     half, that entrance lets you walk into a car standing at the lobby; stepping back out then
    ///     lands you in the lobby, having ridden nothing, since the car's exit resolves against where
    ///     the car actually is.
    /// </summary>
    public bool IsOpenAtTheFarEnd => GetItem<TDoor>().IsOpen && !InLobby;

    [UsedImplicitly] public int TurnsSinceSummoned { get; set; }

    [UsedImplicitly] public int TurnsSinceEnabled { get; set; }

    [UsedImplicitly] public int TurnsSinceMoving { get; set; }

    // TODO: Floyd bounces into the elevator. "Hey, wait for Floyd!" he yells, smiling broadly.

    [UsedImplicitly] public bool IsEnabled { get; set; }

    protected abstract string Color { get; }

    protected abstract string Size { get; }

    protected abstract string ExitDirection { get; }

    protected abstract string EntranceDirection { get; }

    public override string FloydPrompt => FloydPrompts.Elevator;

    public Task<string> Act(IContext context, IGenerationClient client)
    {
        if (TurnsSinceMoving > 0)
            return ElevatorIsMoving();

        // A pending summon owns the actor slot. The enabled countdown ends by removing the actor, which
        // would cancel the summon silently and strand HasBeenSummoned set forever - every later press
        // would then answer "Patience, patience..." and the car could never be recalled. Let the summon
        // finish first; the countdown resumes once it has.
        if (IsEnabled && !HasBeenSummoned)
            return ElevatorIsEnabled(context);

        if (HasBeenSummoned)
            return ElevatorIsSummoned(context);

        return Task.FromResult(string.Empty);
    }

    private Task<string> ElevatorIsMoving()
    {
        TurnsSinceMoving++;

        if (TurnsSinceMoving == 2)
            return Task.FromResult("Some innocuous Hawaiian music oozes from the elevator's intercom. ");

        if (TurnsSinceMoving == 4)
        {
            // Arrived. 
            InLobby = !InLobby;
            var door = GetItem<TDoor>();
            door.IsOpen = true;
            TurnsSinceMoving = 0;

            // keep the countdown for becoming disabled. Then we will remove the actor.
            TurnsSinceEnabled = 3;
            return Task.FromResult(door.NowOpen(this));
        }

        return Task.FromResult(string.Empty);
    }

    public override Task<InteractionResult> RespondToSpecificLocationInteraction(string? input, IContext context,
        IGenerationClient client)
    {
        if (string.IsNullOrEmpty(input))
            return base.RespondToSpecificLocationInteraction(input, context, client);

        if (input.ToLowerInvariant() == "press up button")
            return RespondToSimpleInteraction(new SimpleIntent { Verb = "press", Noun = "up button" }, context, client,
                null!);

        if (input.ToLowerInvariant() == "press down button")
            return RespondToSimpleInteraction(new SimpleIntent { Verb = "press", Noun = "down button" }, context, client,
                null!);
        
        return base.RespondToSpecificLocationInteraction(input, context, client);
    }

    public override async Task<InteractionResult> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (action.Match(Verbs.PushVerbs, ["button"]))
            return new DisambiguationInteractionResult("Which button do you mean, the Up button or the Down button",
                new Dictionary<string, string>
                {
                    { "up", "up button" },
                    { "down", "down button" },
                    { "up button", "up button" },
                    { "down button", "down button" }
                }, "press the {0}");

        if (action.Match(Verbs.PushVerbs, ["up button", "up", "down button", "down"]))
        {
            if (!IsEnabled)
                return new PositiveInteractionResult("Nothing happens. ");

            // The original gates every travel branch on the car being idle as well as enabled
            // (compone.zil, ELEVATOR-BUTTON-F: each success arm requires ELEVATOR-IN-TRANSIT to be
            // false, otherwise it falls through to "Nothing happens."). Without this, a second press
            // mid-flight re-runs Move(), reprinting the departure message and resetting the trip
            // timer - the player could stall the elevator indefinitely.
            if (TurnsSinceMoving > 0)
                return new PositiveInteractionResult("Nothing happens. ");

            if (action.MatchNoun(["up button", "up"]))
                return GoUp(context);

            if (action.MatchNoun(["down button", "down"]))
                return GoDown(context);
        }

        return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }

    protected InteractionResult Move(IContext context)
    {
        TurnsSinceMoving = 1;
        context.RegisterActor(this);
        var door = GetItem<TDoor>();
        door.IsOpen = false;
        // Let the door own its own message, as the arrival path already does with NowOpen. The
        // literal used to be duplicated here, so editing ElevatorDoorBase.NowClosed changed nothing.
        return new PositiveInteractionResult(door.NowClosed(this));
    }

    protected abstract InteractionResult GoDown(IContext context);

    protected abstract InteractionResult GoUp(IContext context);

    private Task<string> ElevatorIsSummoned(IContext context)
    {
        TurnsSinceSummoned++;

        if (TurnsSinceSummoned != 4)
            return Task.FromResult(string.Empty);

        GetItem<TDoor>().IsOpen = true;
        InLobby = true;

        // The summon is fulfilled, so clear its bookkeeping: HasBeenSummoned means "a call is in
        // flight", which is what the "Patience, patience..." check below wants. Left set, a later
        // summon of the same car short-circuited to "Patience" without ever registering the actor,
        // so the car could never be called back a second time.
        HasBeenSummoned = false;
        TurnsSinceSummoned = 0;

        // Only release the actor slot if nothing else still needs ticking - an activation that was
        // paused for this summon has to be able to run its countdown down afterwards.
        if (!IsEnabled)
            context.RemoveActor(this);

        // The doorway being described is the lobby's, so say so only to a player standing in it, the
        // way ElevatorIsEnabled below guards its own recording.
        return Task.FromResult(context.CurrentLocation is ElevatorLobby
            ? $"\n\nThe door at the {EntranceDirection} end of the room slides open. "
            : string.Empty);
    }

    private Task<string> ElevatorIsEnabled(IContext context)
    {
        TurnsSinceEnabled++;
        if (TurnsSinceEnabled < 6)
            return Task.FromResult(string.Empty);

        IsEnabled = false;
        TurnsSinceEnabled = 0;
        context.RemoveActor(this);
        return Task.FromResult(
            context.CurrentLocation == this ? "A recording says \"Elevator no longer enabled.\"" : "");
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        // Interpolate the door's actual state (issue #450). Hardcoding "open" contradicted the
        // "elevator door slides shut" message from Move() and "examine door" during the ~3 turns
        // the car is in motion with the door closed. Mirrors RecArea / the #438 Mess Hall fix.
        return
            $"This is a {Size} room with a sliding door to the {ExitDirection} which is " +
            $"{(GetItem<TDoor>().IsOpen ? "open" : "closed")}. " +
            $"A control panel contains an Up button, a Down button, and a narrow slot. ";
    }

    public override void Init()
    {
        StartWithItem<TDoor>();
        StartWithItem<TSlot>();
    }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            {
                Direction.Up,
                new MovementParameters
                    { CanGo = _ => false, CustomFailureMessage = "You'll have to use the elevator controls. " }
            },
            {
                Direction.Down,
                new MovementParameters
                    { CanGo = _ => false, CustomFailureMessage = "You'll have to use the elevator controls. " }
            },
            {
                DirectionParser.ParseDirection(ExitDirection), new MovementParameters
                {
                    CustomFailureMessage = "The door is closed. ",
                    CanGo = _ => Repository.GetItem<TDoor>().IsOpen,
                    Location = Exit()
                }
            },
            {
                Direction.Out, new MovementParameters
                {
                    CustomFailureMessage = "The door is closed. ",
                    CanGo = _ => Repository.GetItem<TDoor>().IsOpen,
                    Location = Exit()
                }
            }
        };
    }

    protected abstract ILocation? Exit();

    internal InteractionResult SummonElevator(string response, IContext context)
    {
        // The call button is only reachable from the lobby, so "the door is already open" has to mean
        // open *here* - on the raw shared flag, a car parked at the far end refused the summon that is
        // the player's only way to get it back, in exactly the state the lobby describes as closed
        // (issue #505).
        if (IsOpenAtTheLobby)
            return new PositiveInteractionResult($"Pushing the {Color} button has no effect. ");

        if (HasBeenSummoned)
            return new PositiveInteractionResult("Patience, patience... ");

        context.RegisterActor(this);
        HasBeenSummoned = true;
        return new PositiveInteractionResult(response);
    }
}