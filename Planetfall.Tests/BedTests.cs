using FluentAssertions;
using Model.Location;
using Planetfall.Item.Kalamontee;
using Planetfall.Location.Kalamontee;
using Planetfall.Location.Kalamontee.Dorm;

namespace Planetfall.Tests;

/// <summary>
/// Tests for the Bed item and BedLocation.
/// </summary>
public class BedTests : EngineTestsBase
{
    #region Bed Item Tests

    [Test]
    public async Task Bed_GetIn_WhenNotTired_ShowsBasicMessage()
    {
        var target = GetTarget();
        StartHere<DormA>();

        var pfContext = target.Context;
        pfContext.Tired = TiredLevel.WellRested;

        var response = await target.GetResponse("get in bed");

        response.Should().Contain("now in bed");
        pfContext.CurrentLocation.Should().BeOfType<BedLocation>();
    }

    [Test]
    public async Task Bed_GetIn_WhenTired_QueuesAutoSleep()
    {
        var target = GetTarget();
        StartHere<DormA>();

        var pfContext = target.Context;
        pfContext.Tired = TiredLevel.Tired;

        var response = await target.GetResponse("get in bed");

        response.Should().Contain("bed is soft and comfortable");
        response.Should().Contain("asleep in short order");
        pfContext.SleepNotifications.FallAsleepQueued.Should().BeTrue();
    }

    [Test]
    public async Task Bed_GetIn_WhenVeryTired_QueuesAutoSleep()
    {
        var target = GetTarget();
        StartHere<DormA>();

        var pfContext = target.Context;
        pfContext.Tired = TiredLevel.VeryTired;

        var response = await target.GetResponse("get in bed");

        response.Should().Contain("asleep in short order");
        pfContext.SleepNotifications.FallAsleepQueued.Should().BeTrue();
    }

    [Test]
    public async Task Bed_GetIn_SetsPlayerInBedFlag()
    {
        var target = GetTarget();
        StartHere<DormA>();

        var bed = GetItem<Bed>();
        bed.PlayerInBed.Should().BeFalse();

        await target.GetResponse("get in bed");

        bed.PlayerInBed.Should().BeTrue();
    }

    [Test]
    public async Task Bed_GetIn_SetsAutoSleepAt16Ticks()
    {
        var target = GetTarget();
        StartHere<DormA>();

        var pfContext = target.Context;
        pfContext.Tired = TiredLevel.Tired;
        var currentTime = pfContext.CurrentTime;

        await target.GetResponse("get in bed");

        pfContext.SleepNotifications.FallAsleepAt.Should().Be(currentTime + 16);
    }

    [Test]
    public async Task Bed_GetOut_WhenNotQueued_AllowsExit()
    {
        var target = GetTarget();
        StartHere<DormA>();

        var pfContext = target.Context;
        pfContext.Tired = TiredLevel.WellRested;

        await target.GetResponse("get in bed");
        var response = await target.GetResponse("get out");

        response.Should().Contain("climb out of the bed");
        pfContext.CurrentLocation.Should().BeOfType<DormA>();
    }

    [Test]
    public async Task Bed_GetOut_WhenFallingAsleep_PreventsTired()
    {
        var target = GetTarget();
        StartHere<DormA>();

        var pfContext = target.Context;
        pfContext.Tired = TiredLevel.Tired;

        await target.GetResponse("get in bed");
        var response = await target.GetResponse("get out");

        response.Should().Contain("How could you suggest such a thing");
        response.Should().Contain("you're so tired");
        response.Should().Contain("bed is so comfy");
        pfContext.CurrentLocation.Should().BeOfType<BedLocation>();
    }

    [Test]
    public async Task Bed_GetOut_ClearsPlayerInBedFlag()
    {
        var target = GetTarget();
        StartHere<DormA>();

        var pfContext = target.Context;
        pfContext.Tired = TiredLevel.WellRested;
        var bed = GetItem<Bed>();

        await target.GetResponse("get in bed");
        await target.GetResponse("get out");

        bed.PlayerInBed.Should().BeFalse();
    }

