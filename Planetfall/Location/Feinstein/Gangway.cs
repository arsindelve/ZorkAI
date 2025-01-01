using GameEngine.Location;
using Model.Movement;

namespace Planetfall.Location.Feinstein;

public class Gangway : LocationWithNoStartingItems
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.Down, Go<DeckNine>() },
            { Direction.Up, Go<DeckEight>() }
        };

    protected override string GetContextBasedDescription() =>
        "This is a steep metal gangway connecting Deck Eight, above, and Deck Nine, below. ";

    public override string Name => "Gangway";
}