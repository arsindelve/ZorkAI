using Model.Movement;
using Planetfall.Item.Kalamontee;

namespace Planetfall.Location.Kalamontee.Admin;

internal class AdminCorridorNorth : RiftLocationBase
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.E, Go<PlanRoom>() },
            { Direction.N, Go<TransportationSupply>() },
            { Direction.W, Go<SmallOffice>() },
            {
                Direction.S, new MovementParameters
                {
                    Location = GetLocation<AdminCorridor>(),
                    CanGo = _ => GetItem<Ladder>().IsAcrossRift,
                    CustomFailureMessage = "The rift is too wide to jump across. "
                }
            }
        };

    protected override string GetContextBasedDescription() =>
        "The corridor ends here. Portals lead west, north, and east. Signs above these portals read, respectively, " +
        "\"Administraativ Awfisiz,\" \"Tranzportaashun Suplii,\" and \"Plan Ruum.\" To the south is a wide rift, " +
        $"{(GetItem<Ladder>().IsAcrossRift ? "spanned by a metal ladder, " : "")}separating this area from the rest of the building. ";

    public override string Name => "Admin Corridor North";

    public override string BeforeEnterLocation(IContext context, ILocation previousLocation)
    {
        if (previousLocation is AdminCorridor)
            return
                "You slowly make your way across the swaying ladder. You can see sharp, pointy rocks at the bottom of the rift, far below...\n\n";

        return base.BeforeEnterLocation(context, previousLocation);
    }
}