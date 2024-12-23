using GameEngine.Location;
using Model.AIGeneration;
using Model.Movement;
using Planetfall.Command;

namespace Planetfall.Location.Kalamontee.Admin;

internal class AdminCorridorNorth : LocationWithNoStartingItems
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.E, Go<PlanRoom>() },
            { Direction.N, Go<TransportationSupply>() },
            { Direction.W, Go<SmallOffice>() },
            { Direction.S, new MovementParameters
            {
                Location = GetLocation<AdminCorridor>(),
                CanGo = _ => GetLocation<AdminCorridor>().LadderAcrossRift,
                CustomFailureMessage = "The rift is too wide to jump across. "
            }}
        };

    protected override string ContextBasedDescription =>
        "The corridor ends here. Portals lead west, north, and east. Signs above these portals read, respectively, " +
        "\"Administraativ Awfisiz,\" \"Tranzportaashun Suplii,\" and \"Plan Ruum.\" To the south is a wide rift, " +
        "spanned by a metal ladder, separating this area from the rest of the building. ";

    public override InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client)
    {
        if (action.Match(["jump", "leap"], AdminCorridor.RiftNouns))
            return new DeathProcessor().Process(
                "You get a brief (but much closer) view of the sharp and nasty rocks at the bottom of the rift. ",
                context);

        return base.RespondToSimpleInteraction(action, context, client);
    }
    
    public override string Name => "Admin Corridor North";
}