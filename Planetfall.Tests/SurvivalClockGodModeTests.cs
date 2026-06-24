using FluentAssertions;
using Planetfall.Location.Kalamontee.Dorm;
using Utilities;

namespace Planetfall.Tests;

/// <summary>
/// Issue #277: god-mode toggle to disable the sleep and hunger survival clocks, a deterministic
/// playtesting affordance. Covers the command wiring, the per-turn clock guards, persistence
/// through save/restore, and that ordinary (non-god-mode) play is unaffected.
/// </summary>
public class SurvivalClockGodModeTests : EngineTestsBase
{
    #region Command wiring

    [Test]
    public async Task GodMode_NoSleep_DisablesSleepClockOnly()
    {
        var engine = GetTarget();
        StartHere<DormA>();

        var response = await engine.GetResponse("god mode no sleep");

        engine.Context.SleepClockDisabled.Should().BeTrue();
        engine.Context.HungerClockDisabled.Should().BeFalse();
        // Confirmation - and proof the survival branch ran rather than the generic god-mode error.
        response.Should().Contain("sleep clock disabled");
    }

    [Test]
    public async Task GodMode_Sleep_ReEnablesSleepClock()
    {
        var engine = GetTarget();
        StartHere<DormA>();
        engine.Context.SleepClockDisabled = true;

        await engine.GetResponse("god mode sleep");

        engine.Context.SleepClockDisabled.Should().BeFalse();
    }

    [Test]
    public async Task GodMode_NoHunger_DisablesHungerClockOnly()
    {
        var engine = GetTarget();
        StartHere<DormA>();

        await engine.GetResponse("god mode no hunger");

        engine.Context.HungerClockDisabled.Should().BeTrue();
        engine.Context.SleepClockDisabled.Should().BeFalse();
    }

    [Test]
    public async Task GodMode_Hunger_ReEnablesHungerClock()
    {
        var engine = GetTarget();
        StartHere<DormA>();
        engine.Context.HungerClockDisabled = true;

        await engine.GetResponse("god mode hunger");

        engine.Context.HungerClockDisabled.Should().BeFalse();
    }

    [Test]
    public async Task GodMode_NoSurvival_DisablesBothClocks()
    {
        var engine = GetTarget();
        StartHere<DormA>();

        await engine.GetResponse("god mode no survival");

        engine.Context.SleepClockDisabled.Should().BeTrue();
        engine.Context.HungerClockDisabled.Should().BeTrue();
    }

    [Test]
    public async Task GodMode_Survival_ReEnablesBothClocks()
    {
        var engine = GetTarget();
        StartHere<DormA>();
        engine.Context.SleepClockDisabled = true;
        engine.Context.HungerClockDisabled = true;

        await engine.GetResponse("god mode survival");

        engine.Context.SleepClockDisabled.Should().BeFalse();
        engine.Context.HungerClockDisabled.Should().BeFalse();
    }

    #endregion

    #region Sleep clock guards

    [Test]
    public void SleepClockDisabled_DoesNotForceSleep_EvenAtMaxTiredness()
    {
        var engine = GetTarget();
        var context = engine.Context;
        StartHere<DormA>();

        context.SleepClockDisabled = true;
        context.Tired = TiredLevel.AboutToDrop;
        // Forced sleep would normally fire this very turn.
        context.SleepNotifications.NextWarningAt = context.CurrentTime;

        context.ProcessBeginningOfTurn();

        context.Day.Should().Be(1); // no forced sleep, so the day never advanced
        context.Tired.Should().Be(TiredLevel.WellRested); // an already-tired session is relieved
        context.CurrentLocation.Should().BeOfType<DormA>(); // never climbed into a bunk
    }

    [Test]
    public void SleepClockDisabled_NoTirednessWarnings_OverLongSession()
    {
        var engine = GetTarget();
        var context = engine.Context;
        StartHere<DormA>();

        context.SleepClockDisabled = true;
        // Warnings/escalation would otherwise fire repeatedly across the session.
        context.SleepNotifications.NextWarningAt = context.CurrentTime;

        for (var i = 0; i < 150; i++)
        {
            var result = context.ProcessBeginningOfTurn() ?? string.Empty;
            result.Should().NotContain("weary");
            result.Should().NotContain("tired");
            context.ProcessEndOfTurn(); // advances the chronometer
        }

        context.Tired.Should().Be(TiredLevel.WellRested);
        context.Day.Should().Be(1);
    }

    [Test]
    public void SleepClock_WhenEnabled_StillForcesSleep()
    {
        // Control: the toggle is opt-in. Default play must still force sleep at max tiredness.
        var engine = GetTarget();
        var context = engine.Context;
        StartHere<DormA>();

        context.Tired = TiredLevel.AboutToDrop;
        context.SleepNotifications.NextWarningAt = context.CurrentTime;

        var result = context.ProcessBeginningOfTurn() ?? string.Empty;

        result.Should().Contain("bunk beds"); // forced sleep climbed into a bunk
        context.Day.Should().Be(2); // and advanced to the next day
    }

    #endregion

    #region Hunger clock guards

    [Test]
    public void HungerClockDisabled_NoStarvationDeath_OverLongSession()
    {
        var engine = GetTarget();
        var context = engine.Context;
        StartHere<DormA>();

        context.HungerClockDisabled = true;
        context.Hunger = HungerLevel.AboutToPassOut; // one tick from a starvation death
        context.HungerNotifications.NextWarningAt = context.CurrentTime;

        for (var i = 0; i < 150; i++)
        {
            context.ProcessBeginningOfTurn();
            context.PendingDeath.Should().BeNull();
            context.ProcessEndOfTurn();
        }

        context.Hunger.Should().Be(HungerLevel.WellFed); // an already-hungry session is relieved
    }

    [Test]
    public void HungerClock_WhenEnabled_StillKillsByStarvation()
    {
        // Control: the toggle is opt-in. Default play must still starve the player.
        var engine = GetTarget();
        var context = engine.Context;
        StartHere<DormA>();

        context.Hunger = HungerLevel.AboutToPassOut;
        context.HungerNotifications.NextWarningAt = context.CurrentTime;

        context.ProcessBeginningOfTurn();

        context.PendingDeath.Should().NotBeNull();
    }

    #endregion

    #region Persistence

    [Test]
    public void SurvivalClockFlags_SurviveSaveAndRestore()
    {
        var engine = GetTarget();
        StartHere<DormA>();
        engine.Context.SleepClockDisabled = true;
        engine.Context.HungerClockDisabled = true;

        var saved = engine.SaveGame();

        // Restore into a fresh engine/repository to prove the flags round-trip through the save JSON.
        var freshEngine = GetTarget();
        var restored = (PlanetfallContext)freshEngine.RestoreGame(saved);

        restored.SleepClockDisabled.Should().BeTrue();
        restored.HungerClockDisabled.Should().BeTrue();
    }

    #endregion
}
