using GameEngine.Item;

namespace ZorkOne.Item;

public class Wrench : ItemBase, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching => ["wrench"];

    public override string GenericDescription(ILocation? currentLocation) => "A wrench";

    public string OnTheGroundDescription(ILocation currentLocation) => "There is a wrench here. ";

    public override string NeverPickedUpDescription(ILocation currentLocation) => OnTheGroundDescription(currentLocation);
}