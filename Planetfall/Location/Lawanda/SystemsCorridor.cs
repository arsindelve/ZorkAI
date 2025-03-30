using GameEngine.Location;

namespace Planetfall.Location.Lawanda;

internal class SystemsCorridor : LocationWithNoStartingItems
{
    public override string Name => "Systems Corridor";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.E, Go<SystemsCorridorEast>() },
            { Direction.W, Go<SystemsCorridorWest>() },
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This section of hallway has a doorway to the north labelled \"Planateree Deefens.\" The corridor " +
            "continues east and west. ";
    }
}