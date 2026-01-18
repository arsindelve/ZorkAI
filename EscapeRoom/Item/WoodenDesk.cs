using GameEngine.Item;

namespace EscapeRoom.Item;

public class WoodenDesk : OpenAndCloseContainerBase, ICanBeExamined
{
    public override string? CannotBeTakenDescription => "The desk is too heavy to move. ";

    public override string[] NounsForMatching => ["desk", "wooden desk", "drawer", "office desk"];

    public override string Name => "wooden desk";

    public string ExaminationDescription =>
        ((IOpenAndClose)this).IsOpen
            ? "A sturdy wooden desk with an open drawer. "
            : "A sturdy wooden desk with a closed drawer. ";

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "There is a wooden desk here. ";
    }

    public override string NowOpen(ILocation currentLocation)
    {
        return Items.Any() ? "Opening the desk drawer reveals a flashlight. " : "Opened. ";
    }

    public override void Init()
    {
        StartWithItemInside<Flashlight>();
    }
}
