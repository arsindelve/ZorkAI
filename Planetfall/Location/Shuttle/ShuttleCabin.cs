using GameEngine.Location;

namespace Planetfall.Location.Shuttle;

public abstract class ShuttleCabin : LocationWithNoStartingItems
{
    protected abstract string Exit { get; }
    
    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This is the cabin of a large transport, with seating for around 20 people plus space for freight. There " +
            $"are open doors at the eastern and western ends of the cabin, and a doorway leads out to a wide platform to the {Exit}.";
    }

    // Shared by both shuttle cars (Alfie and Betty), which inherit this cabin description.
    protected override IReadOnlyList<SceneryItem> Scenery =>
    [
        new(["seating", "seat", "seats", "freight", "cargo"],
            "There is seating for a couple of dozen passengers, with open space at the back for freight. ",
            "The seats are bolted to the cabin floor. ")
    ];
}