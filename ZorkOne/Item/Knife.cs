namespace ZorkOne.Item;

public class Knife : ItemBase, ICanBeTakenAndDropped, IWeapon
{
    public override string InInventoryDescription => "a nasty knife";

    public override string[] NounsForMatching => ["knife", "nasty knife"];

    string ICanBeTakenAndDropped.OnTheGroundDescription => "There is a knife here. ";

    string ICanBeTakenAndDropped.NeverPickedUpDescription =>
        "On a table is a nasty-looking knife. ";
}