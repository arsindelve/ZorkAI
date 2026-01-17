using FluentAssertions;
using Model.Interface;
using Moq;
using Planetfall.Item.Feinstein;
using Planetfall.Item.Kalamontee;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Planetfall.Location.Kalamontee;
using Planetfall.Location.Kalamontee.Dorm;
using Utilities;

namespace Planetfall.Tests;

/// <summary>
/// Comprehensive tests for the SleepEngine class.
/// Tests voluntary sleep, forced sleep, dangerous sleep, and waking mechanics.
/// </summary>
public class SleepEngineTests : EngineTestsBase
{
    #region CheckForSleep Tests

    [Test]
    public void CheckForSleep_NoSleepConditions_ReturnsNull()
    {
        var target = GetTarget();
        var pfContext = target.Context;
        StartHere<DormA>();

        pfContext.Tired = TiredLevel.WellRested;
        pfContext.SleepNotifications.NextWarningAt = pfContext.CurrentTime + 1000;

        var result = SleepEngine.CheckForSleep(pfContext);
        result.Should().BeNull();
    }

    [Test]
    public void CheckForSleep_FallAsleepQueued_ProcessesVoluntarySleep()
    {
        var target = GetTarget();
        var pfContext = target.Context;
        StartHere<BedLocation>();

        pfContext.Tired = TiredLevel.Tired;
        pfContext.SleepNotifications.QueueFallAsleep(pfContext.CurrentTime - 20);

        var result = SleepEngine.CheckForSleep(pfContext);
        result.Should().NotBeNull();
        result.Should().Contain("deep and restful sleep");
    }

    [Test]
    public void CheckForSleep_ForcedSleepCondition_ProcessesForcedSleep()
    {
        var target = GetTarget();
        var pfContext = target.Context;
        StartHere<DormA>();

        pfContext.Tired = TiredLevel.AboutToDrop;
        pfContext.SleepNotifications.NextWarningAt = pfContext.CurrentTime;

        var result = SleepEngine.CheckForSleep(pfContext);
        result.Should().NotBeNull();
        result.Should().Contain("climb into one of the bunk beds");
    }

    #endregion

    #region ProcessFallAsleep Tests

    [Test]
    public void ProcessFallAsleep_InBed_ReturnsRestfulSleepMessage()
    {
        var target = GetTarget();
        var pfContext = target.Context;
        StartHere<BedLocation>();

        pfContext.SleepNotifications.QueueFallAsleep(pfContext.CurrentTime);
        pfContext.Day = 1;

        var result = SleepEngine.ProcessFallAsleep(pfContext);

        result.Should().Contain("deep and restful sleep");
    }

    [Test]
    public void ProcessFallAsleep_CancelsFallAsleepQueue()
    {
        var target = GetTarget();
        var pfContext = target.Context;
        StartHere<BedLocation>();

        pfContext.SleepNotifications.QueueFallAsleep(pfContext.CurrentTime);
        pfContext.Day = 1;

        SleepEngine.ProcessFallAsleep(pfContext);

        pfContext.SleepNotifications.FallAsleepQueued.Should().BeFalse();
    }

    [Test]
    public void ProcessFallAsleep_AdvancesDay()
    {
        var target = GetTarget();
        var pfContext = target.Context;
        StartHere<BedLocation>();

        pfContext.Day = 2;
        pfContext.SleepNotifications.QueueFallAsleep(pfContext.CurrentTime);

        SleepEngine.ProcessFallAsleep(pfContext);

        pfContext.Day.Should().Be(3);
    }

    [Test]
    public void ProcessFallAsleep_ResetsTiredLevel()
    {
        var target = GetTarget();
        var pfContext = target.Context;
        StartHere<BedLocation>();

        pfContext.Tired = TiredLevel.Exhausted;
        pfContext.Day = 1;
        pfContext.SleepNotifications.QueueFallAsleep(pfContext.CurrentTime);

        SleepEngine.ProcessFallAsleep(pfContext);

        pfContext.Tired.Should().Be(TiredLevel.WellRested);
    }

