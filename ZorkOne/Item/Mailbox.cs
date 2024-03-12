namespace ZorkOne.Item;

public class Mailbox : OpenAndCloseContainerBase, ICanBeExamined
{
    public Mailbox()
    {
        StartWithItemInside<Leaflet>();
    }

    public override string? CannotBeTakenDescription => "It is securely anchored";

    public override string InInventoryDescription => AmIOpen && Items.Any()
        ? "There is a small mailbox here.\n" + ItemListDescription("small mailbox")
        : "There is a small mailbox here.";

    public override string[] NounsForMatching => ["mailbox"];

    public override string NowOpen => Items.Any() ? "Opening the small mailbox reveals a leaflet." : "Opened.";

    public string ExaminationDescription => ((IOpenAndClose)this).IsOpen ? "It's open" : "The small mailbox is closed.";
}