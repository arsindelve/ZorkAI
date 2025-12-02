namespace Planetfall.Item.Lawanda.Library;

public class SpoolReader : ContainerBase, ICanBeExamined, ICanBeTakenAndDropped, ICanContainItems, ICanBeRead
{
    public override string[] NounsForMatching => ["spool reader", "reader", "screen"];

    public string ExaminationDescription => "The machine has a small screen, and below that, a small circular opening. " + (Items.Any()
        ?  "The screen is currently displaying some information: " +
          ReadDescription
        : "The screen is currently blank. ");
    
    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return NeverPickedUpDescription(currentLocation);
    }

    protected override int SpaceForItems => 1;

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return "There is a microfilm reader on one of the tables. " +
               (Items.Any() ? $"\n{ItemListDescription("microfilm reader", null)}" : "");
    }

    public override bool IsTransparent => true;

    public override string ItemPlacedHereResult(IItem item, IContext context)
    {
        return "The spool fits neatly into the opening. Some information appears on the screen. ";
    }

    public override string CanOnlyHoldTheseTypesErrorMessage(string nameOfItemWeTriedToPlace) => "It doesn't fit in the circular opening. ";
    
    public override Type[] CanOnlyHoldTheseTypes => [typeof(GreenSpool), typeof(RedSpool), typeof(BrownSpool)];

    public override string NoRoomMessage => "There's already a spool in the reader. ";

    public override void Init()
    {
    }

    public string ReadDescription => Items.OfType<SpoolBase>().Any()
        ? Items.OfType<SpoolBase>().Single().Contents
        : "The screen is blank.";
}