namespace ZorkOne.Item;

public class BurnedOutLantern : ItemBase, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching => ["lantern", "busted", "busted lantern"];
    
    public string OnTheGroundDescription => "There is a burned-out lantern here. ";

    public override string NeverPickedUpDescription => "The deceased adventurer's useless lantern is here. ";
}