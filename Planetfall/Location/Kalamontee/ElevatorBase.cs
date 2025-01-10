using GameEngine.Location;
using Model.AIGeneration;
using Model.Movement;
using Planetfall.Item.Kalamontee.Admin;

namespace Planetfall.Location.Kalamontee;

internal abstract class ElevatorBase<TDoor, TSlot, TCard> : LocationBase, ITurnBasedActor
    where TDoor : ElevatorDoorBase, IItem, new()
    where TSlot : SlotBase<TCard, TSlot>, IItem, new()
    where TCard : AccessCard, new()
{
    [UsedImplicitly] public bool HasBeenSummoned { get; set; }

    [UsedImplicitly] public bool InLobby { get; set; }

    [UsedImplicitly] public int TurnsSinceSummoned { get; set; }

    [UsedImplicitly] public int TurnsSinceEnabled { get; set; }

    [UsedImplicitly] public int TurnsSinceMoving { get; set; }

    // TODO: Floyd bounces into the elevator. "Hey, wait for Floyd!" he yells, smiling broadly.

    [UsedImplicitly] public bool IsEnabled { get; set; }

    protected abstract string Color { get; }

    protected abstract string Size { get; }

    protected abstract string ExitDirection { get; }

    protected abstract string EntranceDirection { get; }

    public Task<string> Act(IContext context, IGenerationClient client)
    {
        if (TurnsSinceMoving > 0 && context.CurrentLocation == this)
            return ElevatorIsMoving(context);

        if (IsEnabled && context.CurrentLocation == this)
            return ElevatorIsEnabled(context);

        if (HasBeenSummoned && context.CurrentLocation != this)
            return ElevatorIsSummoned(context);

        return Task.FromResult(string.Empty);
    }

    private Task<string> ElevatorIsMoving(IContext context)
    {  
        TurnsSinceMoving++;
        
        if (TurnsSinceMoving == 2)
            return Task.FromResult("Some innocuous Hawaiian music oozes from the elevator's intercom. ");

        if (TurnsSinceMoving == 4)
        {
            context.RemoveActor(this);
            InLobby = !InLobby;
            var door = GetItem<TDoor>();
            door.IsOpen = true;
            TurnsSinceMoving = 0;
            return Task.FromResult(door.NowOpen(this));
        }
      
        return Task.FromResult(string.Empty);
    }

    public override InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client)
    {
        if (action.Match(["press", "push", "activate"], ["button"]))
            return new DisambiguationInteractionResult("Which button do you mean, the Up button or the Down button",
                new Dictionary<string, string>
                {
                    { "up", "up button" },
                    { "down", "down button" },
                    { "up button", "up button" },
                    { "down button", "down button" }
                }, "press the {0} button");

        if (action.Match(["press", "push", "activate"], ["up button", "up", "down button", "down"]))
        {
            if (!IsEnabled)
                return new PositiveInteractionResult("Nothing happens. ");

            if (action.MatchNoun(["up button", "up"]))
                return GoUp(context);

            if (action.MatchNoun(["down button", "down"]))
                return GoDown(context);
        }

        return base.RespondToSimpleInteraction(action, context, client);
    }

    protected InteractionResult Move(IContext context)
    {
        TurnsSinceMoving = 1;
        context.RegisterActor(this);
        GetItem<TDoor>().IsOpen = false;
        return new PositiveInteractionResult(
            "The elevator door slides shut. After a moment, you feel a sensation of vertical movement. ");
    }

    protected abstract InteractionResult GoDown(IContext context);

    protected abstract InteractionResult GoUp(IContext context);

    private Task<string> ElevatorIsSummoned(IContext context)
    {
        TurnsSinceSummoned++;

        if (TurnsSinceSummoned != 4)
            return Task.FromResult(string.Empty);

        GetItem<TDoor>().IsOpen = true;
        context.RemoveActor(this);
        InLobby = true;
        return Task.FromResult($"The door at the {EntranceDirection} end of the room slides open. ");
    }

    private Task<string> ElevatorIsEnabled(IContext context)
    {
        TurnsSinceEnabled++;
        if (TurnsSinceEnabled < 6)
            return Task.FromResult(string.Empty);

        IsEnabled = false;
        context.RemoveActor(this);
        return Task.FromResult("A recording says \"Elevator no longer enabled.\"");
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            $"This is a {Size} room with a sliding door to the {ExitDirection} which is open. " +
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
        if (GetItem<TDoor>().IsOpen)
            return new PositiveInteractionResult($"Pushing the {Color} button has no effect. ");

        if (HasBeenSummoned)
            return new PositiveInteractionResult("Patience, patience... ");

        context.RegisterActor(this);
        HasBeenSummoned = true;
        return new PositiveInteractionResult(response);
    }
}