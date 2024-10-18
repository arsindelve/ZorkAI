using GameEngine.Item;

namespace ZorkOne.Item;

public class Mailbox : OpenAndCloseContainerBase, ICanBeExamined
{
    public override string? CannotBeTakenDescription => "It is securely anchored";

    public override string[] NounsForMatching => ["mailbox"];

    public override string Name => "mailbox";

    public string ExaminationDescription =>
        ((IOpenAndClose)this).IsOpen ? "It's open. " : "The small mailbox is closed. ";

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "There is a small mailbox here.";
    }

    public override string NowOpen(ILocation currentLocation)
    {
        return Items.Any() ? "Opening the small mailbox reveals a leaflet." : "Opened.";
    }

    public override void Init()
    {
        StartWithItemInside<Leaflet>();
    }
}