    [Test]
    public async Task Bed_CanBeAccessedFromAllDorms()
    {
        var target = GetTarget();

        // Test DormA
        StartHere<DormA>();
        var response1 = await target.GetResponse("get in bed");
        response1.Should().Contain("in bed");

        target.Context.CurrentLocation = GetLocation<DormA>();

        // Test DormB
        StartHere<DormB>();
        var response2 = await target.GetResponse("get in bed");
        response2.Should().Contain("in bed");

        target.Context.CurrentLocation = GetLocation<DormB>();

        // Test DormC
        StartHere<DormC>();
        var response3 = await target.GetResponse("get in bed");
        response3.Should().Contain("in bed");

        target.Context.CurrentLocation = GetLocation<DormC>();

        // Test DormD
        StartHere<DormD>();
        var response4 = await target.GetResponse("get in bed");
        response4.Should().Contain("in bed");
    }

    [Test]
    public void Bed_HasCorrectNouns()
    {
        var bed = GetItem<Bed>();
        bed.NounsForMatching.Should().Contain("bed");
        bed.NounsForMatching.Should().Contain("bunk");
        bed.NounsForMatching.Should().Contain("bunks");
    }

    #endregion

    #region BedLocation Tests

    [Test]
    public void BedLocation_IsSubLocation()
    {
        var bedLocation = GetLocation<BedLocation>();
        bedLocation.Should().BeAssignableTo<ISubLocation>();
    }

    [Test]
    public void BedLocation_HasCorrectName()
    {
        var bedLocation = GetLocation<BedLocation>();
        bedLocation.Name.Should().Be("In Bed");
    }

    [Test]
    public void BedLocation_HasCorrectDescription()
    {
        var bedLocation = GetLocation<BedLocation>();
        bedLocation.LocationDescription.Should().Contain("lying in one of the bunk beds");
    }

    [Test]
    public async Task BedLocation_Look_ShowsCorrectDescription()
    {
        var target = GetTarget();
        StartHere<DormA>();

        await target.GetResponse("get in bed");
        var response = await target.GetResponse("look");

        response.Should().Contain("lying in one of the bunk beds");
    }

    [Test]
    public async Task BedLocation_HasNoExits()
    {
        var target = GetTarget();
        StartHere<DormA>();

        await target.GetResponse("get in bed");
        var response = await target.GetResponse("north");

        response.Should().Contain("can't go that way");
    }

    [Test]
    public async Task BedLocation_BeforeEnter_SetsParentLocation()
    {
        var target = GetTarget();
        var dorm = StartHere<DormA>();

        await target.GetResponse("get in bed");

        var bedLocation = target.Context.CurrentLocation as BedLocation;
        bedLocation.Should().NotBeNull();
        bedLocation!.ParentLocation.Should().Be(dorm);
    }

    [Test]
    public async Task BedLocation_GetOut_ReturnsToParentLocation()
    {
        var target = GetTarget();
        var dorm = StartHere<DormA>();

        target.Context.Tired = TiredLevel.WellRested;

        await target.GetResponse("get in bed");
        await target.GetResponse("get out");

        target.Context.CurrentLocation.Should().Be(dorm);
    }

    #endregion

    #region Integration Tests

    [Test]
    public async Task BedSleepCycle_FullFlow_WorksCorrectly()
    {
        var target = GetTarget();
        StartHere<DormA>();

        var pfContext = target.Context;
        pfContext.Tired = TiredLevel.Tired;
        var initialDay = pfContext.Day;

        // Get in bed
        var response1 = await target.GetResponse("get in bed");
        response1.Should().Contain("asleep in short order");

        // Wait for auto-sleep (simulate 16+ turns)
        for (int i = 0; i < 17; i++)
        {
            await target.GetResponse("wait");
        }

        // Should have slept and woken up
        pfContext.Day.Should().Be(initialDay + 1);
        pfContext.Tired.Should().Be(TiredLevel.WellRested);
        pfContext.CurrentLocation.Should().NotBeOfType<BedLocation>();
    }

    [Test]
    public async Task BedSleepCycle_TryToLeaveWhileFallingAsleep_IsPrevented()
    {
        var target = GetTarget();
        StartHere<DormA>();

        var pfContext = target.Context;
        pfContext.Tired = TiredLevel.VeryTired;

        // Get in bed
        await target.GetResponse("get in bed");

        // Try to leave
        var response = await target.GetResponse("get out");

        response.Should().Contain("How could you suggest");
        pfContext.CurrentLocation.Should().BeOfType<BedLocation>();
    }

    #endregion
}