    [Test]
    public void ProcessFallAsleep_IncludesWakeUpMessage()
    {
        var target = GetTarget();
        var pfContext = target.Context;
        StartHere<BedLocation>();

        pfContext.Day = 1;
        pfContext.SleepNotifications.QueueFallAsleep(pfContext.CurrentTime);

        var result = SleepEngine.ProcessFallAsleep(pfContext);

        result.Should().Contain("SEPTEM");
        result.Should().Contain("wake");
    }

    #endregion

    #region ProcessForcedSleep - In Bed Tests

    [Test]
    public void ProcessForcedSleep_AlreadyInBed_ProcessesSafely()
    {
        var target = GetTarget();
        var pfContext = target.Context;
        StartHere<BedLocation>();

        pfContext.Tired = TiredLevel.AboutToDrop;
        pfContext.Day = 1;

        var result = SleepEngine.ProcessForcedSleep(pfContext);

        result.Should().Contain("deep and blissful sleep");
        pfContext.Day.Should().Be(2);
    }

    #endregion

    #region ProcessForcedSleep - In Dormitory Tests

    [Test]
    public void ProcessForcedSleep_InDormA_ClimbsIntoBedAndWakes()
    {
        var target = GetTarget();
        var pfContext = target.Context;
        StartHere<DormA>();

        pfContext.Tired = TiredLevel.AboutToDrop;
        pfContext.Day = 1;

        var result = SleepEngine.ProcessForcedSleep(pfContext);

        result.Should().Contain("climb into one of the bunk beds");
        result.Should().Contain("immediately fall asleep");
        // Player wakes up still in bed - must manually exit (per original game behavior)
        pfContext.CurrentLocation.Should().BeOfType<BedLocation>();
    }

    [Test]
    public void ProcessForcedSleep_InDormB_ClimbsIntoBedAndWakes()
    {
        var target = GetTarget();
        var pfContext = target.Context;
        StartHere<DormB>();

        pfContext.Tired = TiredLevel.AboutToDrop;
        pfContext.Day = 1;

        var result = SleepEngine.ProcessForcedSleep(pfContext);

        result.Should().Contain("climb into one of the bunk beds");
        // Player wakes up still in bed - must manually exit (per original game behavior)
        pfContext.CurrentLocation.Should().BeOfType<BedLocation>();
    }

    [Test]
    public void ProcessForcedSleep_InDormC_ClimbsIntoBedAndWakes()
    {
        var target = GetTarget();
        var pfContext = target.Context;
        StartHere<DormC>();

        pfContext.Tired = TiredLevel.AboutToDrop;
        pfContext.Day = 1;

        var result = SleepEngine.ProcessForcedSleep(pfContext);

        result.Should().Contain("climb into one of the bunk beds");
        // Player wakes up still in bed - must manually exit (per original game behavior)
        pfContext.CurrentLocation.Should().BeOfType<BedLocation>();
    }

    [Test]
    public void ProcessForcedSleep_InDormD_ClimbsIntoBedAndWakes()
    {
        var target = GetTarget();
        var pfContext = target.Context;
        StartHere<DormD>();

        pfContext.Tired = TiredLevel.AboutToDrop;
        pfContext.Day = 1;

        var result = SleepEngine.ProcessForcedSleep(pfContext);

        result.Should().Contain("climb into one of the bunk beds");
        // Player wakes up still in bed - must manually exit (per original game behavior)
        pfContext.CurrentLocation.Should().BeOfType<BedLocation>();
    }

    [Test]
    public void ProcessForcedSleep_InDormitory_PlayerStillInBedAfterWaking()
    {
        var target = GetTarget();
        var pfContext = target.Context;
        StartHere<DormA>();

        var bed = GetItem<Bed>();
        bed.PlayerInBed = false;

        pfContext.Tired = TiredLevel.AboutToDrop;
        pfContext.Day = 1;

        SleepEngine.ProcessForcedSleep(pfContext);

        // Player wakes up still in bed - must manually exit (per original game behavior)
        bed.PlayerInBed.Should().BeTrue();
    }

    #endregion

    #region ProcessForcedSleep - Ground Sleep Tests

