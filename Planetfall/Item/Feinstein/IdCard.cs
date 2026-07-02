namespace Planetfall.Item.Feinstein;

// Issue #211: deliberately NOT an AccessCard, even though the original's I-MAGNET can scramble the ID
// card too. Floyd.cs:395-397 already depends on IdCard not being an AccessCard (enumerates it alongside
// three AccessCard subtypes explicitly, by design), and there is no ID slot in this port for a scrambled
// ID to matter against - so the magnet's scramble list only covers the six slotted AccessCard subtypes.
public class IdCard : ItemBase, ICanBeTakenAndDropped, ICanBeExamined, ICanBeRead
{
    public override string[] NounsForMatching => ["card", "id", "id card"];

    public string ExaminationDescription => ReadDescription;

    public string ReadDescription => """
                                        "STELLAR PATROL
                                        Special Assignment Task Force
                                        ID Number:  6172-531-541"
                                     """;

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is an ID card here. ";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return OnTheGroundDescription(currentLocation);
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "An ID card";
    }
}