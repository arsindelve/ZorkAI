using Game.Item;
using Model.Item;

namespace ZorkOne.Item;

public class Sword : ItemBase, ICanBeExamined, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching => ["sword"];

    public override string InInventoryDescription => "A sword";

    public string ExaminationDescription => "There's nothing special about the sword.";

    public string OnTheGroundDescription => "There is a sword here";

    public override string NeverPickedUpDescription =>
        "Above the trophy case hangs an elvish sword of great antiquity.";
}