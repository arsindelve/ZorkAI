namespace Planetfall.Item.Lawanda.BioLab;

public class Triffid : MutantBase
{
    protected override string[] SpecificNouns => ["triffid", "plant"];

    public override string ExaminationDescription =>
        "A mobile carnivorous plant with writhing tentacles and a poisonous sting. " +
        "It makes a distinctive rattling sound. ";
}
