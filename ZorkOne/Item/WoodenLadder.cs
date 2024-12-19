using GameEngine.Item;

namespace ZorkOne.Item;

public class WoodenLadder : ItemBase
{
    public override string[] NounsForMatching => ["ladder", "wooden ladder"];

    public override string CannotBeTakenDescription => "You can't be serious. ";
}