    [Test]
    public void ProcessForcedSleep_OnGround_ReturnsFitfulSleepMessage()
    {
        var target = GetTarget();
        var pfContext = target.Context;
        StartHere<Kitchen>();

        pfContext.Tired = TiredLevel.AboutToDrop;
        pfContext.Day = 2; // Avoid day-specific drowning

        var result = SleepEngine.ProcessForcedSleep(pfContext);

        // Note: This test may include death messages due to randomness
        result.Should().Contain("drop to the ground");
        result.Should().Contain("fitful sleep");
    }

    [Test]
    public void ProcessForcedSleep_AtCragOnDay1_CausesDrowning()
    {
        var target = GetTarget();
        var pfContext = target.Context;
        StartHere<Crag>();

        pfContext.Tired = TiredLevel.AboutToDrop;
        pfContext.Day = 1;

        var result = SleepEngine.ProcessForcedSleep(pfContext);

        result.Should().Contain("wave of water");
        result.Should().Contain("drown");
        result.Should().Contain("You have died");
    }

    [Test]
    public void ProcessForcedSleep_AtBalconyOnDay3_CausesDrowning()
    {
        var target = GetTarget();
        var pfContext = target.Context;
        StartHere<Balcony>();

        pfContext.Tired = TiredLevel.AboutToDrop;
        pfContext.Day = 3;

        var result = SleepEngine.ProcessForcedSleep(pfContext);

        result.Should().Contain("wave of water");
        result.Should().Contain("drown");
    }

    [Test]
    public void ProcessForcedSleep_AtWindingStairOnDay5_CausesDrowning()
    {
        var target = GetTarget();
        var pfContext = target.Context;
        StartHere<WindingStair>();

        pfContext.Tired = TiredLevel.AboutToDrop;
        pfContext.Day = 5;

        var result = SleepEngine.ProcessForcedSleep(pfContext);

        result.Should().Contain("wave of water");
        result.Should().Contain("drown");
    }

    [Test]
    public void ProcessForcedSleep_AtCragOnDay2_DoesNotDrown()
    {
        var target = GetTarget();
        var pfContext = target.Context;
        StartHere<Crag>();

        pfContext.Tired = TiredLevel.AboutToDrop;
        pfContext.Day = 2; // Not the drowning day

        var result = SleepEngine.ProcessForcedSleep(pfContext);

        // Should either survive or die to beasts, but not drown
        if (result.Contains("You have died"))
        {
            result.Should().NotContain("drown");
            result.Should().Contain("beasts");
        }
        else
        {
            result.Should().Contain("SEPTEM"); // Woke up successfully
        }
    }

    [Test]
    public void ProcessForcedSleep_OnGround_DropsItemsToCurrentLocation()
    {
        var target = GetTarget();
        var pfContext = target.Context;
        var kitchen = StartHere<Kitchen>();

        // Add item to inventory
        var brush = GetItem<Brush>();
        pfContext.ItemPlacedHere(brush);
        pfContext.Items.Should().Contain(brush);

        pfContext.Tired = TiredLevel.AboutToDrop;
        pfContext.Day = 2; // Avoid day-specific drowning

        // Mock chooser to ensure player survives (RollDice > 30 means no beast attack)
        var mockChooser = new Mock<IRandomChooser>();
        mockChooser.Setup(c => c.RollDice(100)).Returns(50); // > 30, survives
        mockChooser.Setup(c => c.RollDice(5)).Returns(1); // Dream selection
        SleepEngine.Chooser = mockChooser.Object;

        try
        {
            var result = SleepEngine.ProcessForcedSleep(pfContext);

            // Should have survived
            result.Should().NotContain("You have died");
            result.Should().Contain("SEPTEM");

            // Items should be dropped to the floor (current location)
            pfContext.Items.Should().NotContain(brush);
            kitchen.Items.Should().Contain(brush);
        }
        finally
        {
            // Reset static chooser
            SleepEngine.Chooser = new GameEngine.RandomChooser();
        }
    }

    #endregion

    #region Wake Up Tests

    [Test]
    public void WakeUp_InBed_ShowsRefreshedMessage()
    {
        var target = GetTarget();
        var pfContext = target.Context;
        StartHere<BedLocation>();

        pfContext.Day = 1;
        pfContext.SleepNotifications.QueueFallAsleep(pfContext.CurrentTime);

        var result = SleepEngine.ProcessFallAsleep(pfContext);

        result.Should().Contain("refreshed");
        result.Should().Contain("ready to face the challenges");
    }

