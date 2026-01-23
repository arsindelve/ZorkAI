namespace Planetfall.Item.Lawanda.LabOffice;

public class Memo : ItemBase, ICanBeTakenAndDropped, ICanBeRead, ICanBeExamined
{
    public override string[] NounsForMatching => ["memo", "note", "paper"];

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a memo here. ";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        // This is unusual - the memo is not made known to the player until they examine the desk, and only
        // via the desk description. 
        return string.Empty;
    }

    public string ExaminationDescription => ReadDescription;

    public string ReadDescription =>
        "Memoo tuu awl lab pursunel: Duu tuu xe daanjuris naatshur uv xe biioo eksperiments, an eemurjensee " +
        "sistum haz bin instawld. Xis\nsistum wud flud xe entiir Biioo Lab wic aa dedlee fungasiid. Propur " +
        "preecawshunz shud bee taakin if xis sistum iz evur yuuzd. ";

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A memo";
    }
}
