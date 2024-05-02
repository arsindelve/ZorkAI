namespace ZorkOne.Item;

public class Garlic : ItemBase, ICanBeTakenAndDropped, ICanBeEaten
{
    public override string[] NounsForMatching => ["clove of garlic", "garlic", "clove"];

    public override string InInventoryDescription => "A clove of garlic";

    string ICanBeEaten.EatenDescription =>
        "What the heck! You won't make friends this way, but nobody around here is too friendly anyhow. Gulp!";

    string ICanBeTakenAndDropped.OnTheGroundDescription => "There is a clove of garlic here.";
}