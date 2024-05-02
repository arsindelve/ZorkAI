namespace ZorkOne.Item;

// TODO: The sword needs to glow near danger 

public class Sword : ItemBase, ICanBeTakenAndDropped, IWeapon
{
    public override string[] NounsForMatching => ["sword", "glamdring", "orcrist"];

    public override string InInventoryDescription => "A sword";

    public override int Size => 4;

    public string OnTheGroundDescription => "There is a sword here. ";

    public override string NeverPickedUpDescription =>
        "Above the trophy case hangs an elvish sword of great antiquity.";
}