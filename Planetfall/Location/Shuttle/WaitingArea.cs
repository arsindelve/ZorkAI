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

    protected override IReadOnlyList<SceneryItem> Scenery =>
    [
        new(["bench", "benches"], "The benches look distinctly uncomfortable. ",
            "The benches are bolted to the platform. ")
    ];
}