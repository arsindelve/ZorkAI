using FluentAssertions;
using GameEngine;
using Planetfall.Item.Feinstein;
using Planetfall.Item.Kalamontee;
using Planetfall.Location.Kalamontee;
using Utilities;

namespace Planetfall.Tests;

/// <summary>
/// Comprehensive tests for the hunger/thirst system.
/// Based on the specification in issue #115.
/// </summary>
public class HungerSystemTests : EngineTestsBase
{
    /// <summary>
    /// Helper method to prevent hunger from advancing during tests.
    /// Sets the next warning time far in the future.
    /// </summary>
    private void PreventHungerAdvancement(PlanetfallContext context)
    {
        context.HungerNotifications.NextWarningAt = context.CurrentTime + 10000;
    }

    #region HungerNotifications Tests

    [Test]
    public void HungerNotifications_InitialState_DefaultsToZero()
    {
        var notifications = new HungerNotifications();
        notifications.NextWarningAt.Should().Be(0);
    }

    [Test]
    public void HungerNotifications_AfterInitialize_SetsNextWarningAt4000TicksFromStart()
    {
        var notifications = new HungerNotifications();
        notifications.Initialize(4500); // Typical starting time
        notifications.NextWarningAt.Should().Be(8500); // 4500 + 4000
    }

    [Test]
    public void HungerNotifications_BeforeFirstWarning_ReturnsNull()
    {
        var notifications = new HungerNotifications();
        notifications.Initialize(0); // Start at time 0
        var result = notifications.GetNotification(3999, HungerLevel.WellFed);
        result.Should().BeNull();
    }

    [Test]
    public void HungerNotifications_AtFirstWarning_ReturnsHungryNotification()
    {
        var notifications = new HungerNotifications();
        notifications.Initialize(0); // Start at time 0
        var result = notifications.GetNotification(4000, HungerLevel.WellFed);
        result.Should().Contain("growl from your stomach");
        result.Should().Contain("hungry and thirsty");
    }

    [Test]
    public void HungerNotifications_ProgressionToLevel2_SchedulesNext900Ticks()
    {
        var notifications = new HungerNotifications();
        notifications.Initialize(0); // Start at time 0
        notifications.GetNotification(4000, HungerLevel.WellFed);
        notifications.NextWarningAt.Should().Be(4900); // 4000 + 900
    }

    [Test]
    public void HungerNotifications_AtLevel2Warning_ReturnsRavenousNotification()
    {
        var notifications = new HungerNotifications();
        notifications.Initialize(0); // Start at time 0
        notifications.GetNotification(4000, HungerLevel.WellFed); // First warning
        var result = notifications.GetNotification(4900, HungerLevel.Hungry);
        result.Should().Contain("ravenous");
        result.Should().Contain("parched");
    }

    [Test]
    public void HungerNotifications_ProgressionToLevel3_SchedulesNext300Ticks()
    {
        var notifications = new HungerNotifications();
        notifications.Initialize(0); // Start at time 0
        notifications.GetNotification(4000, HungerLevel.WellFed);
        notifications.GetNotification(4900, HungerLevel.Hungry);
        notifications.NextWarningAt.Should().Be(5200); // 4900 + 300
    }

    [Test]
    public void HungerNotifications_AtLevel3Warning_ReturnsFaintNotification()
    {
        var notifications = new HungerNotifications();
        notifications.Initialize(0); // Start at time 0
        notifications.GetNotification(4000, HungerLevel.WellFed);
        notifications.GetNotification(4900, HungerLevel.Hungry);
        var result = notifications.GetNotification(5200, HungerLevel.Ravenous);
        result.Should().Contain("faint");
        result.Should().Contain("food and liquid");
    }

    [Test]
    public void HungerNotifications_ProgressionToLevel4_SchedulesNext200Ticks()
    {
        var notifications = new HungerNotifications();
        notifications.Initialize(0); // Start at time 0
        notifications.GetNotification(4000, HungerLevel.WellFed);
        notifications.GetNotification(4900, HungerLevel.Hungry);
        notifications.GetNotification(5200, HungerLevel.Ravenous);
        notifications.NextWarningAt.Should().Be(5400); // 5200 + 200
    }