    [Test]
    public void WakeUp_OnGround_ShowsStiffMessage()
    {
        var target = GetTarget();
        var pfContext = target.Context;
        StartHere<Kitchen>();

        pfContext.Tired = TiredLevel.AboutToDrop;
        pfContext.Day = 2;

        var result = SleepEngine.ProcessForcedSleep(pfContext);

        // Only check if player survived
        if (!result.Contains("You have died"))
        {
            result.Should().Contain("stiff from your night on the floor");
        }
    }

    [Test]
    public void WakeUp_OnDay9_CausesDeath()
    {
        var target = GetTarget();
        var pfContext = target.Context;
        StartHere<BedLocation>();

        pfContext.Day = 8;
        pfContext.SleepNotifications.QueueFallAsleep(pfContext.CurrentTime);

        var result = SleepEngine.ProcessFallAsleep(pfContext);

        result.Should().Contain("don't seem to have survived the night");
        result.Should().Contain("You have died");
    }

    [Test]
    public void WakeUp_DropsNonWornItems()
    {
        var target = GetTarget();
        var pfContext = target.Context;
        var dormA = GetLocation<DormA>();
        var location = StartHere<BedLocation>();
        // Set parent location so waking up returns to DormA
        location.ParentLocation = dormA;

        var brush = GetItem<Brush>();
        pfContext.ItemPlacedHere(brush);

        pfContext.Day = 1;
        pfContext.SleepNotifications.QueueFallAsleep(pfContext.CurrentTime);

        SleepEngine.ProcessFallAsleep(pfContext);

        pfContext.Items.Should().NotContain(brush);
        dormA.Items.Should().Contain(brush);
    }

    [Test]
    public void WakeUp_DoesNotDropWornItems()
    {
        var target = GetTarget();
        var pfContext = target.Context;
        StartHere<BedLocation>();

        var uniform = GetItem<PatrolUniform>();
        uniform.BeingWorn = true;
        pfContext.ItemPlacedHere(uniform);

        pfContext.Day = 1;
        pfContext.SleepNotifications.QueueFallAsleep(pfContext.CurrentTime);

        SleepEngine.ProcessFallAsleep(pfContext);

        pfContext.Items.Should().Contain(uniform);
    }

    [Test]
    public void WakeUp_SpoilsProteinLiquidInOpenCanteen()
    {
        var target = GetTarget();
        var pfContext = target.Context;
        StartHere<BedLocation>();

        var canteen = GetItem<Canteen>();
        canteen.IsOpen = true;
        var proteinLiquid = GetItem<ProteinLiquid>();
        canteen.ItemPlacedHere(proteinLiquid);
        pfContext.ItemPlacedHere(canteen);

        pfContext.Day = 1;
        pfContext.SleepNotifications.QueueFallAsleep(pfContext.CurrentTime);

        SleepEngine.ProcessFallAsleep(pfContext);

        canteen.Items.Should().NotContain(proteinLiquid);
    }

    [Test]
    public void WakeUp_DoesNotSpoilProteinLiquidInClosedCanteen()
    {
        var target = GetTarget();
        var pfContext = target.Context;
        StartHere<BedLocation>();

        var canteen = GetItem<Canteen>();
        canteen.IsOpen = false;
        var proteinLiquid = GetItem<ProteinLiquid>();
        canteen.ItemPlacedHere(proteinLiquid);
        pfContext.ItemPlacedHere(canteen);

        pfContext.Day = 1;
        pfContext.SleepNotifications.QueueFallAsleep(pfContext.CurrentTime);

        SleepEngine.ProcessFallAsleep(pfContext);

        canteen.Items.Should().Contain(proteinLiquid);
    }

    [Test]
    public void WakeUp_AdjustsHungerIfAboveWellFed()
    {
        var target = GetTarget();
        var pfContext = target.Context;
        StartHere<BedLocation>();

        pfContext.Hunger = HungerLevel.Ravenous;
        pfContext.Day = 1;
        pfContext.SleepNotifications.QueueFallAsleep(pfContext.CurrentTime);

        var result = SleepEngine.ProcessFallAsleep(pfContext);

        result.Should().Contain("incredibly famished");
        result.Should().Contain("get some breakfast");
        // Override from original: reset to WellFed for more forgiving gameplay (~31 turns to find food)
        pfContext.Hunger.Should().Be(HungerLevel.WellFed);
    }

