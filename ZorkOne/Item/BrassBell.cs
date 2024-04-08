namespace ZorkOne.Item;

public class BrassBell : ItemBase, ICanBeExamined, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching => ["bell", "brass bell"];

    public string ExaminationDescription => "There is nothing special about the brass bell";

    public string OnTheGroundDescription => "There is a brass bell here.";

    public override string InInventoryDescription => "A brass bell";

    public override string NeverPickedUpDescription => OnTheGroundDescription;
}