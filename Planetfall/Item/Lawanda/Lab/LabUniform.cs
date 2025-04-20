using Planetfall.Item.Kalamontee.Admin;
using Utilities;

namespace Planetfall.Item.Lawanda.Lab;

public class LabUniform : OpenAndCloseContainerBase, ICanBeTakenAndDropped, ICanBeExamined, IAmClothing
{
    public override string[] NounsForMatching => ["lab uniform", "uniform"];

    public override bool IsTransparent => true;

    // If the player tries to put something in this uniform, put it in the pocket instead.
    public override ICanContainItems ForwardingContainer => Repository.GetItem<LabUniformPocket>();

    public override int Size => 3;

    public bool BeingWorn { get; set; } = false;

    public string ExaminationDescription =>
        "It is a plain lab uniform. The logo above the pocket depicts a flame burning above some kind of sleep " +
        $"chamber. The pocket is {(Repository.GetItem<LabUniformPocket>().IsOpen ? "open" : "closed")}. " +
        (Repository.GetItem<LabUniformPocket>().Items.Any() && Repository.GetItem<LabUniformPocket>().IsOpen
            ? $"\n{ItemListDescription("lab uniform", null)}"
            : "");

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a lab uniform here. " +
               (Repository.GetItem<LabUniformPocket>().Items.Any() && Repository.GetItem<LabUniformPocket>().IsOpen
                   ? $"\n{ItemListDescription("lab uniform", null)}"
                   : "");
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return "Hanging on a rack is a pale blue lab uniform. Sewn onto its pocket is a nondescript logo. "
               + (Repository.GetItem<LabUniformPocket>().Items.Any() && Repository.GetItem<LabUniformPocket>().IsOpen
                   ? $"\n{ItemListDescription("lab uniform", null)}"
                   : "");
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A lab uniform";
    }

    public override void Init()
    {
        StartWithItemInside<LabUniformPocket>();
    }
}

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