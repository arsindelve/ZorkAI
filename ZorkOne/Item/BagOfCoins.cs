namespace ZorkOne.Item;

public class BagOfCoins : ItemBase, ICanBeTakenAndDropped, ICanBeExamined, IGivePointsWhenFirstPickedUp,
    IGivePointsWhenPlacedInTrophyCase
{
    public override string[] NounsForMatching =>
        ["bag", "bag of coins", "coins", "leather", "leather bag", "leather bag of coins"];

    public override string InInventoryDescription => "A leather bag of coins";

    public string ExaminationDescription => "There are lots of coins in there. ";
    
    public string OnTheGroundDescription => "An old leather bag, bulging with coins, is here.";

    public override string NeverPickedUpDescription => OnTheGroundDescription;

    int IGivePointsWhenFirstPickedUp.NumberOfPoints => 10;

    int IGivePointsWhenPlacedInTrophyCase.NumberOfPoints => 5;
}