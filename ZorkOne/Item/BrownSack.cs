namespace ZorkOne.Item;

public class BrownSack : OpenAndCloseContainerBase, ICanBeExamined, ICanBeTakenAndDropped
{
    // TODO: The description in inventory and on the ground are wrong.
    
    public override string[] NounsForMatching => ["sack", "brown sack"];

    public override string InInventoryDescription => IsOpen && Items.Any()
        ? "A brown sack" + Environment.NewLine + ItemListDescription("brown sack")
        : "A brown sack";

    public override string NowOpen =>
        !HasEverBeenOpened ? "Opening the brown sack reveals a lunch, and a clove of garlic." : "Opened.";

    public override string Name => "brown sack";

    public string ExaminationDescription =>
        ((IOpenAndClose)this).IsOpen ? InInventoryDescription : "The brown sack is closed.";

    string ICanBeTakenAndDropped.OnTheGroundDescription => !HasEverBeenOpened && !HasEverBeenPickedUp
        ? "On the table is an elongated brown sack, smelling of hot peppers. "
        : IsOpen && Items.Any()
            ? "There is a brown sack here." + Environment.NewLine + ItemListDescription("brown sack")
            : "There is a brown sack here.";

    public override string NeverPickedUpDescription => ((ICanBeTakenAndDropped)this).OnTheGroundDescription;

    public override void Init()
    {
        StartWithItemInside<Lunch>();
        StartWithItemInside<Garlic>();
    }
}