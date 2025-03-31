namespace Planetfall.Item.Lawanda.Library;

public class BrownSpool : SpoolBase
{
    public override string[] NounsForMatching => ["brown spool", "spool", "brown"];

    // You can NEVER get the spool to the library. You die first 
    public override string Contents => throw new NotImplementedException();

    public override string ExaminationDescription => "The spool is labelled \"Instrukshunz foor Reepaareeng Reepaar Roobots.\"";

    public override string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a brown spool here. ";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return "Sitting on a long table is a small brown spool. ";
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A brown spool. ";
    }
}