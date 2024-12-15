namespace Planetfall.Item.Kalamontee.Admin;

public class Key : ItemBase, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching => ["key", "shiny object", "steel key", "shiny thing"];

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A key";
    }

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a key here. ";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        // Deliberate implementation. The key is "hidden" initially. 
        return string.Empty;
    }

    public override string? CannotBeTakenDescription => HasEverBeenPickedUp
        ? ""
        : "Either the crevice is too narrow, or your fingers are too large. ";
}