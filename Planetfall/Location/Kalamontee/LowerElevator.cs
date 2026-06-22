using Planetfall.Item.Kalamontee.Admin;
using Planetfall.Location.Shuttle;

namespace Planetfall.Location.Kalamontee;

internal class LowerElevator : ElevatorBase<LowerElevatorDoor, LowerElevatorAccessSlot, LowerElevatorAccessCard>
{
    public override string Name => "Lower Elevator";

    // Issue #268: "elevator" is the shared/ambiguous term; "lower"/"red"/"red elevator" distinguish it.
    public override string[] NounsForMatching => ["lower elevator", "elevator", "red elevator", "red", "lower"];

    protected override string Color => "red";

    protected override string Size => "medium-sized";

    protected override string ExitDirection => "north";
    
    protected override string EntranceDirection => "south";

    protected override InteractionResult GoDown(IContext context)
    {
        if (!InLobby)
            return new PositiveInteractionResult("Nothing happens. ");

        return Move(context);
    }

    protected override InteractionResult GoUp(IContext context)
    {
        if (InLobby)
            return new PositiveInteractionResult("Nothing happens. ");

        return Move(context);
    }

    protected override ILocation? Exit()
    {
        return InLobby ? GetLocation<ElevatorLobby>() : GetLocation<WaitingArea>();
    }
}