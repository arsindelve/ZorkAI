namespace Planetfall.Item.Lawanda.LabOffice;

public class GasMask : ItemBase, ICanBeTakenAndDropped, IAmClothing
{
    public override string[] NounsForMatching => ["gas mask", "mask"];

    public bool BeingWorn { get; set; } = false;

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a gas mask here. ";
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return BeingWorn ? "A gas mask (being worn)" : "A gas mask";
    }
}
