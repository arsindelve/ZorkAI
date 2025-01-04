using GameEngine.Item;

namespace ZorkOne.Item;

public class NastyKnife : ItemBase, ICanBeTakenAndDropped, IWeapon
{
    public override string[] NounsForMatching => ["knife", "nasty knife", "nasty"];

    string ICanBeTakenAndDropped.OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a nasty knife here. ";
    }

    string ICanBeTakenAndDropped.NeverPickedUpDescription(ILocation currentLocation)
    {
        return "On a table is a nasty-looking knife. ";
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "a nasty knife";
    }
}