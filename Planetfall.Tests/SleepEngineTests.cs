using FluentAssertions;
using GameEngine;
using GameEngine.Location;
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
        // After full sleep cycle completes, player wakes up back in the dormitory
        pfContext.CurrentLocation.Should().BeOfType<DormA>();
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
        // After full sleep cycle completes, player wakes up back in the dormitory
        pfContext.CurrentLocation.Should().BeOfType<DormB>();
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
        // After full sleep cycle completes, player wakes up back in the dormitory
        pfContext.CurrentLocation.Should().BeOfType<DormC>();
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
        // After full sleep cycle completes, player wakes up back in the dormitory
        pfContext.CurrentLocation.Should().BeOfType<DormD>();
    }

    [Test]
    public void ProcessForcedSleep_InDormitory_ResetsPlayerInBedAfterWaking()
    {
        var target = GetTarget();
        var pfContext = target.Context;
        StartHere<DormA>();

        var bed = GetItem<Bed>();
        bed.PlayerInBed = false;

        pfContext.Tired = TiredLevel.AboutToDrop;
        pfContext.Day = 1;

        SleepEngine.ProcessForcedSleep(pfContext);

        // After waking up, player is no longer in bed
        bed.PlayerInBed.Should().BeFalse();
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
        pfContext.Hunger.Should().Be(HungerLevel.AboutToPassOut);
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
    public void WakeUp_ExitsBedLocation()
    {
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

        pfContext.CurrentLocation.Should().Be(dorm);
        bed.PlayerInBed.Should().BeFalse();
    }

    [Test]
    public void WakeUp_ResetsSleepNotifications()
    {
        var target = GetTarget();
        var pfContext = target.Context;
        StartHere<BedLocation>();

        pfContext.Day = 1;
        var currentTime = pfContext.CurrentTime;
        pfContext.SleepNotifications.QueueFallAsleep(currentTime);

        SleepEngine.ProcessFallAsleep(pfContext);

        // Should reset for day 2 (5800 ticks)
        pfContext.SleepNotifications.NextWarningAt.Should().Be(currentTime + 5800);
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
