using GameEngine.Item;
using Model.Item;

namespace EscapeRoom.Item;

public class BrassKey : ItemBase, ICanBeTakenAndDropped, ICanBeExamined, IGivePointsWhenFirstPickedUp
{
    public int NumberOfPoints => 30;
    public override string[] NounsForMatching => ["brass key", "key", "small key", "exit key"];

    public override int Size => 1;

    string ICanBeExamined.ExaminationDescription => "A small brass key with 'EXIT' engraved on it. ";

    string ICanBeTakenAndDropped.OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a small brass key here. ";
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A brass key";
    }
}
