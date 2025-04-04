using GameEngine.Location;

namespace Planetfall.Location.Lawanda;

internal class SystemsCorridorEast : LocationWithNoStartingItems
{
    public override string Name => "Systems Corridor East";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.S, Go<LibraryLobby>() },
            { Direction.E, Go<PhysicalPlant>() },
            { Direction.W, Go<SystemsCorridor>() },
            { Direction.N, Go<CourseControl>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "The hallway ends here with a large doorway leading east, and smaller doorways " +
            "to the north\nand south. The northern doorway is labelled \"Planateree Kors Kontrool.\" " +
            "The hallway itself leads west. ";
    }
}
