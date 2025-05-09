
namespace Planetfall.Location.Shuttle;

public class AlfieControlEast : ShuttleControl<ShuttleCarAlfie, AlfieControlEast>
{
    public override string Name => "Alfie Control East";
    
    protected override Direction LeaveControlsDirection => Direction.W;
    
    protected override void ResetOtherControls()
    {
        Repository.GetLocation<AlfieControlWest>().TunnelPosition = 0;
    }
}