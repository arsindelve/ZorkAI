using GameEngine.Location;
using Planetfall.Item.Kalamontee.Admin;
using Planetfall.Location.Kalamontee;

namespace Planetfall.Location.Shuttle;

public class WaitingArea : LocationWithNoStartingItems
{
    public override string Name => "Waiting Area";

    public override string[] NounsForMatching => ["waiting room", "lounge"];

    private LowerElevatorDoor Door => Repository.GetItem<LowerElevatorDoor>();

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        // The original gates this entrance on the door being open *and* the car actually being at
        // this end of the shaft (compone.zil, OTHER-ELEVATOR-ENTER-F, which also makes the door the
        // implicit "it" - hence GatingItem). A bare Go<LowerElevator>() let you walk into a car
        // parked up at the lobby, and then be sealed in, since the car's own exits gate on the door.
        var intoTheCar = new MovementParameters
        {
            GatingItem = Door,
            CanGo = _ => Door.IsOpen && !GetLocation<LowerElevator>().InLobby,
            Location = GetLocation<LowerElevator>(),
            CustomFailureMessage = "The door is closed. "
        };

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