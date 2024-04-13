namespace ZorkOne.Item;

public class Wrench : ItemBase, ICanBeTakenAndDropped, ICanBeExamined
{
    public override string[] NounsForMatching => ["wrench"];

    public override string InInventoryDescription => "A wrench";

    public string ExaminationDescription => "There's nothing special about the wrench. ";

    public string OnTheGroundDescription => "There is a wrench here. ";

    public override string NeverPickedUpDescription => OnTheGroundDescription;
}