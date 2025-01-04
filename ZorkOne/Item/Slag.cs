using GameEngine;
using GameEngine.Item;
using Model.Interface;

namespace ZorkOne.Item;

public class Slag : ItemBase, ICanBeTakenAndDropped, ICanBeExamined
{
    public override string[] NounsForMatching => ["slag", "vitreous slag", "small piece of vitreous slag"];

    public override string CannotBeTakenDescription => ExaminationDescription;

    public string ExaminationDescription => "The slag was rather insubstantial, and crumbles to dust at your touch. ";

    public override void OnBeingExamined(IContext context)
    {
        OnFailingToBeTaken(context);
    }

    // Can never exist anywhere but in the machine
    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "";
    }

    public override void OnFailingToBeTaken(IContext context)
    {
        Repository.GetItem<Slag>().CurrentLocation = null;
        Repository.GetItem<Machine>().Items.Clear();
    }
}