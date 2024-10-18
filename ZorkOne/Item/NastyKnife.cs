using GameEngine.Item;

namespace ZorkOne.Item;

public class NastyKnife : ItemBase, ICanBeTakenAndDropped, IWeapon
{
    public override string GenericDescription(ILocation? currentLocation) => "a nasty knife";

    public override string[] NounsForMatching => ["knife", "nasty knife", "nasty"];

    string ICanBeTakenAndDropped.OnTheGroundDescription(ILocation currentLocation) => "There is a nasty knife here. ";

    string ICanBeTakenAndDropped.NeverPickedUpDescription(ILocation currentLocation) =>
        "On a table is a nasty-looking knife. ";
}