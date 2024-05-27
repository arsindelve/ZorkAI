namespace ZorkOne.Item;

public class TrunkOfJewels : ItemBase, ICanBeTakenAndDropped, IGivePointsWhenPlacedInTrophyCase,
    IGivePointsWhenFirstPickedUp
{
    public override string[] NounsForMatching => ["trunk", "jewels", "trunk of jewels"];

    public override string InInventoryDescription => "A trunk of jewels";

    public string OnTheGroundDescription => "There is a trunk of jewels here. ";

    public override string NeverPickedUpDescription =>
        "Lying half buried in the mud is an old trunk, bulging with jewels. ";

    int IGivePointsWhenFirstPickedUp.NumberOfPoints => 15;

    int IGivePointsWhenPlacedInTrophyCase.NumberOfPoints => 5;

    public override int Size => 9; 
}