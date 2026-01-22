namespace Planetfall.Item.Lawanda.CryoElevator;

public class CryoElevatorDoor : ItemBase, ICanBeExamined
{
    public override string[] NounsForMatching => ["door", "elevator door", "doors"];

    public override int Size => 100; // Can't be taken

    public string ExaminationDescription =>
        "Heavy metal elevator doors. They appear to be controlled by the button. ";
}
