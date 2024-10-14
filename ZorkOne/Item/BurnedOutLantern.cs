using GameEngine.Item;

namespace ZorkOne.Item;

public class BurnedOutLantern : ItemBase, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching => ["lantern", "burned-out", "burned-out lantern", "useless", "useless lantern"];
    
    public string OnTheGroundDescription(ILocation currentLocation) => "There is a burned-out lantern here. ";

    public override string NeverPickedUpDescription(ILocation currentLocation) => "The deceased adventurer's useless lantern is here. ";

    public override string GenericDescription(ILocation? currentLocation) => "A burned-out lantern";
}