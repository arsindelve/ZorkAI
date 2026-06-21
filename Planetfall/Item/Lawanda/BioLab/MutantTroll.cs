namespace Planetfall.Item.Lawanda.BioLab;

public class MutantTroll : ItemBase, ICanBeExamined
{
    // The original Planetfall source gives the mutant creatures MONSTER as a synonym, so during
    // the Bio Lab chase a panicked player typing "attack monster"/"kill monster" resolves here.
    public override string[] NounsForMatching => ["troll", "mutant troll", "monster"];

    public override int Size => 100; // Can't be taken

    public string ExaminationDescription =>
        "A massive, mutated troll with greenish skin and bulging muscles. " +
        "It growls threateningly. ";
}
