namespace Planetfall.Location.Shuttle;

public class ShuttleCarBetty : ShuttleCabin
{
    public override string Name => "Shuttle Car Betty";

    protected override string Exit => "south";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.W, Go<BettyControlWest>() },
            { Direction.E, Go<BettyControlEast>() },
            {
                // Issue #461: the platform lies SOUTH of the cabin (you board Betty by going
                // north from the platform), and the cabin description says "platform to the
                // south". The exit was wired to Direction.N, contradicting both -- so "go
                // south" failed and the only way out was an unintuitive north<->north loop.
                Direction.S,
                Repository.GetLocation<AlfieControlEast>().TunnelPosition == 0
                    ? Go<LawandaPlatform>()
                    : Go<KalamonteePlatform>()
            }
        };
    }
}