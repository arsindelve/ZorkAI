namespace Planetfall.Item.Kalamontee.Admin;

public class UpperElevatorAccessCard : ElevatorAccessCard, ICanBeExamined, ICanBeTakenAndDropped, ICanBeRead,
    IGivePointsWhenFirstPickedUp
{
    public override string[] NounsForMatching =>
    [
        "upper elevator access card", "elevator access card", "card", "upper", "elevator", "upper card",
        "upper elevator", "access card", "upper elevator card", "upper elevator access"
    ];

    public string ExaminationDescription => "The card is embossed \"upur elivaatur akses kard.\"";

    public string ReadDescription => ExaminationDescription;

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is an upper elevator access card here. ";
    }

    int IGivePointsWhenFirstPickedUp.NumberOfPoints => 1;

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "An upper elevator access card";
    }
}