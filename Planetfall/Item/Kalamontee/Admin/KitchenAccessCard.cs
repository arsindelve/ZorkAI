namespace Planetfall.Item.Kalamontee.Admin;

public class KitchenAccessCard : AccessCard, ICanBeExamined, ICanBeTakenAndDropped, ICanBeRead,
    IGivePointsWhenFirstPickedUp
{
    public override string[] NounsForMatching =>
        ["kitchen access card", "card", "access card", "kitchen", "kitchen card", "kitchen access"];

    public string ExaminationDescription => "The card is embossed \"kitcin akses kard.\"";

    public string ReadDescription => ExaminationDescription;

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a kitchen access card here. ";
    }

    int IGivePointsWhenFirstPickedUp.NumberOfPoints => 1;

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A kitchen access card";
    }
}