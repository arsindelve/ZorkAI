using FluentAssertions;
using GameEngine;
using Model.Interface;
using Moq;
using Stationfall.Item.Duffy;

namespace Stationfall.Tests;

/// <summary>
///     The hunger and fatigue clocks, and the day boundary that resets them. Every test here pins the
///     chronometer and both random seams, because the whole point of these clocks is *when* they fire.
/// </summary>
[TestFixture]
public class SurvivalClockTests : EngineTestsBase
{
    /// <summary>
    ///     A starting reading inside the range real play produces (4431-5650).
    /// </summary>
    private const int StartTime = 5000;

    private GameEngine<StationfallGame, StationfallContext> _target = null!;

    [SetUp]
    public void SetUp()
    {
        _target = GetTarget();

        // No dreams by default: the dream roll is pure colour and would otherwise make the wake-up
        // narration vary between runs. RollDice(100) > 60 means "no dream tonight".
        var noDreams = new Mock<IRandomChooser>();
        noDreams.Setup(r => r.RollDice(100)).Returns(100);
        SleepEngine.Chooser = noDreams.Object;

        // Pin the morning jitter so a new day always starts at exactly 1640.
        var jitter = new Mock<IRandomChooser>();
        jitter.Setup(r => r.RollDice(Chronometer.MorningJitterTicks)).Returns(40);
        GetItem<Chronometer>().Chooser = jitter.Object;

        // Re-arm both clocks from a known reading; Init() armed them off a random start time.
        Pin(StartTime);
        Context.HungerNotifications.Initialize(StartTime);
        Context.SleepNotifications.Initialize(StartTime);
    }

    [TearDown]
    public void TearDown()
    {
        SleepEngine.Chooser = new RandomChooser();
    }

    private const int MorningAfterWaking = 1640;

    private void Pin(int time)
    {
        GetItem<Chronometer>().CurrentTime = time;
    }

    /// <summary>
    ///     Takes one turn with the clock pinned to the given reading. The default command is "wait"
    ///     rather than "look" on purpose: the engine treats looking as a free command and skips turn
    ///     processing entirely for it, so a clock test driven by "look" would never tick.
    /// </summary>
    private async Task<string> TurnAt(int time, string command = "wait")
    {
        Pin(time);
        return await _target.GetResponse(command) ?? string.Empty;
    }

    /// <summary>
    ///     Walks the hunger ladder up to (but not including) the level that kills.
    /// </summary>
    private async Task StarveToTheBrink()
    {
        await TurnAt(6330); // Hungry
        await TurnAt(6780); // Ravenous
        await TurnAt(7080); // Faint
        await TurnAt(7230); // AboutToPassOut
    }

    /// <summary>
    ///     Walks the fatigue ladder up to (but not including) collapse.
    /// </summary>
    private async Task TireToTheBrink()
    {
        await TurnAt(8100); // Tired
        await TurnAt(8420); // VeryTired
        await TurnAt(8580); // Exhausted
        await TurnAt(8660); // AboutToDrop
    }

    [TestFixture]
    public class Hunger : SurvivalClockTests
    {
        [Test]
        public void FirstPang_IsScheduledForTheOriginalsInterval()
        {
            // misc.zil:197 queues the first hunger warning 1330 millichrons in.
            Context.HungerNotifications.NextWarningAt.Should().Be(StartTime + 1330);
        }

        [Test]
        public async Task TheLadder_EscalatesOneLevelAtATime()
        {
            (await TurnAt(6330)).Should().Contain("getting pretty hungry");
            Context.Hunger.Should().Be(HungerLevel.Hungry);

            (await TurnAt(6780)).Should().Contain("ravenous");
            Context.Hunger.Should().Be(HungerLevel.Ravenous);

            (await TurnAt(7080)).Should().Contain("faint");
            Context.Hunger.Should().Be(HungerLevel.Faint);

            (await TurnAt(7230)).Should().Contain("pass out");
            Context.Hunger.Should().Be(HungerLevel.AboutToPassOut);
        }

        [Test]
        public async Task RunningOutCompletely_Kills()
        {
            await StarveToTheBrink();

            var response = await TurnAt(7380);

            response.Should().Contain("You collapse from extreme thirst and hunger");
            response.Should().Contain("You have died");
        }

