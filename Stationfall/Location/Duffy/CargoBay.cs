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
