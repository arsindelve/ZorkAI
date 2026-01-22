namespace Planetfall.Item.Lawanda.BioLab;

public class MutantGrue : ItemBase, ICanBeExamined
{
    public override string[] NounsForMatching => ["grue", "mutant grue"];

    public override int Size => 100; // Can't be taken

    public string ExaminationDescription =>
        "A sinister creature that lurks in shadows, with glowing eyes and sharp teeth. " +
        "Looking at it fills you with dread. ";
}
