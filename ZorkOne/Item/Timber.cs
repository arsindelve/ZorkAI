namespace ZorkOne.Item;

public class Timber : ItemBase, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching => ["timber", "broken timber"];
    
    public string OnTheGroundDescription => "There is a broken timber here. ";

    public override string InInventoryDescription => "A broken timber";

    public override string NeverPickedUpDescription => OnTheGroundDescription;
}