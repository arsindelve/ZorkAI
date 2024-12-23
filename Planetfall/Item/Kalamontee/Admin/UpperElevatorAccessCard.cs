namespace Planetfall.Item.Kalamontee.Admin;

public class UpperElevatorAccessCard : ItemBase, ICanBeExamined, ICanBeTakenAndDropped, ICanBeRead
{
    public override string[] NounsForMatching =>
        ["card", "access card", "upper elevator access card", "upper elevator card", "upper elevator access"];
    
    public string ExaminationDescription => "The card is embossed \"upur elivaatur akses kard.\"";

    public string ReadDescription => ExaminationDescription;

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is an upper elevator access card here. ";
    }
}