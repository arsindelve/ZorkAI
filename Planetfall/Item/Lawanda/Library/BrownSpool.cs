namespace Planetfall.Item.Lawanda.Library;

public class BrownSpool : SpoolBase
{
    public override string[] NounsForMatching => ["brown spool", "spool", "brown"];

    // Intentionally unreachable: the brown spool has no readable contents, by design.
    //
    // The spool starts in the Radiation Lab (RadiationLab.cs), where radiation poisoning kills
    // the player a few turns after entering (RadiationLab.Act increments SickTurnCounter and
    // triggers death on turn 7). You die long before you could carry the spool out and all the
    // way to the library's SpoolReader, so a brown spool never actually ends up in the reader
    // and Contents is never read (its only caller is SpoolReader.ReadDescription).
    //
    // This matches the original game. The ZIL spool reader handles only the green and red spools
    // (planetfall-source/comptwo.zil:1224-1244) and rejects anything else with "It doesn't fit
    // in the circular opening." (comptwo.zil:1245). There is no BROWN-TEXT global: the "repair
    // robot instructions" exist only as the spool's label (see ExaminationDescription below),
    // never as readable screen content. So there is deliberately nothing to return here.
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