using GameEngine;
using GameEngine.Item;

namespace ZorkOne.Item;

// IAmPointyAndPunctureThings: the stiletto is one of the sharp items that punctures the
// inflatable boat on boarding (ZIL RBOAT-FUNCTION, zork1/1actions.zil:2787-2799).
public class Stiletto : ItemBase, ICanBeTakenAndDropped, IWeapon, IAmPointyAndPunctureThings
{
    public override string[] NounsForMatching => ["stiletto"];

    private Thief Thief => Repository.GetItem<Thief>();

    public override string CannotBeTakenDescription => Thief switch
    {
        { IsUnconscious: true } => "The stiletto seems white-hot. You can't hold on to it. ",
        { IsDead: true } => "",
        _ => "The thief swings it out of your reach. "
    };

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a stiletto here. ";
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A stiletto";
    }
}