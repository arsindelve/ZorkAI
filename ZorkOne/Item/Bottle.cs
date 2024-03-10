using Game.Item;
using Model.Item;

namespace ZorkOne.Item;

public class Bottle : OpenAndCloseContainerBase, ICanBeTakenAndDropped, ICanBeExamined
{
    public Bottle()
    {
        StartWithItemInside<Water>();
    }

    protected override int SpaceForItems => 0;

    public override string[] NounsForMatching => ["bottle", "glass bottle"];

    public override string InInventoryDescription => Items.Any()
        ? "A glass bottle" + Environment.NewLine + ItemListDescription("glass bottle")
        : "A glass bottle";

    public string ExaminationDescription => InInventoryDescription;

    string ICanBeTakenAndDropped.OnTheGroundDescription => !HasEverBeenOpened && !HasEverBeenPickedUp
        ? "A bottle is sitting on the table." + Environment.NewLine + ItemListDescription("glass bottle")
        : Items.Any()
            ? "There is a glass bottle here." + Environment.NewLine + ItemListDescription("glass bottle")
            : "There is a glass bottle here.";
}