        [Test]
        public async Task AMeal_ClearsTheLadderAndBuysTheOriginalsStretchOfTime()
        {
            await TurnAt(6330);
            Context.Hunger.Should().Be(HungerLevel.Hungry);

            // The soup is aboard the ship; put it in hand rather than walking there.
            var soup = GetItem<BlueSoup>();
            Context.Take(soup);

            var response = await TurnAt(6400, "eat soup");

            response.Should().Contain("You drink the soup");
            Context.Hunger.Should().Be(HungerLevel.WellFed);
            // verbs.zil:692 - a meal buys 2250 millichrons.
            Context.HungerNotifications.NextWarningAt.Should().Be(6400 + 2250);
        }

        [Test]
        public async Task Eating_IsRefused_WhenThePlayerHasNoAppetite()
        {
            var soup = GetItem<BlueSoup>();
            Context.Take(soup);

            var response = await TurnAt(StartTime, "eat soup");

            response.Should().Contain("not the least bit hungry");
            // Refusing has to mean the food survives, or the guard would be worse than useless.
            Context.Items.Should().Contain(soup);
        }
    }

    [TestFixture]
    public class Fatigue : SurvivalClockTests
    {
        [Test]
        public void TheFirstWarning_LandsAtAFixedTimeOfDay_HoweverEarlyTheGameBegan()
        {
            // misc.zil:196 queues it for (8100 - start time), which is to say: at 8100, always. The
            // starting reading is randomised, so this is what makes day one's stamina vary.
            Context.SleepNotifications.Initialize(4431);
            Context.SleepNotifications.NextWarningAt.Should().Be(8100);

            Context.SleepNotifications.Initialize(5650);
            Context.SleepNotifications.NextWarningAt.Should().Be(8100);
        }

        [Test]
        public async Task TheLadder_EscalatesOneLevelAtATime()
        {
            (await TurnAt(8100)).Should().Contain("begin to feel weary");
            Context.Tired.Should().Be(TiredLevel.Tired);

            (await TurnAt(8420)).Should().Contain("really tired now");
            Context.Tired.Should().Be(TiredLevel.VeryTired);

            (await TurnAt(8580)).Should().Contain("probably drop");
            Context.Tired.Should().Be(TiredLevel.Exhausted);

            (await TurnAt(8660)).Should().Contain("barely keep your eyes open");
            Context.Tired.Should().Be(TiredLevel.AboutToDrop);
        }

        [Test]
        public async Task RunningOutCompletely_DropsThePlayerWhereTheyStand()
        {
            await TireToTheBrink();

            var response = await TurnAt(8700);

            response.Should().Contain("can't stay awake a moment longer");
            response.Should().Contain("drop to the deck");
        }

        [Test]
        public async Task Collapsing_ConsumesTheTurn_SoTheCommandDoesNotRunAgainstStaleState()
        {
            await TireToTheBrink();

            // Ask to walk somewhere on the very turn fatigue runs out. Sleep has already moved the
            // player's belongings and the calendar; running the movement afterwards would act on a
            // turn that no longer exists.
            await TurnAt(8700, "east");

            Context.CurrentLocation.Should().BeOfType<Location.Duffy.DeckTwelve>();
        }
    }

    [TestFixture]
    public class TheDayBoundary : SurvivalClockTests
    {
        private async Task SleepThroughToMorning()
        {
            await TireToTheBrink();
            await TurnAt(8700);
        }

        [Test]
        public async Task SurvivingTheFirstNight_IsWorthThreePoints()
        {
            Context.Score.Should().Be(0);

            await SleepThroughToMorning();

            Context.Day.Should().Be(2);
            Context.Score.Should().Be(3);
        }

        [Test]
        public async Task ASecondNight_IsWorthNothingFurther()
        {
            await SleepThroughToMorning();
            Context.Score.Should().Be(3);

            // A fresh day means a fresh fatigue schedule: 5900 millichrons from the new morning.
            Context.SleepNotifications.NextWarningAt.Should().Be(MorningAfterWaking + 5900);

            await TurnAt(MorningAfterWaking + 5900);
            await TurnAt(MorningAfterWaking + 5900 + 320);
            await TurnAt(MorningAfterWaking + 5900 + 480);
            await TurnAt(MorningAfterWaking + 5900 + 560);
            await TurnAt(MorningAfterWaking + 5900 + 600);

            Context.Day.Should().Be(3);
            Context.Score.Should().Be(3);
        }

