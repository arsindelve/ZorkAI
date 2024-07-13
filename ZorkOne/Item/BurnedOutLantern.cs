namespace ZorkOne.Item;

public class BurnedOutLantern : ItemBase, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching => ["lantern", "burned-out", "burned-out lantern", "useless", "useless lantern"];
    
    public string OnTheGroundDescription => "There is a burned-out lantern here. ";

    public override string NeverPickedUpDescription => "The deceased adventurer's useless lantern is here. ";

    public override string InInventoryDescription => "A burned-out lantern";
}