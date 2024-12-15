namespace Planetfall.Item.Kalamontee;

public class Canteen : OpenAndCloseContainerBase, ICanBeExamined, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching => ["canteen", "octagonally shaped canteen"];

    public string ExaminationDescription => $"The canteen is {(IsOpen ? "open" : "closed")}. ";

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return "Although the room is quite barren, an octagonally shaped canteen is sitting on one of the benches. ";
    }

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a canteen here. ";
    }

    public override void Init()
    {
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A canteen";
    }
}