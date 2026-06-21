namespace Planetfall.Item.Lawanda.BioLab;

public class RatAnt : MutantBase
{
    protected override string[] SpecificNouns => ["rat-ant", "ratant", "rat ant"];

    public override string ExaminationDescription =>
        "A grotesque hybrid creature combining the worst features of a rat and an ant. " +
        "It chitters menacingly with mandibles clicking. ";
}
