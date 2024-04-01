namespace ZorkOne.Item;

public class Wrench : ItemBase, ICanBeTakenAndDropped, ICanBeExamined
{
    public override string[] NounsForMatching => ["wrench"];

    public string OnTheGroundDescription => "There is a wrench here. ";

    public string ExaminationDescription => "There's nothing special about the wrench. ";

    public override string InInventoryDescription => "A wrench";
}