using GameEngine.Item;

namespace ZorkOne.Item;

public class Buoy : OpenAndCloseContainerBase, ICanBeExamined, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching => ["buoy", "red buoy"];
    
    public override void Init()
    {
        StartWithItemInside<Emerald>();
    }
    
    public override string GenericDescription(ILocation? currentLocation) =>
        !IsOpen ? "A red buoy" : $"A red buoy \n {ItemListDescription("red buoy", null)}";

    public override string NowOpen(ILocation currentLocation) =>
        !HasEverBeenOpened ? "Opening the red buoy reveals a large emerald. " : "Opened.";

    public override int Size => 4;

    public string ExaminationDescription =>
        ((IOpenAndClose)this).IsOpen ? ItemListDescription("red buoy", null) : "The buoy is closed. ";

    string ICanBeTakenAndDropped.OnTheGroundDescription(ILocation currentLocation) =>
        "There is a red buoy here. " + (Items.Any() ? ItemListDescription("red buoy", null) : "");

    public override string NeverPickedUpDescription(ILocation currentLocation) => "There is a red buoy here (probably a warning). ";
}