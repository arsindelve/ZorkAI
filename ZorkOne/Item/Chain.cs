using GameEngine.Item;

namespace ZorkOne.Item;

internal class Chain : ItemBase, ICanBeExamined
{
    public override string[] NounsForMatching => ["chain", "heavy iron chain", "iron chain"];

    public override string CannotBeTakenDescription => "The chain is secure. ";

    public string ExaminationDescription => "The chain secures a basket within the shaft. ";
}
