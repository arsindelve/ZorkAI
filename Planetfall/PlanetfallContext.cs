using Planetfall.Item;
using Planetfall.Item.Feinstein;

namespace Planetfall;

public class PlanetfallContext: Context<PlanetfallGame>
{
    public override void Init()
    {
       StartWithItem<Brush>(this);
    }
    
    public override string CurrentScore => $"""
                                           Your score would be {Score} (out of 80 points). It is Day 1 of your adventure. Current Galactic Standard Time (adjusted to your local day-cycle) is 4679.                                                 
                                           This score gives you the rank of {Game.GetScoreDescription(Score)}.  
                                           """;
}