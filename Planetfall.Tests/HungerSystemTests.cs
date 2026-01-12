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
    public void HungerNotifications_AfterInitialize_SetsNextWarningAt2000TicksFromStart()
    {
        var notifications = new HungerNotifications();
        notifications.Initialize(4500); // Typical starting time
        notifications.NextWarningAt.Should().Be(6500); // 4500 + 2000
    }

    [Test]
    public void HungerNotifications_BeforeFirstWarning_ReturnsNull()
    {
        var notifications = new HungerNotifications();
        notifications.Initialize(0); // Start at time 0
        var result = notifications.GetNotification(1999, HungerLevel.WellFed);
        result.Should().BeNull();
    }

    [Test]
    public void HungerNotifications_AtFirstWarning_ReturnsHungryNotification()
    {
        var notifications = new HungerNotifications();
        notifications.Initialize(0); // Start at time 0
        var result = notifications.GetNotification(2000, HungerLevel.WellFed);
        result.Should().Contain("growl from your stomach");
        result.Should().Contain("hungry and thirsty");
    }

    [Test]
    public void HungerNotifications_ProgressionToLevel2_SchedulesNext450Ticks()
    {
        var notifications = new HungerNotifications();
        notifications.Initialize(0); // Start at time 0
        notifications.GetNotification(2000, HungerLevel.WellFed);
        notifications.NextWarningAt.Should().Be(2450); // 2000 + 450
    }

    [Test]
    public void HungerNotifications_AtLevel2Warning_ReturnsRavenousNotification()
    {
        var notifications = new HungerNotifications();
        notifications.Initialize(0); // Start at time 0
        notifications.GetNotification(2000, HungerLevel.WellFed); // First warning
        var result = notifications.GetNotification(2450, HungerLevel.Hungry);
        result.Should().Contain("ravenous");
        result.Should().Contain("parched");
    }

    [Test]
    public void HungerNotifications_ProgressionToLevel3_SchedulesNext150Ticks()
    {
        var notifications = new HungerNotifications();
        notifications.Initialize(0); // Start at time 0
        notifications.GetNotification(2000, HungerLevel.WellFed);
        notifications.GetNotification(2450, HungerLevel.Hungry);
        notifications.NextWarningAt.Should().Be(2600); // 2450 + 150
    }

    [Test]
    public void HungerNotifications_AtLevel3Warning_ReturnsFaintNotification()
    {
        var notifications = new HungerNotifications();
        notifications.Initialize(0); // Start at time 0
        notifications.GetNotification(2000, HungerLevel.WellFed);
        notifications.GetNotification(2450, HungerLevel.Hungry);
        var result = notifications.GetNotification(2600, HungerLevel.Ravenous);
        result.Should().Contain("faint");
        result.Should().Contain("food and liquid");
    }

    [Test]
    public void HungerNotifications_ProgressionToLevel4_SchedulesNext100Ticks()
    {
        var notifications = new HungerNotifications();
        notifications.Initialize(0); // Start at time 0
        notifications.GetNotification(2000, HungerLevel.WellFed);
        notifications.GetNotification(2450, HungerLevel.Hungry);
        notifications.GetNotification(2600, HungerLevel.Ravenous);
        notifications.NextWarningAt.Should().Be(2700); // 2600 + 100
    }

    [Test]
    public void HungerNotifications_AtLevel4Warning_ReturnsAboutToPassOutNotification()
    {
        var notifications = new HungerNotifications();
        notifications.Initialize(0); // Start at time 0
        notifications.GetNotification(2000, HungerLevel.WellFed);
        notifications.GetNotification(2450, HungerLevel.Hungry);
        notifications.GetNotification(2600, HungerLevel.Ravenous);
        var result = notifications.GetNotification(2700, HungerLevel.Faint);
        result.Should().Contain("millichrons");
        result.Should().Contain("pass out");
    }

    [Test]
    public void HungerNotifications_ProgressionToLevel5_SchedulesNext50Ticks()
    {
        var notifications = new HungerNotifications();
        notifications.Initialize(0); // Start at time 0
        notifications.GetNotification(2000, HungerLevel.WellFed);
        notifications.GetNotification(2450, HungerLevel.Hungry);
        notifications.GetNotification(2600, HungerLevel.Ravenous);
        notifications.GetNotification(2700, HungerLevel.Faint);
        notifications.NextWarningAt.Should().Be(2750); // 2700 + 50
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
        var result = notifications.GetNextHungerLevel(2000, HungerLevel.WellFed);
        result.Should().Be(HungerLevel.Hungry);
    }

    [Test]
    public void HungerNotifications_GetNextHungerLevel_BeforeTimeReached_ReturnsNull()
    {
        var notifications = new HungerNotifications();
        notifications.Initialize(0); // Start at time 0
        var result = notifications.GetNextHungerLevel(1999, HungerLevel.WellFed);
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

        var pfContext = (PlanetfallContext)target.Context;
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

        var pfContext = (PlanetfallContext)target.Context;
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

        ((PlanetfallContext)target.Context).Hunger.Should().Be(HungerLevel.WellFed);
    }

    [Test]
    public async Task ProteinLiquid_AfterConsuming_ResetsNotificationTimer()
    {
        var target = GetTarget();
        StartHere<Kitchen>();

        var pfContext = (PlanetfallContext)target.Context;
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

    [Test]
    public async Task ProteinLiquid_InClosedCanteen_CannotDrink()
    {
        var target = GetTarget();
        StartHere<Kitchen>();

        var pfContext = (PlanetfallContext)target.Context;
        pfContext.Hunger = HungerLevel.Hungry;
        PreventHungerAdvancement(pfContext);

        var canteen = GetItem<Canteen>();
        canteen.IsOpen = false;
        canteen.ItemPlacedHere<ProteinLiquid>();
        target.Context.ItemPlacedHere(canteen);

        var response = await target.GetResponse("drink liquid");
        response.Should().Contain("not open");
    }

    #endregion

    #region Goo Tests

    [Test]
    public async Task RedGoo_WhenNotHungry_RejectsConsumption()
    {
        var target = GetTarget();
        StartHere<Kitchen>();

        PreventHungerAdvancement((PlanetfallContext)target.Context);

        var kit = GetItem<SurvivalKit>();
        kit.IsOpen = true;
        target.Context.ItemPlacedHere(kit);

        var response = await target.GetResponse("eat red goo");
        response.Should().Contain("not hungry");
    }

    [Test]
    public async Task RedGoo_WhenHungry_ConsumesWithCherryPieFlavor()
    {
        var target = GetTarget();
        StartHere<Kitchen>();

        var pfContext = (PlanetfallContext)target.Context;
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

        var pfContext = (PlanetfallContext)target.Context;
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

        var pfContext = (PlanetfallContext)target.Context;
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

        var pfContext = (PlanetfallContext)target.Context;
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

        var pfContext = (PlanetfallContext)target.Context;
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

        var pfContext = (PlanetfallContext)target.Context;

        // Initial state
        pfContext.Hunger.Should().Be(HungerLevel.WellFed);

        // Simulate time passage to first warning (2000 ticks / 54 per turn ≈ 37 turns)
        Repository.GetItem<Chronometer>().CurrentTime = 2000;
        pfContext.HungerNotifications.NextWarningAt = 2000;

        var response = await target.GetResponse("look");

        // After processing turn, hunger should advance
        pfContext.Hunger.Should().Be(HungerLevel.Hungry);
    }

    [Test]
    public async Task HungerDeath_AtLevel5_CausesPlayerDeath()
    {
        var target = GetTarget();
        StartHere<Kitchen>();

        var pfContext = (PlanetfallContext)target.Context;
        pfContext.Hunger = HungerLevel.AboutToPassOut;

        // Set up for death
        Repository.GetItem<Chronometer>().CurrentTime = 2750;
        pfContext.HungerNotifications.NextWarningAt = 2750;

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

        var pfContext = (PlanetfallContext)target.Context;
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
