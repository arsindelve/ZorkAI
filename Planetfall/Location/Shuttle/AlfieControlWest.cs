namespace Planetfall.Location.Shuttle;

public class AlfieControlWest : ShuttleControl<ShuttleCarAlfie, AlfieControlWest>
{
    public override string Name => "Alfie Control West";

    public override int TunnelPosition { get; set; } = EndOfTunnel;
    
    protected override Direction LeaveControlsDirection => Direction.E;
    
    protected override void ResetOtherControls()
    {
        Repository.GetLocation<AlfieControlEast>().TunnelPosition = 0;
    }
}