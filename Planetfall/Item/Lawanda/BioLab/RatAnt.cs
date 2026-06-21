namespace Planetfall.Item.Lawanda.BioLab;

public class RatAnt : ItemBase, ICanBeExamined
{
    // The original Planetfall source gives the mutant creatures MONSTER as a synonym, so during
    // the Bio Lab chase a panicked player typing "attack monster"/"kill monster" resolves here.
    public override string[] NounsForMatching => ["rat-ant", "ratant", "rat ant", "monster"];

    public override int Size => 100; // Can't be taken

    public string ExaminationDescription =>
        "A grotesque hybrid creature combining the worst features of a rat and an ant. " +
        "It chitters menacingly with mandibles clicking. ";
}
