namespace Planetfall.Item.Lawanda.Lab;

public class Troll : ItemBase, ICanBeExamined
{
    public override string[] NounsForMatching => ["troll", "biped", "mutant", "monster", "humanoid"];

    public string ExaminationDescription =>
        "Rushing toward you is an ugly, deformed humanoid, bellowing in a guttural tongue. It brandishes a piece of lab equipment shaped somewhat like a battle axe. ";

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A hairy growling biped";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return ExaminationDescription;
    }
}
