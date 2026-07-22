using GameEngine.Location;
using Model.AIGeneration;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Planetfall.Item.Lawanda.Lab;
using Planetfall.Location.Lawanda.LabOffice;

namespace Planetfall.Location.Lawanda.Lab;

internal class BioLockEast : LocationBase, ITurnBasedActor, IFloydDoesNotTalkHere
{
    public override string Name => "Bio Lock East";

    public BioLockStateMachineManager StateMachine { get; } = new();

    public override void Init()
    {
        StartWithItem<BioLockInnerDoor>();
    }

    public override async Task<InteractionResult> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        // Issue #423: on prod the AI parser does not reliably resolve "look through window" to
        // verb ∈ LookVerbs + noun "window" — the "through" disrupts verb/noun extraction, so the
        // through-branch (the exact phrasing the handler was written to catch, and the walkthrough's own)
        // fired only intermittently and silently degraded to the room description. The parser-level #423
        // change alone can't cure that variance, so gate the view on the raw input (a look/examine verb +
        // "through" + "window"). The second branch keeps examine / look at / look into window working.
        if (action.MatchInInput(Verbs.LookVerbs.Concat(Verbs.ExamineVerbs).ToArray(), "through", ["window"]) ||
            action.Match(Verbs.ExamineVerbs, ["window"]))
            return new PositiveInteractionResult(
                "You can see a large laboratory, dimly illuminated. A blue glow comes from a crack in the northern wall of the lab. Shadowy, " +
                "ominous shapes move about within the room. On the floor, just inside the door, you can see a magnetic-striped card. ");

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
            Repository.GetItem<Floyd>().IsAwayOnScriptedSequence = false;

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