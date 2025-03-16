using Model.Movement;

namespace Planetfall.Location.Shuttle;

public class AlfieControlWest : ShuttleControl<ShuttleCarAlfie, AlfieControlWest>
{
    public override string Name => "Alfie Control West";

    public override int TunnelPosition { get; set; } = EndOfTunnel;
    
    protected override Direction LeaveControlsDirection => Direction.E;
}