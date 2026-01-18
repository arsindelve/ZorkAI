using GameEngine.Item;

namespace EscapeRoom.Item;

public class WelcomeDesk : OpenAndCloseContainerBase, ICanBeExamined
{
    public override string? CannotBeTakenDescription => "The desk is too heavy to move. ";

    public override string[] NounsForMatching => ["desk", "welcome desk", "drawer"];

    public override string Name => "welcome desk";

    public string ExaminationDescription =>
        ((IOpenAndClose)this).IsOpen
            ? "A welcome desk with an open drawer. "
            : "A welcome desk with a closed drawer. ";

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "There is a welcome desk here. ";
    }

    public override string NowOpen(ILocation currentLocation)
    {
        return Items.Any() ? "Opening the desk drawer reveals a leaflet. " : "Opened. ";
    }

    public override void Init()
    {
        StartWithItemInside<Leaflet>();
    }
}
