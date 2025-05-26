using Planetfall.Item.Kalamontee.Admin;
using Utilities;

namespace Planetfall.Item.Lawanda.Lab;

internal class LabUniformPocket : OpenAndCloseContainerBase, ICanBeExamined
{
    public override string[] NounsForMatching => ["pocket", "lab pocket", "lab uniform pocket"];

    protected override int SpaceForItems => 1;

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