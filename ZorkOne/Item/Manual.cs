namespace ZorkOne.Item;

public class Manual : ItemBase, ICanBeTakenAndDropped, ICanBeExamined, ICanBeRead
{
    public override string InInventoryDescription => "A ZORK owner's manual";

    public override string[] NounsForMatching => ["paper", "piece", "manual"];

    string ICanBeExamined.ExaminationDescription => ((ICanBeRead)this).ReadDescription;

    // TODO: Implement the full text
    string ICanBeRead.ReadDescription => "The piece of paper has some writing on it.";

    string ICanBeTakenAndDropped.OnTheGroundDescription => "There is a ZORK owner's manual here. ";

    public override string NeverPickedUpDescription => "Loosely attached to a wall is a small piece of paper. ";
}