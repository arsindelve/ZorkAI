namespace ZorkOne.Item;

public class SkeletonKey : ItemBase, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching => ["key", "skeleton", "skeleton key"];

    public override string InInventoryDescription => "A skeleton key";

    public string OnTheGroundDescription => "There is a skeleton key here. ";

    public override string NeverPickedUpDescription => OnTheGroundDescription;
}