    [Test]
    public void HungerNotifications_AtLevel4Warning_ReturnsAboutToPassOutNotification()
    {
        var notifications = new HungerNotifications();
        notifications.Initialize(0); // Start at time 0
        notifications.GetNotification(4000, HungerLevel.WellFed);
        notifications.GetNotification(4900, HungerLevel.Hungry);
        notifications.GetNotification(5200, HungerLevel.Ravenous);
        var result = notifications.GetNotification(5400, HungerLevel.Faint);
        result.Should().Contain("millichrons");
        result.Should().Contain("pass out");
    }

    [Test]
    public void HungerNotifications_ProgressionToLevel5_SchedulesNext100Ticks()
    {
        var notifications = new HungerNotifications();
        notifications.Initialize(0); // Start at time 0
        notifications.GetNotification(4000, HungerLevel.WellFed);
        notifications.GetNotification(4900, HungerLevel.Hungry);
        notifications.GetNotification(5200, HungerLevel.Ravenous);
        notifications.GetNotification(5400, HungerLevel.Faint);
        notifications.NextWarningAt.Should().Be(5500); // 5400 + 100
    }

    [Test]
    public void HungerNotifications_ResetAfterEating_UpdatesNextWarningTime()
    {
        var notifications = new HungerNotifications();
        notifications.ResetAfterEating(5000, 3600);
        notifications.NextWarningAt.Should().Be(8600); // 5000 + 3600
    }

    [Test]
    public void HungerNotifications_GetNextHungerLevel_WhenTimeReached_ReturnsNextLevel()
    {
        var notifications = new HungerNotifications();
        notifications.Initialize(0); // Start at time 0
        var result = notifications.GetNextHungerLevel(4000, HungerLevel.WellFed);
        result.Should().Be(HungerLevel.Hungry);
    }

    [Test]
    public void HungerNotifications_GetNextHungerLevel_BeforeTimeReached_ReturnsNull()
    {
        var notifications = new HungerNotifications();
        notifications.Initialize(0); // Start at time 0
        var result = notifications.GetNextHungerLevel(3999, HungerLevel.WellFed);
        result.Should().BeNull();
    }

    #endregion

    #region HungerLevel Enum Tests

    [Test]
    public void HungerLevel_WellFed_HasCorrectValue()
    {
        ((int)HungerLevel.WellFed).Should().Be(0);
    }

    [Test]
    public void HungerLevel_Hungry_HasCorrectValue()
    {
        ((int)HungerLevel.Hungry).Should().Be(1);
    }

    [Test]
    public void HungerLevel_Dead_HasCorrectValue()
    {
        ((int)HungerLevel.Dead).Should().Be(5);
    }

    [Test]
    public void HungerLevel_AllLevels_HaveNotifications()
    {
        HungerLevel.Hungry.GetNotification().Should().NotBeNullOrEmpty();
        HungerLevel.Ravenous.GetNotification().Should().NotBeNullOrEmpty();
        HungerLevel.Faint.GetNotification().Should().NotBeNullOrEmpty();
        HungerLevel.AboutToPassOut.GetNotification().Should().NotBeNullOrEmpty();
    }

    #endregion

    #region ProteinLiquid Tests

    [Test]
    public async Task ProteinLiquid_WhenNotHungry_RejectsConsumption()
    {
        var target = GetTarget();
        StartHere<Kitchen>();

        var pfContext = target.Context;
        PreventHungerAdvancement(pfContext);

        var canteen = GetItem<Canteen>();
        canteen.IsOpen = true;
        canteen.ItemPlacedHere<ProteinLiquid>();
        target.Context.ItemPlacedHere(canteen);

        var response = await target.GetResponse("drink liquid");
        response.Should().Contain("not hungry");
    }

