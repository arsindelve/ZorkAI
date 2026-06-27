using Planetfall.Item.Kalamontee.Admin;
using Planetfall.Location.Kalamontee.Tower;

namespace Planetfall.Location.Kalamontee;

internal class UpperElevator : ElevatorBase<UpperElevatorDoor, UpperElevatorAccessSlot, UpperElevatorAccessCard>
{
    public override string Name => "Upper Elevator";

    // Issue #268: destination matching already derives "upper"/"elevator" from the Name, so we only add
    // the colour alias that isn't in the title. "elevator" stays the shared term (both cars answer to
    // it -> "which one?"); "blue"/"upper" are the distinguishing reply keys.
    public override string[] NounsForMatching => ["blue"];

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