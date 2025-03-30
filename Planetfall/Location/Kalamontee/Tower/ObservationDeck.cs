using GameEngine.Location;

namespace Planetfall.Location.Kalamontee.Tower;

public class ObservationDeck : LocationWithNoStartingItems
{
    public override string Name => "Observation Deck";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.NE, Go<TowerCore>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This is a balcony girdling the tower. The view is breathtaking; the tower must be half a " +
            "kilometer tall. From here it is clear that you are on an island. The dormitory section of " +
            "the complex is visible on the other side of the island, and the rest of the complex sprawls " +
            "out directly below. In the distance, about 20 kilometers to the east, you can spot another " +
            "island similar to this one. The only exit is a doorway leading northeast. ";
    }
}