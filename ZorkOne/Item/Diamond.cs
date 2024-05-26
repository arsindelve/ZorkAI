namespace ZorkOne.Item;

public class Diamond : ItemBase, ICanBeTakenAndDropped, IGivePointsWhenPlacedInTrophyCase,
    IGivePointsWhenFirstPickedUp
{
    public override string[] NounsForMatching => ["diamond", "huge diamond"];

    public override string InInventoryDescription => "A huge diamond";

    public string OnTheGroundDescription => "There is a huge diamond here. ";

    int IGivePointsWhenFirstPickedUp.NumberOfPoints => 10;

    int IGivePointsWhenPlacedInTrophyCase.NumberOfPoints => 10;
}