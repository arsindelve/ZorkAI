namespace Planetfall;

public class PlanetfallContext : Context<PlanetfallGame>, ITimeBasedContext
{
   public int CurrentTime => Repository.GetItem<Chronometer>().CurrentTime;

   public int Day { get; set; } = 1;

   public override void Init()
   {
      StartWithItem<Brush>(this);
      StartWithItem<Diary>(this);
      StartWithItem<Chronometer>(this);
   }

   public override string? ProcessEndOfTurn()
   {
      Repository.GetItem<Chronometer>().CurrentTime += 54;
      return base.ProcessEndOfTurn();
   }

   public override string CurrentScore => $"Your score would be {Score} (out of 80 points). It is Day {Day} of your adventure. Current Galactic Standard Time (adjusted to your local day-cycle) is {CurrentTime}. This score gives you the rank of {Game.GetScoreDescription(Score)}. ";

   public string CurrentTimeResponse => $"According to the chronometer, the current time is {CurrentTime}. ";
}