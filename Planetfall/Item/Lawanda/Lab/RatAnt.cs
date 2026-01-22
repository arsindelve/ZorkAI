namespace Planetfall.Item.Lawanda.Lab;

public class RatAnt : ItemBase, ICanBeExamined
{
    public override string[] NounsForMatching => ["rat-ant", "monster", "mutant", "rat", "ant"];

    public string ExaminationDescription =>
        "A ferocious feral creature, with a hairy shelled body and a whip-like tail snaps its enormous mandibles at you. ";

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A rat-like, ant-like man-sized monster";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return ExaminationDescription;
    }
}
