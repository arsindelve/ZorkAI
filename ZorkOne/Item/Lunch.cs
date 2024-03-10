using Game.Item;
using Model.Item;

namespace ZorkOne.Item;

public class Lunch : ItemBase, ICanBeTakenAndDropped, ICanBeExamined, ICanBeEaten
{
    public override string[] NounsForMatching => ["lunch", "sandwich"];

    public override string InInventoryDescription => "A lunch";

    string ICanBeEaten.EatenDescription => "Thank you very much. It really hit the spot.";

    string ICanBeExamined.ExaminationDescription => "There's nothing special about the lunch.";

    string ICanBeTakenAndDropped.OnTheGroundDescription => "A hot pepper sandwich is here.";
}