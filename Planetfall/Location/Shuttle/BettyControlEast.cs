using Model.Movement;

namespace Planetfall.Location.Shuttle;

public class BettyControlEast : ShuttleControl<ShuttleCarBetty, BettyControlEast>
{
    public override string Name => "Betty Control East";
    
    public override int TunnelPosition { get; set; } = EndOfTunnel;
    
    protected override Direction LeaveControlsDirection => Direction.W;
    
    protected override void ResetOtherControls()
    {
        Repository.GetLocation<BettyControlWest>().TunnelPosition = 0;
    }
}