using GameEngine.Location;
using Planetfall.Item.Kalamontee.Admin;

namespace Planetfall.Location.Kalamontee.Tower;

internal class TowerCore : LocationWithNoStartingItems
{
    public override string Name => "Tower Core";

    private UpperElevatorDoor Door => Repository.GetItem<UpperElevatorDoor>();

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        // This is the far end of the upper shaft, so the entrance needs the same two-part test the
        // Waiting Area uses on the lower one: the door open *and* the car standing at this end. A bare
        // Go<UpperElevator>() let you walk into a car parked down at the lobby, because the door flag
        // is shared by both ends of the shaft (issue #505).
        var intoTheCar = new MovementParameters
        {
            GatingItem = Door,
            CanGo = _ => GetLocation<UpperElevator>().IsOpenAtTheFarEnd,
            Location = GetLocation<UpperElevator>(),
            CustomFailureMessage = "The door is closed. "
        };

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