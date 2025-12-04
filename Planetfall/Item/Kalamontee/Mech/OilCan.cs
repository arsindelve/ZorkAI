namespace Planetfall.Item.Kalamontee.Mech;

public class OilCan : ItemBase, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching => ["oil can", "oilcan", "can", "oil"];

    public string OnTheGroundDescription(ILocation? currentLocation)
    {
        return "There is an oil can here. ";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return "One dusty shelf, otherwise bare, holds a small oil can. ";
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "An oil can";
    }
}
