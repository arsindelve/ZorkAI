namespace ZorkOne.Item;

public class CrystalSkull : ItemBase, ICanBeExamined, ICanBeTakenAndDropped, IGivePointsWhenPlacedInTrophyCase
{
    public override string[] NounsForMatching => ["skull", "crystal skull"];

    public override string InInventoryDescription => "A crystal skull";
    
    public string ExaminationDescription => "There's nothing special about the crystal skull. ";
    
    public string OnTheGroundDescription => "There is a crystal skull here. ";

    public override string NeverPickedUpDescription =>
        "Lying in one corner of the room is a beautifully carved crystal skull. It appears to be grinning at you rather nastily. ";

    public int NumberOfPoints => 10;
}