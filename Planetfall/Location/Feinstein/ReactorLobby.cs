namespace Planetfall.Location.Feinstein;

internal class ReactorLobby : BlatherLocation
{
    public override string Name => "Reactor Lobby";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            // The west exit back to Deck Nine is sealed by the same wide corridor bulkhead once it
            // crashes shut in the explosion (issue #529; globals.zil:629 gates WEST on CORRIDOR-DOOR).
            {
                Direction.W,
                new MovementParameters
                {
                    Location = Repository.GetLocation<DeckNine>(),
                    CanGo = _ => Repository.GetItem<CorridorDoor>().IsOpen,
                    CustomFailureMessage = "The emergency bulkhead is closed. "
                }
            },
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

    protected override IReadOnlyList<SceneryItem> Scenery =>
    [
        new(["reactor", "ion reactor"],
            "The Ion Reactor hums away to starboard, powering the entire vessel. You'd best not meddle with it. ",
            "The reactor is, mercifully, not something you can pick up. ")
    ];
}