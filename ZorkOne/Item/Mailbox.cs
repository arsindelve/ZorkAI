using Game.Item;
using Model.Item;

namespace ZorkOne.Item;

public class Mailbox : OpenAndCloseContainerBase, ICanBeExamined
{
    public Mailbox()
    {
        StartWithItemInside<Leaflet>();
    }

    public override string? CannotBeTakenDescription => "It is securely anchored";

    public override string InInventoryDescription => AmIOpen && Items.Any()
        ? ItemListDescription("small mailbox")
        : "There is a small mailbox here.";

    public override string[] NounsForMatching => ["mailbox"];
    
    public string ExaminationDescription => ((IOpenAndClose)this).IsOpen ? "It's open" : "The small mailbox is closed.";
    
    public override string NowOpen => Items.Any() ? "Opening the small mailbox reveals a leaflet." : "Opened.";


}