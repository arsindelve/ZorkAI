using GameEngine.Item;

namespace ZorkOne.Item;

public class Shovel : ItemBase, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching => ["shovel"];
    
    public string OnTheGroundDescription(ILocation currentLocation) => "There is a shovel here. ";

    public override string NeverPickedUpDescription(ILocation currentLocation) => OnTheGroundDescription(currentLocation);

    public override string GenericDescription(ILocation? currentLocation) => "A shovel";

    public override int Size => 3;
}