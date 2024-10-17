namespace ZorkOne.Item;

public class BrownSack : OpenAndCloseContainerBase, ICanBeExamined, ICanBeTakenAndDropped, ISmell
{
    public override string[] NounsForMatching => ["sack", "brown sack", "bag"];

    public override string InInventoryDescription =>
        !IsOpen ? "A brown sack" : $"A brown sack \n {ItemListDescription("brown sack")}";

    public override string NowOpen =>
        !HasEverBeenOpened ? "Opening the brown sack reveals a lunch, and a clove of garlic." : "Opened.";

    public override string Name => "brown sack";

    protected override int SpaceForItems => 4;

    string ICanBeExamined.ExaminationDescription =>
        ((IOpenAndClose)this).IsOpen ? ItemListDescription("brown sack") : "The brown sack is closed. ";

    string ICanBeTakenAndDropped.OnTheGroundDescription => "There is a brown sack here. ";

    public override string NeverPickedUpDescription => HasEverBeenOpened
        ? ((ICanBeTakenAndDropped)this).OnTheGroundDescription
        : "On the table is an elongated brown sack, smelling of hot peppers. ";

    string ISmell.SmellDescription => "It smells of hot peppers. ";

    public override void Init()
    {
        StartWithItemInside<Lunch>();
        StartWithItemInside<Garlic>();
    }
}