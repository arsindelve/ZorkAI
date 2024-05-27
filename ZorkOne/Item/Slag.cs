using Model.Interface;

namespace ZorkOne.Item;

public class Slag : ItemBase, ICanBeTakenAndDropped, ICanBeExamined
{
    public override string[] NounsForMatching => ["slag", "vitreous slag", "small piece of vitreous slag"];
    
    // Can never exist anywhere but in the machine
    public string OnTheGroundDescription => "";
    
    public string ExaminationDescription => "The slag was rather insubstantial, and crumbles to dust at your touch. ";

    public override string CannotBeTakenDescription => ExaminationDescription;

    public override void OnFailingToBeTaken(IContext context)
    {
        Repository.GetItem<Slag>().CurrentLocation = null;
        Repository.GetItem<Machine>().Items.Clear();
    }

    public override void OnBeingExamined(IContext context)
    {
        OnFailingToBeTaken(context);
    }
}