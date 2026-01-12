using GameEngine.Item;

namespace ZorkOne.Item;

public class BrownSack : OpenAndCloseContainerBase, ICanBeExamined, ICanBeTakenAndDropped, ISmell
{
    public override string[] NounsForMatching => ["sack", "brown sack", "bag", "elongated brown sack"];

    public override string Name => "brown sack";

    protected override int SpaceForItems => 4;

    string ICanBeExamined.ExaminationDescription =>
        ((IOpenAndClose)this).IsOpen ? ItemListDescription("brown sack", null) : "The brown sack is closed. ";

    string ICanBeTakenAndDropped.OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a brown sack here. ";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return HasEverBeenOpened
            ? ((ICanBeTakenAndDropped)this).OnTheGroundDescription(currentLocation)
            : "On the table is an elongated brown sack, smelling of hot peppers. ";
    }

    string ISmell.SmellDescription => "It smells of hot peppers. ";

    public override string GenericDescription(ILocation? currentLocation)
    {
        return !IsOpen ? "A brown sack" : $"A brown sack\n{ItemListDescription("brown sack", null)}";
    }

    public override string NowOpen(ILocation currentLocation)
    {
        return !HasEverBeenOpened ? "Opening the brown sack reveals a lunch, and a clove of garlic." : "Opened.";
    }

    public override void Init()
    {
        StartWithItemInside<Lunch>();
        StartWithItemInside<Garlic>();
    }
}