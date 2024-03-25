using ZorkOne.Interface;

namespace ZorkOne.Item;

// TODO: The sword needs to glow near danger 

public class Sword : ItemBase, ICanBeExamined, ICanBeTakenAndDropped, IWeapon
{
    public override string[] NounsForMatching => ["sword", "glamdring", "orcrist"];

    public override string InInventoryDescription => "A sword";

    public override int Size => 4;

    public string ExaminationDescription => "There's nothing special about the sword.";

    public string OnTheGroundDescription => "There is a sword here. ";

    public override string NeverPickedUpDescription =>
        "Above the trophy case hangs an elvish sword of great antiquity.";
}