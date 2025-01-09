using Model.AIGeneration;
using Planetfall.Item.Kalamontee.Admin;

namespace Planetfall.Location.Kalamontee;

internal class LowerElevator : ElevatorBase<LowerElevatorDoor>
{
    public override string Name => "Lower Elevator";

    protected override string Color => "red";
    
    protected override string ExitDirection => "north";
    
    protected override string EntranceDirection => "south";
    
}