using GameEngine.Item;

namespace ZorkOne.Item;

public class SkeletonKey : ItemBase, ICanBeTakenAndDropped, IAmATool
{
    // In the original Zork I source the key is (SYNONYM KEY) (ADJECTIVE SKELETON) — "skeleton" is
    // only an *adjective* of the key, never a bare noun. Claiming the bare noun "skeleton" made it
    // collide with the Skeleton (the bones) and produced a spurious disambiguation prompt for
    // "take skeleton" (issue #36). "skeleton key" still resolves here; bare "skeleton" means the bones.
    public override string[] NounsForMatching => ["key", "skeleton key"];

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