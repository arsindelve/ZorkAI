using GameEngine.Location;
using Model.Movement;

namespace Planetfall.Location.Kalamontee.Admin;

internal class PlanRoom : LocationWithNoStartingItems
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.W, Go<AdminCorridorNorth>() }
        };

    protected override string ContextBasedDescription =>
        "This is a small room whose far wall is covered with many small cubbyholes, all empty. The left wall is " +
        "covered with an enormous map, labelled \"Kalamontee Kompleks\", showing two installations connected by " +
        "a long hallway. Near the upper part of this map is a red arrow saying \"Yuu ar heer.\" The right wall " +
        "is covered with a similar map, labelled \"Lawanda Kompleks\", showing two installations, one apparently " +
        "buried deep underground. ";
    
    public override string Name => "Plan Room";
}