namespace ZorkOne.Item;

public class Buoy : OpenAndCloseContainerBase, ICanBeExamined, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching => ["buoy", "red buoy"];
    
    public override void Init()
    {
        StartWithItemInside<Emerald>();
    }
    
    public override string InInventoryDescription =>
        !IsOpen ? "A red buoy" : $"A red buoy \n {ItemListDescription("red buoy")}";

    public override string NowOpen =>
        !HasEverBeenOpened ? "Opening the red buoy reveals a large emerald. " : "Opened.";

    public override int Size => 4;

    public string ExaminationDescription =>
        ((IOpenAndClose)this).IsOpen ? ItemListDescription("red buoy") : "The buoy is closed. ";

    string ICanBeTakenAndDropped.OnTheGroundDescription =>
        "There is a red buoy here. " + (Items.Any() ? ItemListDescription("red buoy") : "");

    public override string NeverPickedUpDescription => "There is a red buoy here (probably a warning). ";
}