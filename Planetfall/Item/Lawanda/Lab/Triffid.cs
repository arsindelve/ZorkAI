namespace Planetfall.Item.Lawanda.Lab;

public class Triffid : ItemBase, ICanBeExamined
{
    public override string[] NounsForMatching => ["triffid", "plant", "mutant", "monster"];

    public string ExaminationDescription =>
        "A giant plant, teeming with poisonous tentacles, is shuffling toward you on three leg-like stalks. ";

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A mobile man-eating plant";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return ExaminationDescription;
    }
}
