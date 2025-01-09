using GameEngine.Location;
using Model.AIGeneration;
using Model.Movement;
using Planetfall.Item.Kalamontee.Admin;

namespace Planetfall.Location.Kalamontee;

internal abstract class ElevatorBase<TDoor> : LocationBase, ITurnBasedActor where TDoor : ElevatorDoorBase, IItem, new()
{
    [UsedImplicitly] public bool HasBeenSummoned { get; set; }

    [UsedImplicitly] public bool InLobby { get; set; }

    [UsedImplicitly] public int TurnsSinceSummoning { get; set; }

    // A recorded voice chimes "Elevator enabled."
    // A recorded voice chimes "Elevator enabled."
    // Some innocuous Hawaiian music oozes from the elevator's intercom.
    // The elevator door slides shut. After a moment, you feel a sensation of vertical movement.
    // The elevator door slides open.
    // TODO: Floyd bounces into the elevator. "Hey, wait for Floyd!" he yells, smiling broadly.

    [UsedImplicitly] public bool IsEnabled { get; set; }

    protected abstract string Color { get; }
    
    protected abstract string Size { get; }

    protected abstract string ExitDirection { get; }

    protected abstract string EntranceDirection { get; }

    public Task<string> Act(IContext context, IGenerationClient client)
    {
        TurnsSinceSummoning++;

        if (TurnsSinceSummoning != 4)
            return Task.FromResult(string.Empty);

        GetItem<TDoor>().IsOpen = true;
        context.RemoveActor(this);
        InLobby = true;
        return Task.FromResult($"The door at the {EntranceDirection} end of the room slides open. ");
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
                    CustomFailureMessage = "The door is closed",
                    CanGo = _ => Repository.GetItem<TDoor>().IsOpen
                }
            }
        };
    }

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