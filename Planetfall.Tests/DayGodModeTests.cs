using FluentAssertions;
using Model.Interface;
using Moq;
using Planetfall.Item.Feinstein;
using Planetfall.Location.Kalamontee.Dorm;
using Utilities;

namespace Planetfall.Tests;

/// <summary>
/// God-mode command to advance the calendar to a given day ("god mode day 5"), a playtesting affordance
/// for day-gated content (sickness escalation, the shrinking carry limit, per-day sleep pressure)
/// without sleeping through the intervening nights. It runs the game's own day-advance transition
/// (<see cref="SleepEngine.StartNewDay" />) once per day, so every clock that is a function of the day
/// moves by the real rules - but it does NOT charge the costs of having slept (dropped inventory,
/// spoiled food), which belong to sleep rather than to the calendar. Covers the command wiring, each
/// piece of day-coupled state, the boundary with sleep's costs, argument validation, and persistence.
/// </summary>
public class DayGodModeTests : EngineTestsBase
{
    /// <summary>
    /// Pins the chronometer's morning jitter so wake times are exact rather than an 80-tick range.
    /// </summary>
    private void PinMorningJitter(int jitter)
    {
        var mockChooser = new Mock<IRandomChooser>();
        mockChooser.Setup(r => r.RollDice(Chronometer.MorningJitterTicks)).Returns(jitter);
        GetItem<Chronometer>().Chooser = mockChooser.Object;
    }

    #region Command wiring

    [Test]
    public async Task GodMode_Day_AdvancesToTheRequestedDay()
    {
        var engine = GetTarget();
        StartHere<DormA>();

        var response = await engine.GetResponse("god mode day 5");

        Context.Day.Should().Be(5);
        // Confirmation - and proof the day branch ran rather than the generic god-mode error.
        response.Should().Contain("Day 5");
    }

    [Test]
    public async Task GodMode_Day_RefusesToGoBackwards()
    {
        // The calendar only advances: the command replays the real day transition, and there is no
        // inverse for it (you cannot un-advance the disease).
        var engine = GetTarget();
        StartHere<DormA>();
        Context.Day = 6;

        var response = await engine.GetResponse("god mode day 2");

        Context.Day.Should().Be(6); // unchanged
        response.Should().Contain("only moves forward");
    }

    [Test]
    public async Task GodMode_Day_RefusesToRepeatTheCurrentDay()
    {
        var engine = GetTarget();
        StartHere<DormA>();
        Context.Day = 4;

        var response = await engine.GetResponse("god mode day 4");

        response.Should().Contain("only moves forward");
    }

    [Test]
    public async Task GodMode_Day_ReportsTheNewDayInTheScore()
    {
        var engine = GetTarget();
        StartHere<DormA>();

        await engine.GetResponse("god mode day 7");
        var score = await engine.GetResponse("score");

        score.Should().Contain("It is Day 7");
    }

    #endregion

    #region Day-coupled state

    [Test]
    public async Task GodMode_Day_ResetsChronometerToThatDaysMorning()
    {
        var engine = GetTarget();
        StartHere<DormA>();
        PinMorningJitter(7);

        await engine.GetResponse("god mode day 4");

        // Day 4's base morning is 1950. The god-mode turn's own end-of-turn tick is already accounted
        // for, so the player reads the day's morning time rather than 54 ticks past it.
        Context.CurrentTime.Should().Be(1957);
    }

    [Test]
    public async Task GodMode_Day_AdvancesTheDiseaseOnceForEachDayCrossed()
    {
        var engine = GetTarget();
        StartHere<DormA>();

        await engine.GetResponse("god mode day 6");

        // Untreated, the sickness counter tracks the calendar (it starts at 1 == Day 1), so five days
        // crossed means five advances.
        Context.SicknessCounter.Should().Be(6);
        Context.EffectiveLoadAllowed.Should().Be(50); // 100 - 10 per sick day past the first
    }

    [Test]
    public async Task GodMode_Day_PreservesMedicineTreatment()
    {
        var engine = GetTarget();
        StartHere<DormA>();

        // A player who drank the experimental medicine on Day 3: Medicine.cs rolls the counter back two
        // levels, so it now trails the calendar. Advancing the day must ADVANCE that counter, not
        // reassign it to the day - reassigning would silently undo the cure.
        Context.Day = 3;
        Context.SicknessCounter = 1;

        await engine.GetResponse("god mode day 6");

        Context.SicknessCounter.Should().Be(4); // 1 + the three days crossed, not 6
        Context.EffectiveLoadAllowed.Should().Be(70); // still healthier than an untreated Day 6 player
    }

    [Test]
    public async Task GodMode_Day_ResetsFatigueAndTheSleepSchedule()
    {
        var engine = GetTarget();
        StartHere<DormA>();

        Context.Tired = TiredLevel.AboutToDrop;
        Context.SleepNotifications.QueueFallAsleep(Context.CurrentTime);

        await engine.GetResponse("god mode day 4");

        Context.Tired.Should().Be(TiredLevel.WellRested);
        Context.SleepNotifications.FallAsleepQueued.Should().BeFalse();
        // Day 4 gets 5200 ticks of wakefulness before the first fatigue warning.
        Context.SleepNotifications.NextWarningAt.Should().Be(Context.CurrentTime + 5200);
    }

