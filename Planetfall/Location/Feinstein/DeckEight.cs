namespace Planetfall.Location.Feinstein;

internal class DeckEight : BlatherLocation
{
    public override string Name => "Deck Eight";

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