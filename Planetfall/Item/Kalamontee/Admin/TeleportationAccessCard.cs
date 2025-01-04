namespace Planetfall.Item.Kalamontee.Admin;

public class TeleportationAccessCard : AccessCard, ICanBeExamined, ICanBeTakenAndDropped, ICanBeRead
{
    public override string[] NounsForMatching =>
    [
        "access card", "teleportation access card", "card", "teleportation", "teleportation card",
        "teleport access card", "teleport", "teleport card"
    ];

    public string ExaminationDescription => ReadDescription;

    public string ReadDescription => "The card is embossed \"teliportaashun akses kard.\"";

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a teleportation access card here. ";
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A teleportation access card";
    }
}