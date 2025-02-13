namespace Planetfall.Item.Kalamontee.Mech;

public class Magnet : ItemBase, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching => ["curved metal bar", "metal bar", "bar", "magnet"];

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a curved metal bar here. ";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return "On an upper shelf is a metal bar, curved into a U-shape. ";
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A curved metal bar";
    }
}