using GameEngine.Item;

namespace ZorkOne.Item;

public class Shovel : ItemBase, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching => ["shovel"];

    public override int Size => 3;

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a shovel here. ";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return OnTheGroundDescription(currentLocation);
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A shovel";
    }
}