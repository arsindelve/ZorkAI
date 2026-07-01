namespace Planetfall.Location.Shuttle;

public class BettyControlWest : ShuttleControl<ShuttleCarBetty, BettyControlWest>
{
    public override string Name => "Betty Control West";

    public override string[] NounsForMatching => ["betty cockpit"];
    
    protected override Direction LeaveControlsDirection => Direction.E;
    
    protected override void ResetOtherControls()
    {
        Repository.GetLocation<BettyControlEast>().TunnelPosition = 0;
    }
}