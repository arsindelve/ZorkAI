using GameEngine.Location;
using Model.Location;

namespace Planetfall.Item.Lawanda;

public class NinetyOhmBedistor : ItemBase, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching => ["ninety-ohm bedistor", "bedistor"];

    public string OnTheGroundDescription(ILocation? currentLocation)
    {
        return "There is a ninety-ohm bedistor here. ";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return OnTheGroundDescription(currentLocation);
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A ninety-ohm bedistor";
    }
}