    [Test]
    public async Task ProteinLiquid_WhenHungry_ConsumesAndResetsHunger()
    {
        var target = GetTarget();
        StartHere<Kitchen>();

        var pfContext = target.Context;
        // Make player hungry
        pfContext.Hunger = HungerLevel.Hungry;
        PreventHungerAdvancement(pfContext);

        var canteen = GetItem<Canteen>();
        canteen.IsOpen = true;
        canteen.ItemPlacedHere<ProteinLiquid>();
        target.Context.ItemPlacedHere(canteen);

        var response = await target.GetResponse("drink liquid");
        response.Should().Contain("Mmmm");
        response.Should().Contain("quenched your thirst");
        response.Should().Contain("satisfied your hunger");

        target.Context.Hunger.Should().Be(HungerLevel.WellFed);
    }

    [Test]
    public async Task ProteinLiquid_AfterConsuming_ResetsNotificationTimer()
    {
        var target = GetTarget();
        StartHere<Kitchen>();

        var pfContext = target.Context;
        pfContext.Hunger = HungerLevel.Ravenous;
        var currentTime = pfContext.CurrentTime;

        var canteen = GetItem<Canteen>();
        canteen.IsOpen = true;
        canteen.ItemPlacedHere<ProteinLiquid>();
        target.Context.ItemPlacedHere(canteen);

        await target.GetResponse("drink liquid");

        // Protein liquid should reset timer to current time + 3600
        pfContext.HungerNotifications.NextWarningAt.Should().Be(currentTime + 3600);
    }

    #endregion

    #region Goo Tests

    [Test]
    public async Task RedGoo_WhenNotHungry_RejectsConsumption()
    {
        var target = GetTarget();
        StartHere<Kitchen>();

        PreventHungerAdvancement(target.Context);

        var kit = GetItem<SurvivalKit>();
        kit.IsOpen = true;
        target.Context.ItemPlacedHere(kit);

        var response = await target.GetResponse("eat red goo");
        response.Should().Contain("not hungry");
    }

    [Test]
    public async Task RedGoo_WhenNotHungry_DoesNotConsumeGoo()
    {
        var target = GetTarget();
        StartHere<Kitchen>();

        PreventHungerAdvancement(target.Context);

        var kit = GetItem<SurvivalKit>();
        kit.IsOpen = true;
        target.Context.ItemPlacedHere(kit);

        // Verify goo exists before
        kit.Items.Should().Contain(item => item is RedGoo);

        var response = await target.GetResponse("eat red goo");
        response.Should().Contain("not hungry");

        // Goo should NOT be consumed when player is not hungry
        kit.Items.Should().Contain(item => item is RedGoo, "the goo should not be consumed when player is not hungry");
    }

    [Test]
    public async Task RedGoo_WhenHungry_ConsumesWithCherryPieFlavor()
    {
        var target = GetTarget();
        StartHere<Kitchen>();

        var pfContext = target.Context;
        pfContext.Hunger = HungerLevel.Hungry;
        PreventHungerAdvancement(pfContext);

        var kit = GetItem<SurvivalKit>();
        kit.IsOpen = true;
        target.Context.ItemPlacedHere(kit);

        var response = await target.GetResponse("eat red goo");
        response.Should().Contain("Mmmm");
        response.Should().Contain("cherry pie");
    }

    [Test]
    public async Task BrownGoo_WhenHungry_ConsumesWithFungusPuddingFlavor()
    {
        var target = GetTarget();
        StartHere<Kitchen>();

        var pfContext = target.Context;
        pfContext.Hunger = HungerLevel.Faint;
        PreventHungerAdvancement(pfContext);

        var kit = GetItem<SurvivalKit>();
        kit.IsOpen = true;
        target.Context.ItemPlacedHere(kit);

        var response = await target.GetResponse("eat brown goo");
        response.Should().Contain("Mmmm");
        response.Should().Contain("fungus pudding");
    }

    [Test]
    public async Task GreenGoo_WhenHungry_ConsumesWithLimaBeansFlavor()
    {
        var target = GetTarget();
        StartHere<Kitchen>();

        var pfContext = target.Context;
        pfContext.Hunger = HungerLevel.AboutToPassOut;
        PreventHungerAdvancement(pfContext);

        var kit = GetItem<SurvivalKit>();
        kit.IsOpen = true;
        target.Context.ItemPlacedHere(kit);

        var response = await target.GetResponse("eat green goo");
        response.Should().Contain("Mmmm");
        response.Should().Contain("lima beans");
    }

