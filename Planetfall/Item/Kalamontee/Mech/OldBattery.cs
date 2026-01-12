namespace Planetfall.Item.Kalamontee.Mech;

public class OldBattery : BatteryBase, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching => ["battery", "old battery"];
    
    public override int ChargesRemaining { get; set; } = 3;

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "An old battery";
    }
    
    public string OnTheGroundDescription(ILocation? currentLocation)
    {
        return "There is an old battery here. ";
    }
    
    public override string NeverPickedUpDescription(ILocation currentLocation) => OnTheGroundDescription(currentLocation);
}