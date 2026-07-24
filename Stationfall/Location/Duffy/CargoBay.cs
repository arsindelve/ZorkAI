using Model.AIGeneration;

namespace Stationfall.Location.Duffy;

/// <summary>
///     A giant loading dock where truckloads of forms arrive from the printing planets
///     (ship.zil:903-920). The spacetruck waits here; you can board it once its hatch is open.
/// </summary>
public class CargoBay : LocationBase
{
    public override string Name => "Cargo Bay";

    public override string[] NounsForMatching => ["cargo bay", "bay"];

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        var hatch = Repository.GetItem<SpacetruckHatch>();

        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.W, Go<CargoBayEntrance>() },
            {
                Direction.In,
                new MovementParameters
                {
                    // Lets "enter truck" / "enter hatch" resolve to this exit.
                    GatingItem = hatch,
                    Location = Repository.GetLocation<Spacetruck>(),
                    CanGo = _ => hatch.IsOpen,
                    CustomFailureMessage = "The spacetruck's hatch is closed. "
                }
            }
        };
    }

    /// <summary>
    ///     "enter truck" is the natural way to board, but the truck is a room rather than an item, so
    ///     the parser has nothing to resolve the noun to. Route the common phrasings to the In exit.
    /// </summary>
    public override Task<InteractionResult> RespondToSpecificLocationInteraction(string? input, IContext context,
        IGenerationClient client)
    {
        var normalized = input?.ToLowerInvariant().Replace("the ", "").Trim();

        string[] boarding =
        [
            "enter truck", "enter spacetruck", "board truck", "board spacetruck", "get in truck",
            "get into truck", "get in spacetruck", "climb in truck", "enter cab"
        ];

        if (normalized is not null && boarding.Contains(normalized))
        {
            var hatch = Repository.GetItem<SpacetruckHatch>();

            if (!hatch.IsOpen)
                return Task.FromResult<InteractionResult>(
                    new PositiveInteractionResult("The spacetruck's hatch is closed. "));

            return Task.FromResult<InteractionResult>(new PositiveInteractionResult(
                MoveTo<Spacetruck>(context, client)));
        }

        return base.RespondToSpecificLocationInteraction(input, context, client);
    }

    private static string MoveTo<T>(IContext context, IGenerationClient client) where T : class, ILocation, new()
    {
        var destination = Repository.GetLocation<T>();
        context.CurrentLocation.OnLeaveLocation(context, destination, context.CurrentLocation);
        context.CurrentLocation = destination;
        return destination.GetDescription(context);
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        var hatch = Repository.GetItem<SpacetruckHatch>();

        return "This enormous airlock is a loading dock, where truckloads of forms arrive from the " +
               "printing planets of the sector and are distributed through the administrative deck. " +
               "The only exit on foot is back the way you came. A spacetruck waits here, its hatch " +
               $"{(hatch.IsOpen ? "open" : "closed")}. ";
    }

    public override void Init()
    {
        StartWithItem<SpacetruckHatch>();
    }
}
