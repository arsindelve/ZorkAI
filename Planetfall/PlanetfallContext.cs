namespace Planetfall;

public class PlanetfallContext : Context<PlanetfallGame>, ITimeBasedContext
{
    public int CurrentTime => Repository.GetItem<Chronometer>().CurrentTime;

    public int Day { get; set; } = 1;

    public override string CurrentScore =>
        $"Your score would be {Score} (out of 80 points). It is Day {Day} of your adventure. " +
        $"Current Galactic Standard Time (adjusted to your local day-cycle) is " +
        $"{(WearingWatch ? CurrentTime : "impossible to determine, since you're not wearing your chronometer")}. " +
        $"\nThis score gives you the rank of {Game.GetScoreDescription(Score)}. ";

    private bool WearingWatch =>
        Items.Contains(Repository.GetItem<Chronometer>()) && Repository.GetItem<Chronometer>().BeingWorn;

    public string CurrentTimeResponse =>
        WearingWatch
            ? $"According to the chronometer, the current time is {CurrentTime}. "
            : "It's hard to say, since you've removed your chronometer. ";

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
}