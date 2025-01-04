namespace Planetfall.Item.Feinstein;

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