using GameEngine.Item;

namespace ZorkOne.Item;

public class BagOfCoins
    : ItemBase,
        ICanBeTakenAndDropped,
        ICanBeExamined,
        IGivePointsWhenFirstPickedUp,
        IGivePointsWhenPlacedInTrophyCase,
        IPluralNoun
{
    public override string[] NounsForMatching =>
        ["bag", "bag of coins", "coins", "leather", "leather bag", "leather bag of coins"];

    public string ExaminationDescription => "There are lots of coins in there. ";

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "An old leather bag, bulging with coins, is here. ";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return OnTheGroundDescription(currentLocation);
    }

    int IGivePointsWhenFirstPickedUp.NumberOfPoints => 10;

    int IGivePointsWhenPlacedInTrophyCase.NumberOfPoints => 5;

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A leather bag of coins";
    }
}