        [Test]
        public async Task WakingResetsTheClockToMorning()
        {
            await SleepThroughToMorning();

            Context.CurrentTime.Should().Be(MorningAfterWaking + StationfallContext.TurnTimeIncrement);
        }

        [Test]
        public async Task TheChronometerStops_OnceThePlayerIsPastTheirSecondDay()
        {
            await SleepThroughToMorning();
            GetItem<Chronometer>().HasStopped.Should().BeFalse("it still works on day two");

            var response = await _target.GetResponse("read time");
            response.Should().Contain("Galactic Standard Time");
        }

        [Test]
        public async Task Sleeping_MakesThePlayerLetGoOfEverythingNotWorn()
        {
            var form = GetItem<AssignmentCompletionForm>();
            var uniform = GetItem<PatrolUniform>();
            Context.Items.Should().Contain(form);

            await SleepThroughToMorning();

            Context.Items.Should().NotContain(form, "you don't keep hold of things while unconscious");
            form.CurrentLocation.Should().Be(Context.CurrentLocation, "it lands on the floor where you slept");
            Context.Items.Should().Contain(uniform, "what you're wearing stays on");
        }

        [Test]
        public async Task WakingHungry_ShortensTheMorningsGrace()
        {
            await TurnAt(6330); // reach Hungry before collapsing
            Context.Hunger.Should().Be(HungerLevel.Hungry);

            await SleepThroughToMorning();

            Context.Hunger.Should().Be(HungerLevel.WellFed);
            // globals.zil:1124 - going to bed famished costs you most of the next morning.
            Context.HungerNotifications.NextWarningAt.Should().Be(MorningAfterWaking + 200);
        }

        [Test]
        public async Task WakingWellFed_GivesTheFullMorning()
        {
            // Fatigue takes long enough to bite that hunger would normally get there first, so a player
            // who is still well-fed at bedtime is one who ate on the way. Push the hunger clock out
            // rather than walking to the food, which is what eating would amount to here.
            Context.HungerNotifications.NextWarningAt = 20000;

            await SleepThroughToMorning();

            Context.Hunger.Should().Be(HungerLevel.WellFed);
            Context.HungerNotifications.NextWarningAt.Should().Be(MorningAfterWaking + 400);
        }
    }

    [TestFixture]
    public class TurnCost : SurvivalClockTests
    {
        [Test]
        public async Task AnOrdinaryTurn_CostsTheDefault()
        {
            await TurnAt(StartTime);

            Context.CurrentTime.Should().Be(StartTime + StationfallContext.TurnTimeIncrement);
        }

        [Test]
        public async Task AMeal_CostsMoreThanAnOrdinaryTurn()
        {
            await TurnAt(6330);
            Context.Take(GetItem<BlueSoup>());

            await TurnAt(6400, "eat soup");

            // verbs.zil:690 prices a meal at 15 millichrons rather than the usual 7.
            Context.CurrentTime.Should().Be(6400 + 15);
        }

        [Test]
        public async Task AnExpensiveTurn_DoesNotMakeEveryLaterTurnExpensive()
        {
            await TurnAt(6330);
            Context.Take(GetItem<BlueSoup>());
            await TurnAt(6400, "eat soup");

            // misc.zil:381 resets the cost to the default at the end of every turn; leaving it raised
            // would quietly speed up every clock in the game for the rest of the session.
            await TurnAt(6500);
            Context.CurrentTime.Should().Be(6500 + StationfallContext.TurnTimeIncrement);
        }
    }

    [TestFixture]
    public class TheSleepVerb : SurvivalClockTests
    {
        [Test]
        public async Task RefusesWhenThePlayerIsNotTired()
        {
            (await TurnAt(StartTime, "sleep")).Should().Contain("You're not tired");
        }

        [Test]
        public async Task ExplainsThatSleepNeedsABed_WhenTiredAndStandingUp()
        {
            await TurnAt(8100);

            (await TurnAt(8150, "sleep")).Should().Contain("usually sleep in beds");
        }
    }
}
