using ZorkOne.Interface;

namespace ZorkOne.Item;

public class PotOfGold :  ItemBase, ICanBeExamined, ICanBeTakenAndDropped, IGivePointsWhenPlacedInTrophyCase
{
    public override string[] NounsForMatching => ["pot", "gold", "pot of gold"];

    public string ExaminationDescription => "There's nothing special about the pot of gold. ";

    public string OnTheGroundDescription => "There is a pot of gold here. ";

    public int NumberOfPoints => 10;
    
    public override string NeverPickedUpDescription =>
        "At the end of the rainbow is a pot of gold. ";
}