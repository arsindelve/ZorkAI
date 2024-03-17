namespace ZorkOne.Item;

public class Bottle : OpenAndCloseContainerBase, ICanBeTakenAndDropped, ICanBeExamined
{
    protected override int SpaceForItems => 0;

    public override bool IsTransparent => true;

    public override string[] NounsForMatching => ["bottle", "glass bottle"];

    public override string InInventoryDescription => Items.Any()
        ? "A glass bottle" + Environment.NewLine + ItemListDescription("glass bottle")
        : "A glass bottle";

    public string ExaminationDescription => !Items.Any()
        ? "The glass bottle is empty."
        : Environment.NewLine + ItemListDescription("glass bottle");

    string ICanBeTakenAndDropped.OnTheGroundDescription => !HasEverBeenOpened && !HasEverBeenPickedUp
        ? NeverPickedUpDescription
        : Items.Any()
            ? "There is a glass bottle here." + Environment.NewLine + ItemListDescription("glass bottle")
            : "There is a glass bottle here.";

    public override string NeverPickedUpDescription => "A bottle is sitting on the table." + Environment.NewLine +
                                                       ItemListDescription("glass bottle");

    public override void Init()
    {
        StartWithItemInside<Water>();
    }
}