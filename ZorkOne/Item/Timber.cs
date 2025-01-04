using GameEngine.Item;

namespace ZorkOne.Item;

public class Timber : ItemBase, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching => ["timber", "broken timber"];

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a broken timber here. ";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return OnTheGroundDescription(currentLocation);
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A broken timber";
    }
}