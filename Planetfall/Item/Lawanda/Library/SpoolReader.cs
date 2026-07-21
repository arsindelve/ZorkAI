namespace Planetfall.Item.Lawanda.Library;

public class SpoolReader : ContainerBase, ICanBeExamined, ICanContainItems, ICanBeRead
{
    public override string[] NounsForMatching => ["spool reader", "reader", "screen"];

    public string ExaminationDescription => "The machine has a small screen, and below that, a small circular opening. " + (Items.Any()
        ?  "The screen is currently displaying some information: " +
          ReadDescription
        : "The screen is currently blank. ");

    protected override int SpaceForItems => 1;

    // Issue #401: the reader is a machine fixed on a table (ZIL SPOOL-READER has no TAKEBIT), so it
    // deliberately does NOT implement ICanBeTakenAndDropped - "take reader" must be refused, not pick
    // up the machine. Because fixed (non-takeable) items get their room text from GenericDescription
    // rather than NeverPickedUpDescription/OnTheGroundDescription (see LocationBase.GetItemDescriptions),
    // the on-the-table description lives here.
    public override string GenericDescription(ILocation? currentLocation)
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
    
    // BrownSpool is listed for completeness, but in practice it can never reach the reader: the
    // player dies of radiation poisoning before carrying it here from the Radiation Lab. Hence
    // BrownSpool.Contents is never read (and deliberately throws). See BrownSpool.cs for details.
    public override Type[] CanOnlyHoldTheseTypes => [typeof(GreenSpool), typeof(RedSpool), typeof(BrownSpool)];

    public override string NoRoomMessage => "There's already a spool in the reader. ";

    public override void Init()
    {
    }

    public string ReadDescription => Items.OfType<SpoolBase>().Any()
        ? Items.OfType<SpoolBase>().Single().Contents
        : "The screen is blank.";
}