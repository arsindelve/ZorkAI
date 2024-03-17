namespace ZorkOne.Item;

public class BrownSack : OpenAndCloseContainerBase, ICanBeExamined, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching => ["sack", "brown sack"];

    public override string InInventoryDescription => IsOpen 
        ? !Items.Any() ? "The brown sack is empty. " : ItemListDescription("brown sack")
        : "The brown sack is closed. ";

    public override string NowOpen =>
        !HasEverBeenOpened ? "Opening the brown sack reveals a lunch, and a clove of garlic." : "Opened.";

    public override string Name => "brown sack";

    public string ExaminationDescription =>
        ((IOpenAndClose)this).IsOpen ? InInventoryDescription : "The brown sack is closed.";

    string ICanBeTakenAndDropped.OnTheGroundDescription => "There is a brown sack here. ";

    public override string NeverPickedUpDescription => IsOpen ? ((ICanBeTakenAndDropped)this).OnTheGroundDescription :
        "On the table is an elongated brown sack, smelling of hot peppers. ";

    public override void Init()
    {
        StartWithItemInside<Lunch>();
        StartWithItemInside<Garlic>();
    }
}