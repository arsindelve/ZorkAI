using GameEngine.Location;
using Model.Movement;

namespace Planetfall.Location.Kalamontee.Admin;

internal class AdminCorridorNorth : LocationWithNoStartingItems
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.E, Go<PlanRoom>() },
            { Direction.N, Go<TransportationSupply>() },
            { Direction.W, Go<SmallOffice>() }
        };

    protected override string ContextBasedDescription =>
        "The corridor ends here. Portals lead west, north, and east. Signs above these portals read, respectively, " +
        "\"Administraativ Awfisiz,\" \"Tranzportaashun Suplii,\" and \"Plan Ruum.\" To the south is a wide rift, " +
        "spanned by a metal ladder, separating this area from the rest of the building. ";

    public override string Name => "Admin Corridor North";
}