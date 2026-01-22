namespace Planetfall.Item.Lawanda.BioLab;

public class Triffid : ItemBase, ICanBeExamined
{
    public override string[] NounsForMatching => ["triffid", "plant"];

    public override int Size => 100; // Can't be taken

    public string ExaminationDescription =>
        "A mobile carnivorous plant with writhing tentacles and a poisonous sting. " +
        "It makes a distinctive rattling sound. ";
}
