namespace ZorkOne.Item;

public class Coffin : OpenAndCloseContainerBase, ICanBeExamined, ICanBeTakenAndDropped,
    IGivePointsWhenPlacedInTrophyCase
{
    public override string[] NounsForMatching => ["coffin", "solid-gold coffin", "gold coffin"];

    public override string NowOpen => "The gold coffin opens. " + (Items.Contains(Repository.GetItem<Sceptre>())
        ? "A sceptre, possibly that of ancient Egypt itself, is in the coffin. The sceptre is ornamented with colored enamel, and tapers to a sharp point."
        : "");

    public override string InInventoryDescription =>
        !IsOpen ? "A gold coffin." : "A gold coffin " + (Items.Any() ? $"\n{ItemListDescription("gold coffin")}" : "");

    public string ExaminationDescription =>
        ((IOpenAndClose)this).IsOpen
            ? Items.Count == 1 && Items.Single() == Repository.GetItem<Sceptre>()
                ? "A sceptre, possibly that of ancient Egypt itself, is in the coffin. The sceptre is ornamented with colored enamel, and tapers to a sharp point."
                : ItemListDescription("gold coffin")
            : "The gold coffin is closed. ";

    public string OnTheGroundDescription => "The solid-gold coffin used for the burial of Ramses II is here. " +
                                            (IsOpen & Items.Any()
                                                ? $"\n{ItemListDescription("gold coffin")}"
                                                : "");

    public override string NeverPickedUpDescription => OnTheGroundDescription;

    int IGivePointsWhenPlacedInTrophyCase.NumberOfPoints => 15;

    public override void Init()
    {
        StartWithItemInside<Sceptre>();
    }
}