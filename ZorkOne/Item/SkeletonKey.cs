using GameEngine.Item;

namespace ZorkOne.Item;

public class SkeletonKey : ItemBase, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching => ["key", "skeleton", "skeleton key"];

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a skeleton key here. ";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return OnTheGroundDescription(currentLocation);
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A skeleton key";
    }
}