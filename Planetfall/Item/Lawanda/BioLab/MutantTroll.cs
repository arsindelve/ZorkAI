namespace Planetfall.Item.Lawanda.BioLab;

public class MutantTroll : MutantBase
{
    protected override string[] SpecificNouns => ["troll", "mutant troll"];

    public override string ExaminationDescription =>
        "A massive, mutated troll with greenish skin and bulging muscles. " +
        "It growls threateningly. ";
}
