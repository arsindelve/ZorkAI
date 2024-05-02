namespace ZorkOne.Item;

public class JadeFigurine : ItemBase, ICanBeExamined, ICanBeTakenAndDropped, IGivePointsWhenPlacedInTrophyCase,
    IGivePointsWhenFirstPickedUp
{
    public override string[] NounsForMatching => ["jade", "jade figurine", "figurine"];

    public override string InInventoryDescription => "A jade figurine";

    public string ExaminationDescription => "There's nothing special about the jade figurine. ";

    public string OnTheGroundDescription => "There is an exquisite jade figurine here. ";

    public override string NeverPickedUpDescription => OnTheGroundDescription;

    int IGivePointsWhenFirstPickedUp.NumberOfPoints => 5;

    int IGivePointsWhenPlacedInTrophyCase.NumberOfPoints => 5;
}