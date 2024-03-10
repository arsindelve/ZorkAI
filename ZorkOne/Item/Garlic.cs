using Game.Item;
using Model.Item;

namespace ZorkOne.Item;

public class Garlic : ItemBase, ICanBeTakenAndDropped, ICanBeExamined, ICanBeEaten
{
    public override string[] NounsForMatching => ["clove of garlic", "garlic"];

    public override string InInventoryDescription => "A clove of garlic";

    string ICanBeEaten.EatenDescription =>
        "What the heck! You won't make friends this way, but nobody around here is too friendly anyhow. Gulp!";

    string ICanBeExamined.ExaminationDescription => "There's nothing special about the clove of garlic.";

    string ICanBeTakenAndDropped.OnTheGroundDescription => "There is a clove of garlic here.";
}