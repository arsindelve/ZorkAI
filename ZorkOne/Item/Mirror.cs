using GameEngine.Item;

namespace ZorkOne.Item;

public class Mirror : ItemBase
{
    public override string[] NounsForMatching => ["mirror"];

    public override string CannotBeTakenDescription => "The mirror is many times your size. Give up.";
}