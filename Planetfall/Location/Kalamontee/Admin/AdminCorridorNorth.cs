using Planetfall.Item.Kalamontee;

namespace Planetfall.Location.Kalamontee.Admin;

internal class AdminCorridorNorth : RiftLocationBase
{
    public override string Name => "Admin Corridor North";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
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
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "The corridor ends here. Portals lead west, north, and east. Signs above these portals read, respectively, " +
            "\"Administraativ Awfisiz,\" \"Tranzportaashun Suplii,\" and \"Plan Ruum.\" To the south is a wide rift, " +
            $"{(GetItem<Ladder>().IsAcrossRift ? "spanned by a metal ladder, " : "")}separating this area from the rest of the building. ";
    }

    public override string BeforeEnterLocation(IContext context, ILocation previousLocation)
    {
        string prepend = "";
        if (previousLocation is AdminCorridor)
            prepend =
                "You slowly make your way across the swaying ladder. You can see sharp, pointy rocks at the bottom of the rift, far below...\n\n";

        return prepend + base.BeforeEnterLocation(context, previousLocation);
    }

    protected override void OnFirstTimeEnterLocation(IContext context)
    {
        context.AddPoints(4);
        base.OnFirstTimeEnterLocation(context);
    }
}