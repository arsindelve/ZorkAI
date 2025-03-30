using GameEngine.Location;

namespace Planetfall.Location.Feinstein;

public class Gangway : LocationWithNoStartingItems
{
    public override string Name => "Gangway";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.Down, Go<DeckNine>() },
            { Direction.Up, Go<DeckEight>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "This is a steep metal gangway connecting Deck Eight, above, and Deck Nine, below. ";
    }
}