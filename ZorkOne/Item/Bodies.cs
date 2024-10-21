using GameEngine.Item;

namespace ZorkOne.Item;

public class Bodies : ItemBase, IPluralNoun
{
    public override string[] NounsForMatching => ["bodies", "pile of bodies"];
}
