using GameEngine.Item;

namespace ZorkOne.Item;

public class Screwdriver : ItemBase, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching => ["screwdriver"];

    public override int Size => 2;

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a screwdriver here. ";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return OnTheGroundDescription(currentLocation);
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A screwdriver";
    }
}