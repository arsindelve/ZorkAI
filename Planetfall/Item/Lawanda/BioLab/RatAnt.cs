namespace Planetfall.Item.Lawanda.BioLab;

public class RatAnt : ItemBase, ICanBeExamined
{
    public override string[] NounsForMatching => ["rat-ant", "ratant", "rat ant"];

    public override int Size => 100; // Can't be taken

    public string ExaminationDescription =>
        "A grotesque hybrid creature combining the worst features of a rat and an ant. " +
        "It chitters menacingly with mandibles clicking. ";
}