    [Test]
    public async Task GodMode_Day_ResetsHungerAndTheHungerSchedule()
    {
        var engine = GetTarget();
        StartHere<DormA>();

        Context.Hunger = HungerLevel.Faint;

        await engine.GetResponse("god mode day 4");

        Context.Hunger.Should().Be(HungerLevel.WellFed);
        // Rescheduled off the NEW (much earlier) morning time - otherwise the stale warning time from
        // the old day would sit thousands of ticks in the future and hunger would never advance again.
        // 800 (the fresh timer) rather than 200 (the famished one): the very first day crossed cleared
        // the hunger, so the player arrives on Day 4 well-fed rather than waking up starving.
        Context.HungerNotifications.NextWarningAt.Should().Be(Context.CurrentTime + 800);
    }

    [Test]
    public async Task GodMode_Day_StillDeliversThatDaysSicknessNotification()
    {
        var engine = GetTarget();
        StartHere<DormA>();

        await engine.GetResponse("god mode day 3");
        // "wait", not "look" - look is a free command and skips ProcessBeginningOfTurn entirely.
        var response = await engine.GetResponse("wait");

        response.Should().Contain("unusually weak"); // the Day 3 sickness warning
    }

    [Test]
    public async Task GodMode_Day_DoesNotForceSleepOnTheVeryNextTurn()
    {
        // The jump must leave a coherent clock: an exhausted player who advances days wakes up rested,
        // not immediately face-planted by the forced-sleep check.
        var engine = GetTarget();
        StartHere<DormA>();

        Context.Tired = TiredLevel.AboutToDrop;
        // Due on the turn AFTER the god-mode command (the end-of-turn tick advances 54), so the stale
        // schedule is what the advance has to defuse - not something that fires before it even runs.
        Context.SleepNotifications.NextWarningAt = Context.CurrentTime + 1;

        await engine.GetResponse("god mode day 5");
        await engine.GetResponse("wait");

        Context.CurrentLocation.Should().BeOfType<DormA>(); // never climbed into a bunk
        Context.Day.Should().Be(5); // and no forced sleep rolled the day over
    }

    #endregion

    #region Boundary with sleep's costs

    [Test]
    public async Task GodMode_Day_DoesNotChargeSleepsCosts()
    {
        // The calendar advance replays the day transition, NOT the wake-up routine: a tester who
        // advances days keeps the kit they were about to test with, instead of finding it on the floor.
        var engine = GetTarget();
        StartHere<DormA>();
        var diary = Take<Diary>();

        await engine.GetResponse("god mode day 5");

        Context.Items.Should().Contain(diary);
    }

    #endregion

    #region Argument validation

    [Test]
    public async Task GodMode_Day_RejectsADayPastTheEndOfTheGame()
    {
        var engine = GetTarget();
        StartHere<DormA>();

        var response = await engine.GetResponse("god mode day 12");

        Context.Day.Should().Be(1); // unchanged
        response.Should().Contain("between 1 and 8");
    }

    [Test]
    public async Task GodMode_Day_RejectsADayBeforeTheStartOfTheGame()
    {
        var engine = GetTarget();
        StartHere<DormA>();

        var response = await engine.GetResponse("god mode day 0");

        Context.Day.Should().Be(1); // unchanged
        response.Should().Contain("between 1 and 8");
    }

    [Test]
    public async Task GodMode_Day_AcceptsAWordierPhrasing()
    {
        // The sibling god-mode subcommands all match loosely on trigger words rather than on position;
        // the day argument should be just as forgiving.
        var engine = GetTarget();
        StartHere<DormA>();

        await engine.GetResponse("god mode set day to 5");

        Context.Day.Should().Be(5);
    }

    [Test]
    public async Task GodMode_Day_WithNoNumber_FallsThroughToTheGenericError()
    {
        var engine = GetTarget();
        StartHere<DormA>();

        var response = await engine.GetResponse("god mode day");

        Context.Day.Should().Be(1);
        response.Should().Contain("Invalid use of God mode");
    }

    [Test]
    public async Task GodMode_Day_WithANonNumber_FallsThroughToTheGenericError()
    {
        var engine = GetTarget();
        StartHere<DormA>();

        var response = await engine.GetResponse("god mode day tuesday");

        Context.Day.Should().Be(1);
        response.Should().Contain("Invalid use of God mode");
    }

    #endregion

    #region Persistence

    [Test]
    public async Task GodMode_Day_SurvivesSaveAndRestore()
    {
        var engine = GetTarget();
        StartHere<DormA>();
        PinMorningJitter(7);

        await engine.GetResponse("god mode day 5");
        var saved = engine.SaveGame();

        // Restore into a fresh engine/repository to prove the whole jump round-trips through the save
        // JSON - the calendar AND the clock it is coupled to, which live in different parts of the save.
        var freshEngine = GetTarget();
        var restored = (PlanetfallContext)freshEngine.RestoreGame(saved);

        restored.Day.Should().Be(5);
        restored.SicknessCounter.Should().Be(5);
        restored.CurrentTime.Should().Be(2157); // Day 5's base morning of 2150, plus the pinned jitter
    }

    #endregion
}
