using Planetfall.Item.Kalamontee.Admin;
using Planetfall.Location.Kalamontee.Tower;

namespace Planetfall.Location.Kalamontee;

internal class UpperElevator : ElevatorBase<UpperElevatorDoor, UpperElevatorAccessSlot, UpperElevatorAccessCard>
{
    public override string Name => "Upper Elevator";

    protected override string Color => "blue";

    protected override string Size => "tiny";

    protected override string ExitDirection => "south";

    protected override string EntranceDirection => "north";

    protected override InteractionResult GoDown(IContext context)
    {
        if (InLobby)
            return new PositiveInteractionResult("Nothing happens. ");

        return Move(context);
    }

    protected override InteractionResult GoUp(IContext context)
    {
        if (!InLobby)
            return new PositiveInteractionResult("Nothing happens. ");

        return Move(context);
    }
    
    protected override ILocation? Exit()
    {
        return InLobby ? GetLocation<ElevatorLobby>() : Repository.GetLocation<TowerCore>();
    }
}