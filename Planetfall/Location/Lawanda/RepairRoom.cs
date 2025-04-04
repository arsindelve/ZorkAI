using GameEngine.Location;

namespace Planetfall.Location.Lawanda;

internal class RepairRoom : LocationWithNoStartingItems
{
    public override string Name => "Repair Room";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.S, Go<SystemsCorridorWest>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "You are in a dimly lit room, filled with strange machines and wide storage cabinets, all locked. To the south, a narrow " +
            "stairway leads upward. On the north wall of the room is a very small doorway. ";
    }
}
