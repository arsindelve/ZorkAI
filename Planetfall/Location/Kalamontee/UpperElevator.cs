using Model.AIGeneration;
using Planetfall.Item.Kalamontee.Admin;

namespace Planetfall.Location.Kalamontee;

internal class UpperElevator : ElevatorBase<UpperElevatorDoor>
{
    public override string Name => "Upper Elevator";

    protected override string Color => "blue";

    protected override string Size => "tiny";

    protected override string ExitDirection => "south";
    
    protected override string EntranceDirection => "north";
}