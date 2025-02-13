namespace Planetfall.Item.Feinstein;

internal class ComputerOutput : ItemBase, ICanBeTakenAndDropped, ICanBeExamined, ICanBeRead
{
    public override string[] NounsForMatching => ["output", "computer output"];
    public string ExaminationDescription => ReadDescription;

    public string ReadDescription => """
                                     The printout is hundreds of pages long. It would take many chrons to read it all. The last page looks pretty interesting, though:

                                     "Daalee Statis Reeport:
                                     PREELIMINEREE REESURC:  100.000%
                                     INTURMEEDEEIT REESURC:  100.000%
                                     FIINUL REESURC:         100.000%
                                     DRUG PROODUKSHUN:       100.000%
                                     DRUG TESTEENG:           99.985%
                                     Proojektid tiim tuu reeviivul prooseedzur:  0 daaz, 0.8 kronz


                                     *** ALURT! ALURT! ***
                                     Malfunkshun in Sekshun 384! Sumuneeng reepaar roobot."

                                     The printout ends at this point.
                                     """;

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a pile of computer output here. ";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return OnTheGroundDescription(currentLocation);
    }


    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A pile of computer output";
    }
}