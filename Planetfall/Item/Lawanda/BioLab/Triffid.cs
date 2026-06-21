namespace Planetfall.Item.Lawanda.BioLab;

public class Triffid : ItemBase, ICanBeExamined
{
    // The original Planetfall source gives the mutant creatures MONSTER as a synonym, so during
    // the Bio Lab chase a panicked player typing "attack monster"/"kill monster" resolves here.
    public override string[] NounsForMatching => ["triffid", "plant", "monster"];

    public override int Size => 100; // Can't be taken

    public string ExaminationDescription =>
        "A mobile carnivorous plant with writhing tentacles and a poisonous sting. " +
        "It makes a distinctive rattling sound. ";
}
