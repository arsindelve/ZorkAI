namespace ZorkOne.Item;

public class Emerald : ItemBase, ICanBeTakenAndDropped, IGivePointsWhenPlacedInTrophyCase,
    IGivePointsWhenFirstPickedUp
{
    public override string[] NounsForMatching => ["emerald", "large emerald"];
    
    public string OnTheGroundDescription => "There is a large emerald here. ";

    public override string InInventoryDescription => "A large emerald ";

    public override string NeverPickedUpDescription => OnTheGroundDescription;

    int IGivePointsWhenFirstPickedUp.NumberOfPoints => 5;

    int IGivePointsWhenPlacedInTrophyCase.NumberOfPoints => 10;
}