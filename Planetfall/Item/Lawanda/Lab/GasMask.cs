namespace Planetfall.Item.Lawanda.Lab;

public class GasMask : ItemBase, ICanBeTakenAndDropped, ICanBeExamined, IAmClothing
{
    public override string[] NounsForMatching => ["gas mask", "mask"];

    public override int Size => 10;

    public bool BeingWorn { get; set; }

    public string ExaminationDescription =>
        "The gas mask is a standard issue protective device designed to filter out toxic gases and chemicals. ";

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a gas mask here. ";
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A gas mask";
    }
}
