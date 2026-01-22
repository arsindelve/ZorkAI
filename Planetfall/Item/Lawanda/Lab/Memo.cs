namespace Planetfall.Item.Lawanda.Lab;

public class Memo : ItemBase, ICanBeTakenAndDropped, ICanBeExamined, ICanBeRead
{
    public override string[] NounsForMatching => ["memo"];

    public override int Size => 1;

    public string ExaminationDescription => "It's a memo. ";

    public string ReadDescription =>
        "\"Memoo tuu awl lab pursunel: Duu tuu xe daanjuris naatshur uv xe biioo eksperiments, an eemurjensee sistum haz bin instawld. Xis sistum wud flud xe entiir Biioo Lab wic aa dedlee fungasiid. Propur preecawshunz shud bee taakin if xis sistum iz evur yuuzd.\" ";

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a memo here. ";
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A memo";
    }
}
