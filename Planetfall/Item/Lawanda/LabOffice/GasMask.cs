namespace Planetfall.Item.Lawanda.LabOffice;

public class GasMask : ItemBase, ICanBeTakenAndDropped, ICanBeExamined, IAmClothing
{
    public override string[] NounsForMatching => ["gas mask", "mask", "breathing equipment"];

    public override int Size => 2;

    public bool BeingWorn { get; set; } = false;

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "A gas mask lies here. ";
    }

    public string ExaminationDescription =>
        "It's a protective gas mask with a clear visor and breathing filters. " +
        "It looks like it would protect against airborne contaminants. ";

    public override string GenericDescription(ILocation? currentLocation)
    {
        return BeingWorn ? "A gas mask (being worn)" : "A gas mask";
    }
}
