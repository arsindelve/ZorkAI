namespace ZorkOne.Item;

public class Screwdriver : ItemBase, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching => ["screwdriver"];

    public string OnTheGroundDescription => "There is a screwdriver here. ";

    public override string InInventoryDescription => "A screwdriver";

    public override string NeverPickedUpDescription => OnTheGroundDescription;

    public override int Size => 1;
}