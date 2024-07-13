namespace ZorkOne.Item;

public class RustyKnife : ItemBase, ICanBeTakenAndDropped, IWeapon
{
    public override string InInventoryDescription => "a rusty knife";

    public override string[] NounsForMatching => ["knife", "rusty knife", "rusty"];

    string ICanBeTakenAndDropped.OnTheGroundDescription => "TThere is a rusty knife here. ";

    string ICanBeTakenAndDropped.NeverPickedUpDescription =>
        "Beside the skeleton is a rusty knife. ";
}