namespace ZorkOne.Item;

//TODO: The descriptions are not working, does not show items inside. 

public class TrophyCase : OpenAndCloseContainerBase, ICanBeExamined
{
    public override string[] NounsForMatching => ["case", "trophy case"];

    public override string CannotBeTakenDescription => "The trophy case is securely fastened to the wall.";

    public string ExaminationDescription => Items.Any() ? "" : "The trophy case is empty.";

    protected override int SpaceForItems => int.MaxValue;

    public override string Name => "trophy case";
}