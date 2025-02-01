using GameEngine;
using GameEngine.Item;

namespace ZorkOne.Item;

public class Stiletto : ItemBase, ICanBeTakenAndDropped, IWeapon
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