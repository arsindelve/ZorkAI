using GameEngine.Item;

namespace ZorkOne.Item;

public class SkeletonKey : ItemBase, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching => ["key", "skeleton", "skeleton key"];

    public override string GenericDescription(ILocation currentLocation) => "A skeleton key";

    public string OnTheGroundDescription(ILocation currentLocation) => "There is a skeleton key here. ";

    public override string NeverPickedUpDescription(ILocation currentLocation) => OnTheGroundDescription(currentLocation);
}