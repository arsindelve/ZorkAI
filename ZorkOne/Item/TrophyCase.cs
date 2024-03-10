using Game.Item;
using Model.Item;

namespace ZorkOne.Item;

public class TrophyCase : OpenAndCloseContainerBase, ICanBeExamined
{
    public override string[] NounsForMatching => ["case", "trophy case"];
    
    public string ExaminationDescription => Items.Any() ? "" : "The trophy case is empty.";
    
    public override string CannotBeTakenDescription => "The trophy case is securely fastened to the wall.";

}