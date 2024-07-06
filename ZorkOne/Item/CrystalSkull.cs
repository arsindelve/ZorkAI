namespace ZorkOne.Item;

public class CrystalSkull : ItemBase, ICanBeTakenAndDropped, IGivePointsWhenPlacedInTrophyCase,
    IGivePointsWhenFirstPickedUp
{
    public override string[] NounsForMatching => ["skull", "crystal skull"];

    public override string InInventoryDescription => "A crystal skull";

    public string OnTheGroundDescription => "There is a crystal skull here. ";

    public override string NeverPickedUpDescription =>
        "Lying in one corner of the room is a beautifully carved crystal skull. It appears to be grinning at you rather nastily. ";

    int IGivePointsWhenFirstPickedUp.NumberOfPoints => 10;

    int IGivePointsWhenPlacedInTrophyCase.NumberOfPoints => 10;

    public override int Size => 1;
}