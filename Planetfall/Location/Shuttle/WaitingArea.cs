using GameEngine.Location;

namespace Planetfall.Location.Shuttle;

public class WaitingArea : LocationWithNoStartingItems
{
    public override string Name => "Waiting Area";
    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.S, Go<LowerElevator>() },
            { Direction.E, Go<KalamonteePlatform>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This is a concrete platform sparsely furnished with benches. The platform continues to the east, " +
            "and to the south is a metal door. ";
    }
}