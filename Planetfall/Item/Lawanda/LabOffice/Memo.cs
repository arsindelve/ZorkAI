namespace Planetfall.Item.Lawanda.LabOffice;

public class Memo : ItemBase, ICanBeTakenAndDropped, ICanBeRead, ICanBeExamined
{
    public override string[] NounsForMatching => ["memo", "note", "paper"];

    public override int Size => 1;

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "A memo lies here. ";
    }

    public string ExaminationDescription =>
        "It's a short memo describing the fungicide misting system controls. ";

    public string ReadDescription =>
        "MEMO: The fungicide misting system can be activated using the button marked 'FUNGICIDE'. " +
        "The system will run for approximately 50 turns. CAUTION: Wear protective breathing equipment " +
        "when entering the Bio Lab while the system is active. ";

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A memo";
    }
}
