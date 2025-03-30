using GameEngine.Location;

namespace Planetfall.Location.Kalamontee.Tower;

public class Helipad : LocationWithNoStartingItems
{
    public override string Name => "Helipad";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.Down, Go<TowerCore>() },
            { Direction.In, Go<Helicopter>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "You are at the center of a wide, flat area atop the tower. A fence prevents you from " +
            "approaching the perimeter, so your view is limited to cloud-filled sky. A large vehicle, " +
            "severely weathered and topped with rotor blades, lies nearby. A spiral staircase leads " +
            "down into the tower. ";
    }
}