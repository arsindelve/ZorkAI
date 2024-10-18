using GameEngine.Item;

namespace ZorkOne.Item;

public class BrownSack : OpenAndCloseContainerBase, ICanBeExamined, ICanBeTakenAndDropped, ISmell
{
    public override string[] NounsForMatching => ["sack", "brown sack", "bag"];

    public override string GenericDescription(ILocation? currentLocation) =>
        !IsOpen ? "A brown sack" : $"A brown sack \n {ItemListDescription("brown sack", null)}";

    public override string NowOpen(ILocation currentLocation) =>
        !HasEverBeenOpened ? "Opening the brown sack reveals a lunch, and a clove of garlic." : "Opened.";

    public override string Name => "brown sack";

    protected override int SpaceForItems => 4;

    string ICanBeExamined.ExaminationDescription =>
        ((IOpenAndClose)this).IsOpen ? ItemListDescription("brown sack", null) : "The brown sack is closed. ";

    string ICanBeTakenAndDropped.OnTheGroundDescription(ILocation currentLocation) => "There is a brown sack here. ";

    public override string NeverPickedUpDescription(ILocation currentLocation) => HasEverBeenOpened
        ? ((ICanBeTakenAndDropped)this).OnTheGroundDescription(currentLocation)
        : "On the table is an elongated brown sack, smelling of hot peppers. ";

    string ISmell.SmellDescription => "It smells of hot peppers. ";

    public override void Init()
    {
        StartWithItemInside<Lunch>();
        StartWithItemInside<Garlic>();
    }
}