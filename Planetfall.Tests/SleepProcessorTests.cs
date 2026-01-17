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
        // Prevent automatic tiredness progression by setting next warning far in the future
        pfContext.SleepNotifications.NextWarningAt = pfContext.CurrentTime + 10000;
        pfContext.HungerNotifications.NextWarningAt = pfContext.CurrentTime + 10000;

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
        // Prevent automatic tiredness progression by setting next warning far in the future
        pfContext.SleepNotifications.NextWarningAt = pfContext.CurrentTime + 10000;
        pfContext.HungerNotifications.NextWarningAt = pfContext.CurrentTime + 10000;

        var response1 = await target.GetResponse("sleep");
        var response2 = await target.GetResponse("go to sleep");
        var response3 = await target.GetResponse("rest");

        response1.Should().Contain("not tired");
        response2.Should().Contain("not tired");
        response3.Should().Contain("not tired");
    }

    [Test]
    public async Task SleepCommand_AfterSleepCycleCompletes_DoesNotShowNotTiredMessage()
    {
        var target = GetTarget();
        StartHere<DormA>();

        var pfContext = target.Context;
        pfContext.Tired = TiredLevel.Tired;

        // Get in bed to trigger sleep queue
        await target.GetResponse("get in bed");

        // Force the sleep to occur by setting the timer to now
        pfContext.SleepNotifications.FallAsleepAt = pfContext.CurrentTime;

        // This should trigger sleep AND run the sleep command
        // The response should NOT contain "not tired" after the sleep message
        var response = await target.GetResponse("sleep");

        // Should contain sleep/wake messages
        response.Should().Contain("SEPTEM");
        // Should NOT contain "not tired" - that would be redundant
        response.Should().NotContain("not tired");
    }

    [Test]
    public void SleepJustOccurred_InitiallyFalse()
    {
        var target = GetTarget();
        target.Context.SleepJustOccurred.Should().BeFalse();
    }

    [Test]
    public async Task SleepJustOccurred_ResetAfterTurn()
    {
        var target = GetTarget();
        StartHere<DormA>();

        var pfContext = target.Context;

        // Manually set the flag
        pfContext.SleepJustOccurred = true;

        // Process a turn (this should reset the flag at end of turn)
        await target.GetResponse("look");

        pfContext.SleepJustOccurred.Should().BeFalse();
    }
}
