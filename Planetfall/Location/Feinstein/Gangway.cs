using GameEngine.Location;

namespace Planetfall.Location.Feinstein;

public class Gangway : LocationWithNoStartingItems
{
    public override string Name => "Gangway";

    public override string[] NounsForMatching => ["stairway", "stairs", "stairwell"];

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            // The down exit back to Deck Nine is sealed by the same narrow bulkhead once it crashes
            // shut in the explosion (issue #529; globals.zil:646 gates DOWN on GANGWAY-DOOR).
            {
                Direction.Down,
                new MovementParameters
                {
                    Location = Repository.GetLocation<DeckNine>(),
                    CanGo = _ => Repository.GetItem<GangwayDoor>().IsOpen,
                    CustomFailureMessage = "The emergency bulkhead is closed. "
                }
            },
            { Direction.Up, Go<DeckEight>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "This is a steep metal gangway connecting Deck Eight, above, and Deck Nine, below. ";
    }
}