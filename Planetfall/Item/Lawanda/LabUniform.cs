namespace Planetfall.Item.Lawanda;

public class LabUniform : OpenAndCloseContainerBase, ICanBeTakenAndDropped, ICanBeExamined, IAmClothing
{
    public override string[] NounsForMatching => ["lab uniform", "uniform"];

    public bool BeingWorn { get; set; } = false;

    public string ExaminationDescription =>
        "It is a plain lab uniform. The logo above the pocket depicts a flame burning above some kind of sleep " +
        $"chamber. The pocket is {(IsOpen ? "open" : "closed")}. " +
        (Items.Any() && IsOpen ? $"\n{ItemListDescription("lab uniform", null)}" : "");

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a lab uniform here. " +
               (Items.Any() && IsOpen ? $"\n{ItemListDescription("lab uniform", null)}" : "");
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return "Hanging on a rack is a pale blue lab uniform. Sewn onto its pocket is a nondescript logo. ";
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A lab uniform";
    }

    public override void Init()
    {
        
    }
}