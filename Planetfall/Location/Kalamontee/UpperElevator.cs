using Planetfall.Item.Kalamontee.Admin;
using Planetfall.Location.Kalamontee.Tower;

namespace Planetfall.Location.Kalamontee;

internal class UpperElevator : ElevatorBase<UpperElevatorDoor, UpperElevatorAccessSlot, UpperElevatorAccessCard>
{
    public override string Name => "Upper Elevator";

    // Issue #268: "elevator" is the shared/ambiguous term (both elevators answer to it, triggering the
    // "which one?" prompt); "upper"/"blue"/"blue elevator" are the distinguishing reply keys.
    public override string[] NounsForMatching => ["upper elevator", "elevator", "blue elevator", "blue", "upper"];

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