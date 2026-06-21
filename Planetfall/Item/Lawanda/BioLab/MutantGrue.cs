namespace Planetfall.Item.Lawanda.BioLab;

public class MutantGrue : MutantBase
{
    protected override string[] SpecificNouns => ["grue", "mutant grue"];

    public override string ExaminationDescription =>
        "A sinister creature that lurks in shadows, with glowing eyes and sharp teeth. " +
        "Looking at it fills you with dread. ";
}
