namespace ZorkOne.Item;

public class Spirits : ItemBase, ICanBeExamined, ITurnBasedActor
{
    public override string[] NounsForMatching => ["spirits, ghosts"];

    public string ExaminationDescription => "You seem unable to interact with these spirits. ";

    public override string CannotBeTakenDescription => ExaminationDescription;

    public string? Act(IContext context)
    {
        throw new NotImplementedException();
    }
}