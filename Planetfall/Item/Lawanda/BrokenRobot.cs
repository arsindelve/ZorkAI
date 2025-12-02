using GameEngine.Location;
using Model.Location;

namespace Planetfall.Item.Lawanda;

public class BrokenRobot : ItemBase
{
    public override string[] NounsForMatching => ["broken robot", "robot", "damaged robot"];

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return "Lying face down at the bottom of the stairs is a motionless robot. It appears to be damaged beyond repair. ";
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return NeverPickedUpDescription(currentLocation!);
    }
}
