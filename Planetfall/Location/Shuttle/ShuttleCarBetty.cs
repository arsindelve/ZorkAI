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
                // Issue #468: key the destination off Betty's OWN control (BettyControlEast),
                // not Alfie's. The two cars move independently and nothing links them, so
                // reading AlfieControlEast teleported a stationary Betty to the wrong station
                // the moment Alfie's tunnel position went non-zero. BettyControlEast == 0
                // means Betty is docked at Kalamontee (its East end); otherwise Lawanda --
                // mirroring Alfie's own-control logic and KalamonteePlatform.BettyIsHere.
                Repository.GetLocation<BettyControlEast>().TunnelPosition == 0
                    ? Go<KalamonteePlatform>()
                    : Go<LawandaPlatform>()
            }
        };
    }
}