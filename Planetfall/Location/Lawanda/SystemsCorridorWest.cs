using GameEngine.Location;

namespace Planetfall.Location.Lawanda;

internal class SystemsCorridorWest : LocationWithNoStartingItems
{
    public override string Name => "Systems Corridor West";
    
    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.E, Go<SystemsCorridor>() },
            { Direction.SW, Go<Fork>() },
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "The corridor bends here, leading east and southwest. A doorway opens to the northwest, and a narrow " +
            "stairway leads down to the north. ";
    }
}