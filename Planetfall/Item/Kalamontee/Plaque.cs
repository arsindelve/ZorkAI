namespace Planetfall.Item.Kalamontee;

public class Plaque : ItemBase, ICanBeRead, ICanBeExamined
{
    public override string[] NounsForMatching => ["plaque"];

    public override string CannotBeTakenDescription => "Not bloody likely. ";

    public string ExaminationDescription => ReadDescription;

    public string ReadDescription =>
        "Xis stuneeng vuu uf xee Kalamontee Valee kuvurz oovur fortee skwaar miilz uf xat faamus tuurist spot. " +
        "Xee larj bildeeng at xee bend in xee Gulmaan Rivur iz xee formur pravincul kapitul bildeeng. ";
}