    [Test]
    public void WakeUp_WhenHungry_SetsHungerTimerTo200Ticks()
    {
        var target = GetTarget();
        var pfContext = target.Context;
        StartHere<BedLocation>();

        pfContext.Hunger = HungerLevel.Hungry;
        pfContext.Day = 1;
        pfContext.SleepNotifications.QueueFallAsleep(pfContext.CurrentTime);

        SleepEngine.ProcessFallAsleep(pfContext);

        // Chronometer is reset to morning on wake-up, so check offset from NEW current time
        // 200 ticks = ~4 turns (4 * 54 = 216 ticks) to find food before first warning
        var ticksUntilWarning = pfContext.HungerNotifications.NextWarningAt - pfContext.CurrentTime;
        ticksUntilWarning.Should().Be(200);
    }

    [Test]
    public void WakeUp_WhenWellFed_SetsHungerTimerTo800Ticks()
    {
        var target = GetTarget();
        var pfContext = target.Context;
        StartHere<BedLocation>();

        pfContext.Hunger = HungerLevel.WellFed;
        pfContext.Day = 1;
        pfContext.SleepNotifications.QueueFallAsleep(pfContext.CurrentTime);

        SleepEngine.ProcessFallAsleep(pfContext);

        // Chronometer is reset to morning on wake-up, so check offset from NEW current time
        var ticksUntilWarning = pfContext.HungerNotifications.NextWarningAt - pfContext.CurrentTime;
        ticksUntilWarning.Should().Be(800);
    }

    [Test]
    public void WakeUp_WhenHungry_PlayerHasManyTurnsToFindFood()
    {
        // Regression test: Player was dying immediately after waking because
        // hunger was set to AboutToPassOut with only 100 ticks delay
        // Fix: Reset to WellFed, giving ~31 turns total before death
        var target = GetTarget();
        var pfContext = target.Context;
        StartHere<BedLocation>();

        pfContext.Hunger = HungerLevel.Hungry;
        pfContext.Day = 1;
        pfContext.SleepNotifications.QueueFallAsleep(pfContext.CurrentTime);

        SleepEngine.ProcessFallAsleep(pfContext);

        // Hunger should be reset to WellFed
        pfContext.Hunger.Should().Be(HungerLevel.WellFed);

        // First warning at 200 ticks (~4 turns), then full progression to death
        // Total: 200 + 900 + 300 + 200 + 100 = 1700 ticks (~31 turns)
        var ticksUntilFirstWarning = pfContext.HungerNotifications.NextWarningAt - pfContext.CurrentTime;
        ticksUntilFirstWarning.Should().Be(200);
    }

    [Test]
    public void WakeUp_DoesNotAdjustHungerIfWellFed()
    {
        var target = GetTarget();
        var pfContext = target.Context;
        StartHere<BedLocation>();

        pfContext.Hunger = HungerLevel.WellFed;
        pfContext.Day = 1;
        pfContext.SleepNotifications.QueueFallAsleep(pfContext.CurrentTime);

        var result = SleepEngine.ProcessFallAsleep(pfContext);

        result.Should().NotContain("famished");
        pfContext.Hunger.Should().Be(HungerLevel.WellFed);
    }

    [Test]
    public void WakeUp_FloydGreetsPlayerInBed()
    {
        var target = GetTarget();
        var pfContext = target.Context;
        StartHere<BedLocation>();

        var floyd = GetItem<Floyd>();
        floyd.HasEverBeenOn = true;
        floyd.IsOn = true;

        pfContext.Day = 1;
        pfContext.SleepNotifications.QueueFallAsleep(pfContext.CurrentTime);

        var result = SleepEngine.ProcessFallAsleep(pfContext);

        result.Should().Contain("Floyd");
        result.Should().Contain("lazy bones");
    }

