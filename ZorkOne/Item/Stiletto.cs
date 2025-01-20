using GameEngine;
using GameEngine.Item;

namespace ZorkOne.Item;

public class Stiletto : ItemBase
{
    public override string[] NounsForMatching => ["stiletto"];
    
    private Thief Thief => Repository.GetItem<Thief>();
    
    public override string CannotBeTakenDescription => Thief is { IsUnconscious: true, IsDead: false }
        ? "The stiletto seems white-hot. You can't hold on to it. "
        : "The thief swings it out of your reach. ";

}