namespace ZorkOne.Item;

public class NastyKnife : ItemBase, ICanBeTakenAndDropped, IWeapon
{
    public override string InInventoryDescription => "a nasty knife";

    public override string[] NounsForMatching => ["knife", "nasty knife"];

    string ICanBeTakenAndDropped.OnTheGroundDescription => "There is a nasty knife here. ";

    string ICanBeTakenAndDropped.NeverPickedUpDescription =>
        "On a table is a nasty-looking knife. ";
}