using Model.Movement;

namespace Planetfall.Location.Shuttle;

public class BettyControlWest : ShuttleControl
{
    public override string Name => "Betty Control West";
    
    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.E, Go<ShuttleCarBetty>() }
        };
    }

    public override void Init()
    {
        StartWithItem<ShuttleSlot<BettyControlWest>>();
    }
}