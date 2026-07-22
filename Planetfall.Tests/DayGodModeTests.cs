using FluentAssertions;
using Planetfall.Location.Kalamontee.Dorm;
using Utilities;

namespace Planetfall.Tests;

/// <summary>
/// God-mode command to jump the calendar to a given day ("god mode day 5"), a playtesting affordance
/// for day-gated content (sickness escalation, the shrinking carry limit, per-day sleep pressure)
/// without sleeping through four in-game nights to reach it. The jump lands the player at the START of
/// the requested day, exactly as waking up would: chronometer at that day's morning, fatigue and
/// hunger reset, and the disease at the untreated level for that day. Covers the command wiring, each
/// piece of day-coupled state it drags along, argument validation, and persistence.
/// </summary>
public class DayGodModeTests : EngineTestsBase
{
    #region Command wiring

    [Test]
    public async Task GodMode_Day_SetsTheCurrentDay()
    {
        var engine = GetTarget();
        StartHere<DormA>();

        var response = await engine.GetResponse("god mode day 5");

        engine.Context.Day.Should().Be(5);
        // Confirmation - and proof the day branch ran rather than the generic god-mode error.
        response.Should().Contain("Day 5");
    }

    [Test]
    public async Task GodMode_Day_CanJumpBackwards()
    {
        var engine = GetTarget();
        StartHere<DormA>();
        engine.Context.Day = 6;

        await engine.GetResponse("god mode day 2");

        engine.Context.Day.Should().Be(2);
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

        await engine.GetResponse("god mode day 4");

        // Day 4 wakes between 1950 and 2029 (Chronometer.MorningTimesByDay + up to 80 ticks of jitter).
        // The god-mode turn's own end-of-turn tick must already be accounted for, so the player reads
        // the day's morning time on the very next turn rather than 54 ticks past it.
        engine.Context.CurrentTime.Should().BeInRange(1950, 2029);
    }

    [Test]
    public async Task GodMode_Day_AdvancesTheDiseaseToTheUntreatedLevelForThatDay()
    {
        var engine = GetTarget();
        StartHere<DormA>();

        await engine.GetResponse("god mode day 6");

        // Untreated, the sickness counter tracks the calendar (it starts at 1 == Day 1), so jumping the
        // day must drag the disease along or the jump would silently cure the player.
        engine.Context.SicknessCounter.Should().Be(6);
        engine.Context.EffectiveLoadAllowed.Should().Be(50); // 100 - 10 per sick day past the first
    }

    [Test]
    public async Task GodMode_Day_ResetsFatigueAndTheSleepSchedule()
    {
        var engine = GetTarget();
        var context = engine.Context;
        StartHere<DormA>();

        context.Tired = TiredLevel.AboutToDrop;
        context.SleepNotifications.QueueFallAsleep(context.CurrentTime);

        await engine.GetResponse("god mode day 4");

        context.Tired.Should().Be(TiredLevel.WellRested);
        context.SleepNotifications.FallAsleepQueued.Should().BeFalse();
        // Day 4 gets 5200 ticks of wakefulness before the first fatigue warning.
        context.SleepNotifications.NextWarningAt.Should().Be(context.CurrentTime + 5200);
    }

    [Test]
    public async Task GodMode_Day_ResetsHungerAndTheHungerSchedule()
    {
        var engine = GetTarget();
        var context = engine.Context;
        StartHere<DormA>();

        context.Hunger = HungerLevel.Faint;

        await engine.GetResponse("god mode day 4");

        context.Hunger.Should().Be(HungerLevel.WellFed);
        // Rescheduled off the NEW (much earlier) morning time - otherwise the stale warning time from
        // the old day would sit thousands of ticks in the future and hunger would never advance again.
        context.HungerNotifications.NextWarningAt.Should().Be(context.CurrentTime + 800);
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
        // The jump must leave a coherent clock: an exhausted player who jumps days wakes up rested,
        // not immediately face-planted by the forced-sleep check.
        var engine = GetTarget();
        var context = engine.Context;
        StartHere<DormA>();

        context.Tired = TiredLevel.AboutToDrop;
        // Due on the turn AFTER the god-mode command (the end-of-turn tick advances 54), so the stale
        // schedule is what the jump has to defuse - not something that fires before it even runs.
        context.SleepNotifications.NextWarningAt = context.CurrentTime + 1;

        await engine.GetResponse("god mode day 5");
        await engine.GetResponse("wait");

        context.CurrentLocation.Should().BeOfType<DormA>(); // never climbed into a bunk
        context.Day.Should().Be(5); // and no forced sleep rolled the day over
    }

    #endregion

    #region Argument validation

    [Test]
    public async Task GodMode_Day_RejectsADayPastTheEndOfTheGame()
    {
        var engine = GetTarget();
        StartHere<DormA>();

        var response = await engine.GetResponse("god mode day 12");

        engine.Context.Day.Should().Be(1); // unchanged
        response.Should().Contain("between 1 and 8");
    }

    [Test]
    public async Task GodMode_Day_RejectsADayBeforeTheStartOfTheGame()
    {
        var engine = GetTarget();
        StartHere<DormA>();

        var response = await engine.GetResponse("god mode day 0");

        engine.Context.Day.Should().Be(1); // unchanged
        response.Should().Contain("between 1 and 8");
    }

    [Test]
    public async Task GodMode_Day_WithNoNumber_FallsThroughToTheGenericError()
    {
        var engine = GetTarget();
        StartHere<DormA>();

        var response = await engine.GetResponse("god mode day");

        engine.Context.Day.Should().Be(1);
        response.Should().Contain("Invalid use of God mode");
    }

    [Test]
    public async Task GodMode_Day_WithANonNumber_FallsThroughToTheGenericError()
    {
        var engine = GetTarget();
        StartHere<DormA>();

        var response = await engine.GetResponse("god mode day tuesday");

        engine.Context.Day.Should().Be(1);
        response.Should().Contain("Invalid use of God mode");
    }

    #endregion

    #region Persistence

    [Test]
    public async Task GodMode_Day_SurvivesSaveAndRestore()
    {
        var engine = GetTarget();
        StartHere<DormA>();

        await engine.GetResponse("god mode day 5");
        var saved = engine.SaveGame();

        // Restore into a fresh engine/repository to prove the day round-trips through the save JSON.
        var freshEngine = GetTarget();
        var restored = (PlanetfallContext)freshEngine.RestoreGame(saved);

        restored.Day.Should().Be(5);
        restored.SicknessCounter.Should().Be(5);
    }

    #endregion
}
