namespace Planetfall.Item.Kalamontee.Mech;

public class GoodBedistor : ItemBase, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching => ["bedistor", "ninety-ohm bedistor", "good ninety-ohm bedistor", "good bedistor", "ninety-ohm", "90-ohm bedistor", "90-ohm"];

    public string OnTheGroundDescription(ILocation? currentLocation)
    {
        return "There is a good ninety-ohm bedistor here. ";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return OnTheGroundDescription(currentLocation);
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A good ninety-ohm bedistor";
    }
}
