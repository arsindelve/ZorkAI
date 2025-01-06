using GameEngine.Location;
using Model.Movement;
using Planetfall.Item.Kalamontee.Admin;

namespace Planetfall.Location.Kalamontee;

internal abstract class ElevatorBase<TDoor> : LocationBase where TDoor : ElevatorDoorBase, IItem, new()
{
    // A recorded voice chimes "Elevator enabled."
    // A recorded voice chimes "Elevator enabled."
    // Some innocuous Hawaiian music oozes from the elevator's intercom.
    // The elevator door slides shut. After a moment, you feel a sensation of vertical movement.
    // The elevator door slides open.
    // TODO: Floyd bounces into the elevator. "Hey, wait for Floyd!" he yells, smiling broadly.

    [UsedImplicitly] public bool IsEnabled { get; set; }

    protected abstract string ExitDirection { get; }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            $"This is a medium-sized room with a door to the {ExitDirection} which is open. " +
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
}