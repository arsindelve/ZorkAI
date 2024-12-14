using GameEngine.Location;
using Model.Movement;

namespace Planetfall.Location.Kalamontee.Admin;

public class AdminCorridor : LocationWithNoStartingItems
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.S, Go<AdminCorridorSouth>() },
            { Direction.W, Go<SystemsMonitors>() },
            {
                Direction.N,
                new MovementParameters
                    { CanGo = _ => false, CustomFailureMessage = "The rift is too wide to jump across. " }
            }
        };

    protected override string ContextBasedDescription =>
        "The hallway, in fact the entire building, has been rent apart here, presumably by seismic upheaval. " +
        "You can see the sky through the severed roof above, and the ground is thick with rubble. To the north " +
        "is a gaping rift, at least eight meters across and thirty meters deep. A wide doorway, " +
        "labelled \"Sistumz Moniturz,\" leads west. ";

    public override string Name => "Admin Corridor South";
}