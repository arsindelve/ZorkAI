using Model.Movement;

namespace Planetfall.Location.Shuttle;

public class BettyControlWest : ShuttleControl<ShuttleCarBetty, BettyControlWest>
{
    public override string Name => "Betty Control West";
    protected override Direction LeaveControlsDirection => Direction.E;
}