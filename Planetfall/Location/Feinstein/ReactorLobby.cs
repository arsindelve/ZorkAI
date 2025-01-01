using Model.Movement;

namespace Planetfall.Location.Feinstein;

internal class ReactorLobby : BlatherLocation
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.W, Go<DeckNine>() },
            { Direction.E, BlatherBlocksYou() },
            { Direction.S, BlatherBlocksYou() }
        };

    protected override string GetContextBasedDescription() =>
        "The corridor widens here as it nears the main drive area. To starboard is the Ion Reactor that powers " +
        "the vessel, and aft of here is the Auxiliary Control Room. The corridor continues to port. ";

    public override string Name => "Reactor Lobby";
}