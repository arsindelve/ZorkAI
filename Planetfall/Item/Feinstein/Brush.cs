namespace Planetfall.Item.Feinstein;
public class Brush : ItemBase, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching =>
    [
        "brush", "scrub brush", "multi-purpose scrub brush", "patrol-issue self-contained multi-purpose scrub brush"
    ];

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a Patrol-issue self-contained multi-purpose scrub brush here.";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return OnTheGroundDescription(currentLocation);
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A Patrol-issue self-contained multi-purpose scrub brush ";
    }
}