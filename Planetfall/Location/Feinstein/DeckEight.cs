namespace Planetfall.Location.Feinstein;

internal class DeckEight : BlatherLocation
{
    public override string Name => "Deck Eight";

    // "deck 8" so naming the deck works whether the player (or the AI parser) writes the number out or
    // as a digit — the room title spells it, but "go to deck 8" is at least as common (issue #268).
    public override string[] NounsForMatching => ["deck 8"];

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.Down, Go<Gangway>() },
            {
                Direction.W,
                BlatherBlocksYou()
            },
            {
                Direction.N,
                BlatherBlocksYou()
            },
            {
                Direction.E,
                BlatherBlocksYou()
            }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "This is a featureless corridor leading port and starboard. A gangway leads down, and to fore " +
               "is the Hyperspatial Jump Machinery Room. ";
    }
}