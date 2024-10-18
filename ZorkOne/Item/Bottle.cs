using GameEngine.Item;

namespace ZorkOne.Item;

public class Bottle : OpenAndCloseContainerBase, ICanBeTakenAndDropped, ICanBeExamined
{
    protected override int SpaceForItems => 0;

    public override bool IsTransparent => true;

    public override string[] NounsForMatching => ["bottle", "glass bottle"];

    public override int Size => 3;

    public string ExaminationDescription => !Items.Any()
        ? "The glass bottle is empty."
        : Environment.NewLine + ItemListDescription("glass bottle", null);

    string ICanBeTakenAndDropped.OnTheGroundDescription(ILocation currentLocation)
    {
        return !HasEverBeenOpened && !HasEverBeenPickedUp
            ? NeverPickedUpDescription(currentLocation)
            : Items.Any()
                ? "There is a glass bottle here." + Environment.NewLine +
                  ItemListDescription("glass bottle", currentLocation)
                : "There is a glass bottle here.";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return "A bottle is sitting on the table." + Environment.NewLine +
               ItemListDescription("glass bottle", null);
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return Items.Any()
            ? "A glass bottle" + Environment.NewLine + ItemListDescription("glass bottle", currentLocation)
            : "A glass bottle";
    }

    public override void Init()
    {
        StartWithItemInside<Water>();
    }
}