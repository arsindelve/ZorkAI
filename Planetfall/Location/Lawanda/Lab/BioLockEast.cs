using GameEngine.Location;
using Model.AIGeneration;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Planetfall.Item.Lawanda.Lab;
using Planetfall.Location.Lawanda.LabOffice;

namespace Planetfall.Location.Lawanda.Lab;

internal class BioLockEast : LocationBase, ITurnBasedActor, IFloydDoesNotTalkHere
{
    public override string Name => "Bio Lock East";

    // Issue #493: this MUST have a setter. The deployed game is stateless — each turn is a separate
    // request whose full state travels in the base64 session — and the session serializer deliberately
    // drops read-only properties (GameEngine.DoNotSerializeReadOnlyPropertiesResolver). As a get-only
    // property, the entire Bio-Lab sacrifice state machine was never written to the session and reset to
    // a fresh new() every turn, so Floyd never offered the plan and opening the inner door was always an
    // instant "wrong time" death — making the mini card (and thus the game) unreachable in production.
    // A setter lets it round-trip like every other writable state object (cf. the #472 disambiguation fix).
    public BioLockStateMachineManager StateMachine { get; set; } = new();

    public override void Init()
    {
        StartWithItem<BioLockInnerDoor>();
    }

    public override async Task<InteractionResult> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (
            (action.Match(new[] { "examine" }
                .Concat(Verbs.LookVerbs)
                .ToArray(), ["window"]) && action.OriginalInput != null && action.OriginalInput.Contains("through")) ||
            action.Match(Verbs.ExamineVerbs, ["window"]))
        {
            var view =
                "You can see a large laboratory, dimly illuminated. A blue glow comes from a crack in the northern wall of the lab. Shadowy, " +
                "ominous shapes move about within the room. ";

            // Issue #477: the window looks into the lab, so only describe the card on the lab floor while
            // it is actually still in the lab, mirroring the ZIL WINDOW-F guard on MINI-CARD's TOUCHBIT.
            // The card has no real location until it leaves the lab, by either path: Floyd's sacrifice
            // delivers it to this room (EndSequence -> ItemPlacedHere, which sets CurrentLocation but NOT
            // HasEverBeenPickedUp), and a direct/god-mode take sets HasEverBeenPickedUp. Keying only on
            // HasEverBeenPickedUp would leave the post-sacrifice window where the card sits at the player's
            // feet yet the view still claimed it was inside the lab. Note we must not set the card's
            // picked-up flag at delivery: that flag gates its first-pickup point award (Context.cs).
            var card = Repository.GetItem<MiniaturizationAccessCard>();
            var cardStillInLab = card.CurrentLocation is null && !card.HasEverBeenPickedUp;
            if (cardStillInLab)
                view += "On the floor, just inside the door, you can see a magnetic-striped card. ";

            return new PositiveInteractionResult(view);
        }

        return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.W, Go<BioLockWest>() },
            {
                Direction.E,
                new MovementParameters
                {
                    CanGo = _ => GetItem<BioLockInnerDoor>().IsOpen,
                    CustomFailureMessage = "The door to the Bio Lab is closed. ",
                    Location = GetLocation<BioLabLocation>()
                }
            }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This is the second half of the sterilization chamber leading from the main lab to the Bio Lab. The door " +
            "to the east, leading to the Bio Lab, has a window. The bio lock continues to the west. ";
    }

    public override void OnLeaveLocation(IContext context, ILocation newLocation, ILocation previousLocation)
    {
        // Issue #365 follow-up: if the player abandons the Bio Lab sequence mid-flight (walks away
        // instead of closing the door), this actor stops running and never gets another turn to progress
        // the state machine. Without releasing Floyd here, IsAwayOnScriptedSequence would stay true
        // forever with no other code path to clear it, permanently blocking his normal following behavior.
        if (StateMachine.IsFloydInLabFighting)
        {
            Repository.GetItem<Floyd>().IsAwayOnScriptedSequence = false;

            // Issue #493 review: releasing Floyd is not enough. The state machine is still frozen mid-
            // sequence (e.g. FloydRushedInNeedToCloseDoor). Left as-is, walking back into Bio Lock East
            // re-registers this actor and resumes that state, firing a spurious "you failed to close the
            // door" death even though Floyd followed the player out. Reset the sequence so a later
            // re-entry starts clean (Floyd, who still remembers he volunteered, simply re-offers).
            StateMachine.ResetSequence();

            // Also close the inner door Floyd rushed through. Resetting the state machine alone would
            // leave an anomalous "NotStarted but door open" state: on re-entry Floyd nags "open the door"
            // while it is already open, and re-opening an open door is a no-op (OpenMe short-circuits to
            // "It is already open." without calling OnOpening), so the obvious restart does nothing.
            // Closing it restores the genuine pre-sequence state, so "open door" cleanly restarts the rush.
            GetItem<BioLockInnerDoor>().IsOpen = false;
        }

        context.RemoveActor(this);
    }

    public override string BeforeEnterLocation(IContext context, ILocation previousLocation)
    {
        context.RegisterActor(this);
        return base.BeforeEnterLocation(context, previousLocation);
    }

    public Task<string> Act(IContext context, IGenerationClient client)
    {
        var floyd = Repository.GetItem<Floyd>();
        var computerRoom = Repository.GetLocation<ComputerRoom>();

        var result = StateMachine.HandleTurnAction(
            floyd.IsHereAndIsOn(context),
            computerRoom.FloydHasExpressedConcern,
            context,
            floyd);

        return Task.FromResult(result);
    }

    
}