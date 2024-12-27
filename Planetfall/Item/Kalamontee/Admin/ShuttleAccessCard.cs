namespace Planetfall.Item.Kalamontee.Admin;

public class ShuttleAccessCard : AccessCard, ICanBeExamined, ICanBeTakenAndDropped, ICanBeRead
{
    public override string[] NounsForMatching =>
        ["shuttle access card", "card", "access card", "shuttle", "shuttle card", "shuttle access"];

    public string ExaminationDescription => "The card is embossed \"shutul akses kard. \"";

    public string ReadDescription => ExaminationDescription;

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a shuttle access card here. ";
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A shuttle access card";
    }
}