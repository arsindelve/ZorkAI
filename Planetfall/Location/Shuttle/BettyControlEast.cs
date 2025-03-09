using Model.Movement;

namespace Planetfall.Location.Shuttle;

public class BettyControlEast : ShuttleControl
{
    public override string Name => "Betty Control East";
    
    public override int TunnelPosition { get; set; } = EndOfTunnel;

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.W, Go<ShuttleCarBetty>() }
        };
    }
    
    public override void Init()
    {
        StartWithItem<ShuttleSlot<BettyControlEast>>();
    }
}