using GameEngine.Item;

namespace EscapeRoom.Item;

public class CardboardBox : OpenAndCloseContainerBase, ICanBeExamined
{
    public override string? CannotBeTakenDescription => "The box is too bulky to carry around. ";

    public override string[] NounsForMatching => ["box", "cardboard box", "cardboard"];

    public override string Name => "cardboard box";

    public string ExaminationDescription =>
        ((IOpenAndClose)this).IsOpen
            ? "A dusty cardboard box, now open. "
            : "A dusty cardboard box. ";

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "There is a cardboard box here. ";
    }

    public override string NowOpen(ILocation currentLocation)
    {
        return Items.Any() ? "Opening the cardboard box reveals a brass key. " : "Opened. ";
    }

    public override void Init()
    {
        StartWithItemInside<BrassKey>();
    }
}
