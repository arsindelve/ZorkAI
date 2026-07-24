using Planetfall.Item.Kalamontee.Admin;
using Utilities;

namespace Planetfall.Item.Lawanda.Lab;

internal class LabUniformPocket : OpenAndCloseContainerBase, ICanBeExamined
{
    public override string[] NounsForMatching => ["pocket", "lab pocket", "lab uniform pocket"];

    // The pocket ships with two size-1 items (teleportation access card + piece of paper), so it
    // must have room for both. Inherit ContainerBase's default capacity of 2 - do NOT override this
    // to 1 (as the sibling PatrolUniformPocket does; that pocket only seeds a single item). A
    // capacity of 1 leaves the pocket unable to hold its own starting contents: take either item
    // out and it can't be put back. See issue #478.
    public override bool IsTransparent => false;

    public string ExaminationDescription => ItemListDescription("Lab uniform pocket", null);

    public override string NowOpen(ILocation currentLocation)
    {
        if (!Items.Any())
            return base.NowOpen(currentLocation);

        var itemNames = Items.Select(s => s.NounsForMatching.First()).ToList().SingleLineListWithAnd();
        return $"You discover {itemNames} in the pocket of the uniform. ";
    }

    public override void Init()
    {
        StartWithItemInside<TeleportationAccessCard>();
        StartWithItemInside<PieceOfPaper>();
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return ItemListDescription("", null);
    }
}