using Model.Movement;

namespace Planetfall.Location.Feinstein;

internal class ReactorLobby : BlatherLocation
{
    public override string Name => "Reactor Lobby";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.W, Go<DeckNine>() },
            { Direction.E, BlatherBlocksYou() },
            { Direction.S, BlatherBlocksYou() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "The corridor widens here as it nears the main drive area. To starboard is the Ion Reactor that powers " +
            "the vessel, and aft of here is the Auxiliary Control Room. The corridor continues to port. ";
    }
}