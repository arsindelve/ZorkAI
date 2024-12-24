namespace Planetfall.Item.Kalamontee.Admin;

public class UpperElevatorAccessCard : ItemBase, ICanBeExamined, ICanBeTakenAndDropped, ICanBeRead
{
    public override string[] NounsForMatching =>
        ["upper elevator access card", "elevator access card", "card", "upper", "elevator", "upper card", "access card", "upper elevator card", "upper elevator access"];
    
    public string ExaminationDescription => "The card is embossed \"upur elivaatur akses kard.\"";

    public string ReadDescription => ExaminationDescription;

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is an upper elevator access card here. ";
    }
    
    public override string GenericDescription(ILocation? currentLocation)
    {
        return "An upper elevator access card";
    }
}