using GameEngine.Item;
using Model.Interface;

namespace ZorkOne.Item;

public class Lunch : ItemBase, ICanBeTakenAndDropped, ICanBeExamined, ICanBeEaten
{
    public override string[] NounsForMatching => ["lunch", "sandwich"];

    public override string GenericDescription(ILocation? currentLocation) => "A lunch";

    public override int Size => 2;

    string ICanBeEaten.OnEating(IContext context)
    {
        return "Thank you very much. It really hit the spot. ";
    }

    string ICanBeExamined.ExaminationDescription => "There's nothing special about the lunch. ";

    string ICanBeTakenAndDropped.OnTheGroundDescription(ILocation currentLocation)
    {
        return "A hot pepper sandwich is here. ";
    }
}