    [Test]
    public async Task Goo_AfterConsuming_ResetsHungerTo1450Ticks()
    {
        var target = GetTarget();
        StartHere<Kitchen>();

        var pfContext = target.Context;
        pfContext.Hunger = HungerLevel.Ravenous;
        var currentTime = pfContext.CurrentTime;

        var kit = GetItem<SurvivalKit>();
        kit.IsOpen = true;
        target.Context.ItemPlacedHere(kit);

        await target.GetResponse("eat green goo");

        pfContext.Hunger.Should().Be(HungerLevel.WellFed);
        pfContext.HungerNotifications.NextWarningAt.Should().Be(currentTime + 1450);
    }

    [Test]
    public async Task Goo_WithoutSurvivalKit_CannotEat()
    {
        var target = GetTarget();
        StartHere<Kitchen>();

        var pfContext = target.Context;
        pfContext.Hunger = HungerLevel.Hungry;
        PreventHungerAdvancement(pfContext);

        // Place goo on ground without kit
        target.Context.CurrentLocation.ItemPlacedHere(Repository.GetItem<RedGoo>());

        var response = await target.GetResponse("eat red goo");
        response.Should().Contain("aren't holding that");
    }
   
    [Test]
    public void SurvivalKit_StartsWithThreeGooItems()
    {
        Repository.Reset();
        var kit = Repository.GetItem<SurvivalKit>();
        kit.Items.Should().HaveCount(3);
        kit.Items.Should().Contain(item => item is RedGoo);
        kit.Items.Should().Contain(item => item is BrownGoo);
        kit.Items.Should().Contain(item => item is GreenGoo);
    }

    [Test]
    public async Task Goo_WhenPlayerTriesToTake_ShowsCannotBeTakenMessage()
    {
        var target = GetTarget();
        StartHere<Kitchen>();

        var kit = GetItem<SurvivalKit>();
        kit.IsOpen = true;
        target.Context.ItemPlacedHere(kit);

        var response = await target.GetResponse("take red goo");
        response.Should().Contain("ooze through your fingers");
        response.Should().Contain("eat it right from the survival kit");
    }

    #endregion

    #region Integration Tests

    [Test]
    public async Task HungerProgression_OverTime_AdvancesThroughLevels()
    {
        var target = GetTarget();
        StartHere<Kitchen>();

        var pfContext = target.Context;

        // Initial state
        pfContext.Hunger.Should().Be(HungerLevel.WellFed);

        // Simulate time passage to first warning (4000 ticks)
        Repository.GetItem<Chronometer>().CurrentTime = 4000;
        pfContext.HungerNotifications.NextWarningAt = 4000;

        await target.GetResponse("look");

        // After processing turn, hunger should advance
        pfContext.Hunger.Should().Be(HungerLevel.Hungry);
    }

    [Test]
    public async Task HungerDeath_AtLevel5_CausesPlayerDeath()
    {
        var target = GetTarget();
        StartHere<Kitchen>();

        var pfContext = target.Context;
        pfContext.Hunger = HungerLevel.AboutToPassOut;

        // Set up for death
        Repository.GetItem<Chronometer>().CurrentTime = 5500;
        pfContext.HungerNotifications.NextWarningAt = 5500;

        var response = await target.GetResponse("look");

        response.Should().Contain("collapse");
        response.Should().Contain("thirst and hunger");
        response.Should().Contain("You have died");
    }

    [Test]
    public async Task HungerProgression_AfterEating_Resets()
    {
        var target = GetTarget();
        StartHere<Kitchen>();

        var pfContext = target.Context;
        pfContext.Hunger = HungerLevel.Faint;

        var kit = GetItem<SurvivalKit>();
        kit.IsOpen = true;
        target.Context.ItemPlacedHere(kit);

        await target.GetResponse("eat red goo");

        pfContext.Hunger.Should().Be(HungerLevel.WellFed);

        // Advance time but not enough to trigger next warning
        Repository.GetItem<Chronometer>().CurrentTime += 1000;

        await target.GetResponse("look");

        // Should still be well fed
        pfContext.Hunger.Should().Be(HungerLevel.WellFed);
    }

    #endregion
}