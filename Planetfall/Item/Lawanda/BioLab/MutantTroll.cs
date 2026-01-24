namespace Planetfall.Item.Lawanda.BioLab;

public class MutantTroll : ItemBase, ICanBeExamined
{
    public override string[] NounsForMatching => ["troll", "mutant troll"];

    public override int Size => 100; // Can't be taken

    public string ExaminationDescription =>
        "A massive, mutated troll with greenish skin and bulging muscles. " +
        "It growls threateningly. ";
}
