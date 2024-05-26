namespace ZorkOne.Item;

public class Coal : ItemBase, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching => ["pile of coal", "coal", "pile", "small pile of coal"];

    public override string InInventoryDescription => "A small pile of coal ";
    
    public string OnTheGroundDescription => "There is a small pile of coal here. ";

    public override string NeverPickedUpDescription => OnTheGroundDescription;

    public override int Size => 5;
}