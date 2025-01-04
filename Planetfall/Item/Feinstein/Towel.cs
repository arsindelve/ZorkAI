namespace Planetfall.Item.Feinstein;

public class Towel : ItemBase, ICanBeExamined, ICanBeRead, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching => ["towel"];
    public string ExaminationDescription => "A pretty ordinary towel. Something is written in its corner. ";

    public string ReadDescription => """
                                     "S.P.S. FEINSTEIN
                                     Escape Pod #42
                                      Don't Panic!"
                                     """;

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a towel here. ";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return OnTheGroundDescription(currentLocation);
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A towel";
    }
}