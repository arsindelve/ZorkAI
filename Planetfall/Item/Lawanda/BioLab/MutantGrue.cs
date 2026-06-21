namespace Planetfall.Item.Lawanda.BioLab;

public class MutantGrue : ItemBase, ICanBeExamined
{
    // The original Planetfall source gives the mutant creatures MONSTER as a synonym, so during
    // the Bio Lab chase a panicked player typing "attack monster"/"kill monster" resolves here.
    public override string[] NounsForMatching => ["grue", "mutant grue", "monster"];

    public override int Size => 100; // Can't be taken

    public string ExaminationDescription =>
        "A sinister creature that lurks in shadows, with glowing eyes and sharp teeth. " +
        "Looking at it fills you with dread. ";
}
