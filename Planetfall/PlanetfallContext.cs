namespace Planetfall;

// TODO Wake up: 1800
// TODO >= Day 4 You wake after sleeping restlessly. You feel weak and listless.
// TODO Day 8 You wake feeling weak and worn-out. It will be an effort just to stand up.
// TODO Day 9 Unfortunately, you don't seem to have survived the night.


//TODO Day 2, 1850: You notice that you feel a bit weak and slightly flushed, but you're not sure why.
//TODO Day 3, 2250: You notice that you feel unusually weak, and you suspect that you have a fever.
//TODO Day 4, 2500: You are now feeling quite under the weather, not unlike a bad flu.
//TODO Day 5, 2700: Your fever seems to have gotten worse, and you're developing a bad headache.
//TODO Day 6, 3000: Your health has deteriorated further. You feel hot and weak, and your head is throbbing.
//TODO Day 7, 3000: You feel very, very sick, and have almost no strength left.
//TODO Day 8, 3000: You feel like you're on fire, burning up from the fever. You're almost too weak to move, and your brain is reeling from the pounding headache.

// TODO: If you sleep with the canteen open, the liquid evaporates. I think

// TODO > day 6, your inventory capacity is very small

public class PlanetfallContext : Context<PlanetfallGame>, ITimeBasedContext
{
    // ReSharper disable once MemberCanBePrivate.Global
    public int Day { get; set; } = 1;

    public override string CurrentScore =>
        $"Your score would be {Score} (out of 80 points). It is Day {Day} of your adventure. " +
        $"Current Galactic Standard Time (adjusted to your local day-cycle) is " +
        $"{(WearingWatch ? CurrentTime : "impossible to determine, since you're not wearing your chronometer")}. " +
        $"\nThis score gives you the rank of {Game.GetScoreDescription(Score)}. ";

    private bool WearingWatch =>
        Items.Contains(Repository.GetItem<Chronometer>()) && Repository.GetItem<Chronometer>().BeingWorn;

    public HungerLevel Hunger { get; set; } = HungerLevel.WellFed;

    public TiredLevel Tired { get; set; } = TiredLevel.WellRested;

    // ReSharper disable once MemberCanBePrivate.Global
    public int CurrentTime => Repository.GetItem<Chronometer>().CurrentTime;

    public string CurrentTimeResponse =>
        WearingWatch
            ? $"According to the chronometer, the current time is {CurrentTime}. "
            : "It's hard to say, since you've removed your chronometer. ";

    public override void Init()
    {
        StartWithItem<Brush>(this);
        StartWithItem<Diary>(this);
        StartWithItem<Chronometer>(this);
        StartWithItem<Uniform>(this);
    }

    public override string? ProcessEndOfTurn()
    {
        Repository.GetItem<Chronometer>().CurrentTime += 54;
        return base.ProcessEndOfTurn();
    }
}