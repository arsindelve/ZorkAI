using GameEngine.Location;
using Model.AIGeneration;
using Planetfall.Item.Kalamontee.Admin;

namespace Planetfall.Location.Kalamontee.Tower;

internal class TowerCore : LocationBase
{
    public override string Name => "Tower Core";

    // This is the far end of the upper shaft, so the entrance needs the same two-part test the Waiting
    // Area uses on the lower one: the door open *and* the car standing at this end. A bare
    // Go<UpperElevator>() let you walk into a car parked down at the lobby, because the door flag is
    // shared by both ends of the shaft (issue #505). That test is the tower door's own IsOpen.
    private Doorway Door => new(GetItem<UpperElevatorTowerDoor>());

    // The room's description names this door ("a sliding door leads north"), so the room has to have
    // one. It never did - the shaft's single door object was owned by the car and the lobby - so
    // "examine door" and "enter door" up here resolved to nothing at all (issue #532).
    public override void Init()
    {
        StartWithItem<UpperElevatorTowerDoor>();
    }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        var intoTheCar = Door.Passage(GetLocation<UpperElevator>());

        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.SW, Go<ObservationDeck>() },
            { Direction.N, intoTheCar },
            { Direction.Up, Go<Helipad>() },
            { Direction.NE, Go<CommRoom>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This is a small, circular room. A sliding door leads north, and a spiral staircase heads " +
            "upwards. Other exits lie to the northeast and southwest.";
    }

    protected override void OnFirstTimeEnterLocation(IContext context)
    {
        context.AddPoints(4);
    }
}
