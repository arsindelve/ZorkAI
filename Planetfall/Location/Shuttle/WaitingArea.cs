using GameEngine.Location;
using Planetfall.Item.Kalamontee.Admin;
using Planetfall.Location.Kalamontee;

namespace Planetfall.Location.Shuttle;

public class WaitingArea : LocationBase
{
    public override string Name => "Waiting Area";

    public override string[] NounsForMatching => ["waiting room", "lounge"];

    // The original gates this entrance on the door being open *and* the car actually being at this end
    // of the shaft (compone.zil, OTHER-ELEVATOR-ENTER-F, which also makes the door the implicit "it" -
    // hence the Doorway's GatingItem). A bare Go<LowerElevator>() let you walk into a car parked up at
    // the lobby, and then be sealed in, since the car's own exits gate on the door. That two-part test
    // is the waiting area door's own IsOpen.
    private Doorway Door => new(GetItem<LowerElevatorWaitingAreaDoor>());

    // The room's description names this door ("to the south is a metal door"), so the room has to have
    // one. It never did - the shaft's single door object was owned by the car and the lobby - so
    // "examine door" and "enter door" here resolved to nothing at all (issue #532).
    public override void Init()
    {
        StartWithItem<LowerElevatorWaitingAreaDoor>();
    }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        var intoTheCar = Door.Passage(GetLocation<LowerElevator>());

        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.S, intoTheCar },
            { Direction.E, Go<KalamonteePlatform>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This is a concrete platform sparsely furnished with benches. The platform continues to the east, " +
            "and to the south is a metal door. ";
    }

    protected override IReadOnlyList<SceneryItem> Scenery =>
    [
        new(["bench", "benches"], "The benches look distinctly uncomfortable. ",
            "The benches are bolted to the platform. ")
    ];
}
