using GameEngine.Item;

namespace ZorkOne.Item;

public class Timber : ItemBase, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching => ["timber", "broken timber"];

    public override string GenericDescription(ILocation? currentLocation) => "A broken timber";

    public string OnTheGroundDescription(ILocation currentLocation) => "There is a broken timber here. ";

    public override string NeverPickedUpDescription(ILocation currentLocation) => OnTheGroundDescription(currentLocation);
}