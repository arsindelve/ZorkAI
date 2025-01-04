using GameEngine.Item;

namespace ZorkOne.Item;

public class AirPump : ItemBase, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching => ["pump", "air pump", "hand-held air pump"];

    public override int Size => 5;

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a hand-held air pump here. ";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return OnTheGroundDescription(currentLocation);
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A hand-held air pump";
    }
}