using GameEngine;
using GameEngine.Item;
using Model.Interface;

namespace ZorkOne.Item;

public class ToolChests : ItemBase, IPluralNoun, ICanBeExamined, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching => ["tool chest", "tool chests", "chests", "chest", "group of chests", "group of tool chests"];

    public override string CannotBeTakenDescription =>
        "The chests are so rusty and corroded that they crumble when you touch them. ";

    public string ExaminationDescription => "The chests are all empty. ";

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a group of tool chests here. ";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return OnTheGroundDescription(currentLocation);
    }

    public override void OnFailingToBeTaken(IContext context)
    {
        Repository.DestroyItem<ToolChests>();
    }
}