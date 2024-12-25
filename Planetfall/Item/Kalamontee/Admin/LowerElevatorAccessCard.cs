namespace Planetfall.Item.Kalamontee.Admin;

public class LowerElevatorAccessCard : ElevatorAccessCard, ICanBeExamined, ICanBeTakenAndDropped, ICanBeRead
{
    public override string[] NounsForMatching =>
        ["lower elevator access card", "elevator access card", "card", "lower", "elevator", "lower card", "lower elevator", "access card", "lower elevator card", "lower elevator access"];
    
    public string ExaminationDescription => "The card is embossed \"loowur elivaatur akses kard.\"";

    public string ReadDescription => ExaminationDescription;

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a lower elevator access card here. ";
    }
    
    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A lower elevator access card";
    }
}