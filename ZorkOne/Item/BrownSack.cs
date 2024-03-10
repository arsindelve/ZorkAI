using Game.Item;
using Model.Item;

namespace ZorkOne.Item;

public class BrownSack : OpenAndCloseContainerBase, ICanBeExamined, ICanBeTakenAndDropped
{
    public BrownSack()
    {
        StartWithItemInside<Lunch>();
        StartWithItemInside<Garlic>();
    }

    public override string[] NounsForMatching => ["sack", "brown sack"];

    public override string InInventoryDescription => AmIOpen && Items.Any()
        ? "A brown sack" + Environment.NewLine + ItemListDescription("brown sack")
        : "A brown sack";

    public string ExaminationDescription =>
        ((IOpenAndClose)this).IsOpen ? InInventoryDescription : "The brown sack is closed.";

    string ICanBeTakenAndDropped.OnTheGroundDescription => !HasEverBeenOpened && !HasEverBeenPickedUp
        ? "On the table is an elongated brown sack, smelling of hot peppers"
        : AmIOpen && Items.Any()
            ? "There is a brown sack here." + Environment.NewLine + ItemListDescription("brown sack")
            : "There is a brown sack here.";

    public override string NeverPickedUpDescription => ((ICanBeTakenAndDropped)this).OnTheGroundDescription;

    public override string NowOpen =>
        !HasEverBeenOpened ? "Opening the brown sack reveals a lunch, and a clove of garlic." : "Opened.";


}