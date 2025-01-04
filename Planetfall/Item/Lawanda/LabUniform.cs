namespace Planetfall.Item.Lawanda;

public class LabUniform : ContainerBase, ICanBeTakenAndDropped, ICanBeExamined, IAmClothing
{
    public override string[] NounsForMatching => ["lab uniform", "uniform"];

    public bool BeingWorn { get; set; } = false;

    public string ExaminationDescription =>
        "It is a plain lab uniform. The logo above the pocket depicts a flame burning above some kind of sleep chamber. The pocket is open. ";

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        throw new NotImplementedException();
    }

    public override void Init()
    {
    }
}