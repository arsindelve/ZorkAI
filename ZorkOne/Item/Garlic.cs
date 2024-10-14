using GameEngine.Item;

namespace ZorkOne.Item;

public class Garlic : ItemBase, ICanBeTakenAndDropped, ICanBeEaten
{
    public override string[] NounsForMatching => ["clove of garlic", "garlic", "clove"];

    public override string GenericDescription(ILocation? currentLocation) => "A clove of garlic";

    public override int Size => 1;

    string ICanBeEaten.EatenDescription =>
        "What the heck! You won't make friends this way, but nobody around here is too friendly anyhow. Gulp!";

    string ICanBeTakenAndDropped.OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a clove of garlic here.";
    }
}