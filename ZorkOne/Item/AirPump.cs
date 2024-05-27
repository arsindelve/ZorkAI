namespace ZorkOne.Item;

public class AirPump : ItemBase, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching => ["pump", "air pump", "hand-held air pump"];

    public override string InInventoryDescription => "A hand-held air pump";
    
    public string OnTheGroundDescription => "There is a hand-held air pump here. ";

    public override string NeverPickedUpDescription => OnTheGroundDescription;

    public override int Size => 5;
}