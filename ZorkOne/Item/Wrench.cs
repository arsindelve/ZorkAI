using GameEngine.Item;

namespace ZorkOne.Item;

public class Wrench : ItemBase, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching => ["wrench"];

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a wrench here. ";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return OnTheGroundDescription(currentLocation);
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A wrench";
    }
}