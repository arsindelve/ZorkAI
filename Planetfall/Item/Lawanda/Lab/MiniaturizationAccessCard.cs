namespace Planetfall.Item.Lawanda.Lab;

public class MiniaturizationAccessCard : Kalamontee.Admin.AccessCard, ICanBeExamined, ICanBeTakenAndDropped, ICanBeRead,
    IGivePointsWhenFirstPickedUp
{
    public override string[] NounsForMatching =>
    [
        "miniaturization access card", "miniaturization card", "card", "access card", "miniaturization",
        "miniaturization access", "mini card", "mini access card"
    ];

    public string ExaminationDescription => "The card is embossed \"minitcurizaashun akses kard.\"";

    public string ReadDescription => ExaminationDescription;

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a miniaturization access card here. ";
    }

    int IGivePointsWhenFirstPickedUp.NumberOfPoints => 1;

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A miniaturization access card";
    }
}
