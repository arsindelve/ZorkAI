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
}