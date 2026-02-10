namespace Planetfall.Item.Kalamontee.Mech;

public class KSeriesMegafuse : ItemBase, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching => ["k-series megafuse", "k-series", "fuse", "k megafuse", "k-megafuse", "megafuse"];

    public string OnTheGroundDescription(ILocation? currentLocation)
    {
        return "There is a K-series megafuse here. ";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return OnTheGroundDescription(currentLocation);
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A K-series megafuse";
    }
}
