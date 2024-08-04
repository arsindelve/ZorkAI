using GameEngine.Location;
using Model.Movement;

namespace Planetfall.Location;

public class DeckNine : LocationWithNoStartingItems 
{
    protected override Dictionary<Direction, MovementParameters> Map { get; }

    protected override string ContextBasedDescription =>
        "This is a featureless corridor similar to every other corridor on the ship. It curves away to starboard, and a gangway leads up. To port is the entrance to one of the ship's primary escape pods. The pod bulkhead is closed.";

    public override string Name => "Deck Nine";
}