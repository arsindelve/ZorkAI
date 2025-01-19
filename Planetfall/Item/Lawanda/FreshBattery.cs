namespace Planetfall.Item.Lawanda;

public class FreshBattery : ItemBase, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching => ["battery", "fresh", "fresh battery"];
    
    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a new battery here. ";
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A new battery";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return
            "The odds and ends on the shelves include a small round object, which closer inspection " +
            "reveals to be a fresh laser battery. ";
    }
}