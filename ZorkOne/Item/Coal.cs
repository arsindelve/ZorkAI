using GameEngine.Item;

namespace ZorkOne.Item;

public class Coal : ItemBase, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching => ["pile of coal", "coal", "pile", "small pile of coal"];

    public override int Size => 5;

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a small pile of coal here. ";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return OnTheGroundDescription(currentLocation);
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A small pile of coal ";
    }
}