    [Test]
    public void WakeUp_FloydGreetsPlayerOnGround()
    {
        var target = GetTarget();
        var pfContext = target.Context;
        StartHere<Kitchen>();

        var floyd = GetItem<Floyd>();
        floyd.HasEverBeenOn = true;
        floyd.IsOn = true;

        pfContext.Tired = TiredLevel.AboutToDrop;
        pfContext.Day = 2;

        var result = SleepEngine.ProcessForcedSleep(pfContext);

        // Only check if player survived
        if (!result.Contains("You have died"))
        {
            result.Should().Contain("Floyd");
            result.Should().Contain("sleeping on the floor");
        }
    }

    [Test]
    public void WakeUp_StaysInBedLocation()
    {
        // Per original game behavior: player wakes up still in bed and must manually exit
        var target = GetTarget();
        var pfContext = target.Context;
        var dorm = GetLocation<DormA>();
        var bedLocation = StartHere<BedLocation>();
        bedLocation.ParentLocation = dorm;

        var bed = GetItem<Bed>();
        bed.PlayerInBed = true;

        pfContext.Day = 1;
        pfContext.SleepNotifications.QueueFallAsleep(pfContext.CurrentTime);

        SleepEngine.ProcessFallAsleep(pfContext);

        pfContext.CurrentLocation.Should().Be(bedLocation);
        bed.PlayerInBed.Should().BeTrue();
    }

    [Test]
    public void WakeUp_ResetsSleepNotifications()
    {
        var target = GetTarget();
        var pfContext = target.Context;
        StartHere<BedLocation>();

        pfContext.Day = 1;
        pfContext.SleepNotifications.QueueFallAsleep(pfContext.CurrentTime);

        SleepEngine.ProcessFallAsleep(pfContext);

        // Chronometer is reset to morning on wake-up, so check offset from NEW current time
        // Should reset for day 2 (5800 ticks until first sleep warning)
        var ticksUntilWarning = pfContext.SleepNotifications.NextWarningAt - pfContext.CurrentTime;
        ticksUntilWarning.Should().Be(5800);
    }

    [Test]
    public void WakeUp_EnablesNextSicknessCheck()
    {
        var target = GetTarget();
        var pfContext = target.Context;
        StartHere<BedLocation>();

        pfContext.Day = 2;
        pfContext.SicknessNotifications.DaysNotified.Add(3); // Block day 3

        pfContext.SleepNotifications.QueueFallAsleep(pfContext.CurrentTime);

        SleepEngine.ProcessFallAsleep(pfContext);

        // Day advanced to 3, so day 3 should be removed from blocked list
        pfContext.SicknessNotifications.DaysNotified.Should().NotContain(3);
    }

    #endregion

    #region TiredLevel Enum Tests

    [Test]
    public void TiredLevel_WellRested_HasCorrectValue()
    {
        ((int)TiredLevel.WellRested).Should().Be(0);
    }

    [Test]
    public void TiredLevel_Tired_HasCorrectValue()
    {
        ((int)TiredLevel.Tired).Should().Be(1);
    }

    [Test]
    public void TiredLevel_AboutToDrop_HasCorrectValue()
    {
        ((int)TiredLevel.AboutToDrop).Should().Be(4);
    }

    [Test]
    public void TiredLevel_AllLevels_HaveNotifications()
    {
        TiredLevel.Tired.GetNotification().Should().NotBeNullOrEmpty();
        TiredLevel.VeryTired.GetNotification().Should().NotBeNullOrEmpty();
        TiredLevel.Exhausted.GetNotification().Should().NotBeNullOrEmpty();
        TiredLevel.AboutToDrop.GetNotification().Should().NotBeNullOrEmpty();
    }

    [Test]
    public void TiredLevel_AllLevels_HaveDescriptions()
    {
        TiredLevel.WellRested.GetDescription().Should().NotBeNullOrEmpty();
        TiredLevel.Tired.GetDescription().Should().NotBeNullOrEmpty();
        TiredLevel.VeryTired.GetDescription().Should().NotBeNullOrEmpty();
        TiredLevel.Exhausted.GetDescription().Should().NotBeNullOrEmpty();
        TiredLevel.AboutToDrop.GetDescription().Should().NotBeNullOrEmpty();
    }

    #endregion
}
