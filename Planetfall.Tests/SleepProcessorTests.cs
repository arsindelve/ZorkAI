using FluentAssertions;
using Planetfall.Location.Kalamontee.Dorm;

namespace Planetfall.Tests;

/// <summary>
/// Tests for the SLEEP command processor.
/// </summary>
public class SleepProcessorTests : EngineTestsBase
{
    [Test]
    public async Task SleepCommand_WhenWellRested_RejectsWithNotTiredMessage()
    {
        var target = GetTarget();
        StartHere<DormA>();

        var pfContext = target.Context;
        pfContext.Tired = TiredLevel.WellRested;

        var response = await target.GetResponse("sleep");

        response.Should().Contain("not tired");
    }

    [Test]
    public async Task SleepCommand_WhenTiredButNotInBed_SuggestsBed()
    {
        var target = GetTarget();
        StartHere<DormA>();

        var pfContext = target.Context;
        pfContext.Tired = TiredLevel.Tired;

        var response = await target.GetResponse("sleep");

        response.Should().Contain("Civilized members");
        response.Should().Contain("sleep in beds");
    }

    [Test]
    public async Task SleepCommand_WhenExhaustedButNotInBed_SuggestsBed()
    {
        var target = GetTarget();
        StartHere<DormA>();

        var pfContext = target.Context;
        pfContext.Tired = TiredLevel.Exhausted;

        var response = await target.GetResponse("sleep");

        response.Should().Contain("Civilized members");
        response.Should().Contain("sleep in beds");
    }

    [Test]
    public async Task SleepCommand_WhenAlreadyFallingAsleep_ReturnsAsleepSoonMessage()
    {
        var target = GetTarget();
        StartHere<DormA>();

        var pfContext = target.Context;
        pfContext.Tired = TiredLevel.Tired;
        pfContext.SleepNotifications.QueueFallAsleep(pfContext.CurrentTime);

        var response = await target.GetResponse("sleep");

        response.Should().Contain("probably be asleep");
        response.Should().Contain("before you know it");
    }

    [Test]
    public async Task SleepCommand_WithVariations_AllWork()
    {
        var target = GetTarget();
        StartHere<DormA>();

        var pfContext = target.Context;
        pfContext.Tired = TiredLevel.WellRested;

        var response1 = await target.GetResponse("sleep");
        var response2 = await target.GetResponse("go to sleep");
        var response3 = await target.GetResponse("rest");

        response1.Should().Contain("not tired");
        response2.Should().Contain("not tired");
        response3.Should().Contain("not tired");
    }
}
