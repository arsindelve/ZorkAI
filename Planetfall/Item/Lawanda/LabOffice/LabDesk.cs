namespace Planetfall.Item.Lawanda.LabOffice;

public class LabDesk : OpenAndCloseContainerBase, ICanBeExamined
{
    public override string[] NounsForMatching => ["desk", "lab desk", "drawer"];

    public override int Size => 20;

    public string ExaminationDescription =>
        "It's a standard laboratory desk with a single drawer. " +
        (IsOpen ? "The drawer is open. " : "The drawer is closed. ") +
        (IsOpen && Items.Any() ? $"\n{ItemListDescription("drawer", null)}" : "");

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return "A laboratory desk stands here with a drawer. " +
               (IsOpen && Items.Any() ? $"\n{ItemListDescription("drawer", null)}" : "");
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A laboratory desk";
    }

    public override void Init()
    {
        IsOpen = false;
        StartWithItemInside<GasMask>();
    }
}
