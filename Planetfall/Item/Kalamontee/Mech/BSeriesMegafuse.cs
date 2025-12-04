namespace Planetfall.Item.Kalamontee.Mech;

public class BSeriesMegafuse : ItemBase, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching => ["megafuse", "b-series megafuse", "b-series", "fuse", "b megafuse", "b-megafuse"];

    public string OnTheGroundDescription(ILocation? currentLocation)
    {
        return "There is a B-series megafuse here. ";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return OnTheGroundDescription(currentLocation);
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A B-series megafuse";
    }
}
