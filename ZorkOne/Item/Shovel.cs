namespace ZorkOne.Item;

public class Shovel : ItemBase, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching => ["shovel"];
    
    public string OnTheGroundDescription => "There is a shovel here. ";

    public override string NeverPickedUpDescription => OnTheGroundDescription;

    public override string InInventoryDescription => "A shovel";

    public override int Size => 3;
}