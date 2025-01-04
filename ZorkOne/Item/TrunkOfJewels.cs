using GameEngine.Item;

namespace ZorkOne.Item;

public class TrunkOfJewels
    : ItemBase,
        ICanBeTakenAndDropped,
        IGivePointsWhenPlacedInTrophyCase,
        IGivePointsWhenFirstPickedUp,
        ICanBeExamined,
        IPluralNoun
{
    public override string[] NounsForMatching => ["trunk", "jewels", "trunk of jewels"];

    public override int Size => 7;

    public string ExaminationDescription => "There are lots of jewels in there. ";

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a trunk of jewels here. ";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return "Lying half buried in the mud is an old trunk, bulging with jewels. ";
    }

    int IGivePointsWhenFirstPickedUp.NumberOfPoints => 15;

    int IGivePointsWhenPlacedInTrophyCase.NumberOfPoints => 5;

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A trunk of jewels";
    }
}