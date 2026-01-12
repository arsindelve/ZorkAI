using FluentAssertions;

namespace Planetfall.Tests;

/// <summary>
/// Comprehensive tests for the SleepNotifications class.
/// Tests the progressive fatigue warning system with day-specific timing.
/// </summary>
public class SleepNotificationsTests
{
    #region Initialization Tests

    [Test]
    public void SleepNotifications_InitialState_DefaultsToZero()
    {
        var notifications = new SleepNotifications();
        notifications.NextWarningAt.Should().Be(0);
        notifications.FallAsleepQueued.Should().BeFalse();
        notifications.FallAsleepAt.Should().Be(0);
    }

    [Test]
    public void SleepNotifications_AfterInitialize_SetsNextWarningAt3600TicksFromStart()
    {
        var notifications = new SleepNotifications();
        notifications.Initialize(4500); // Typical starting time
        notifications.NextWarningAt.Should().Be(8100); // 4500 + 3600
    }

    [Test]
    public void SleepNotifications_Initialize_AtTimeZero_SetsCorrectWarningTime()
    {
        var notifications = new SleepNotifications();
        notifications.Initialize(0);
        notifications.NextWarningAt.Should().Be(3600);
    }

    #endregion

    #region Warning Notification Tests

    [Test]
    public void SleepNotifications_BeforeFirstWarning_ReturnsNull()
    {
        var notifications = new SleepNotifications();
        notifications.Initialize(0);
        var result = notifications.GetNotification(3599, TiredLevel.WellRested);
        result.Should().BeNull();
    }

    [Test]
    public void SleepNotifications_AtFirstWarning_ReturnsTiredNotification()
    {
        var notifications = new SleepNotifications();
        notifications.Initialize(0);
        var result = notifications.GetNotification(3600, TiredLevel.WellRested);
        result.Should().Contain("begin to feel weary");
        result.Should().Contain("finding a nice safe place to sleep");
    }

    [Test]
    public void SleepNotifications_ProgressionToVeryTired_SchedulesNext400Ticks()
    {
        var notifications = new SleepNotifications();
        notifications.Initialize(0);
        notifications.GetNotification(3600, TiredLevel.WellRested);
        notifications.NextWarningAt.Should().Be(4000); // 3600 + 400
    }

    [Test]
    public void SleepNotifications_AtVeryTiredWarning_ReturnsReallyTiredNotification()
    {
        var notifications = new SleepNotifications();
        notifications.Initialize(0);
        notifications.GetNotification(3600, TiredLevel.WellRested);
        var result = notifications.GetNotification(4000, TiredLevel.Tired);
        result.Should().Contain("really tired");
        result.Should().Contain("find a place to sleep real soon");
    }

    [Test]
    public void SleepNotifications_ProgressionToExhausted_SchedulesNext135Ticks()
    {
        var notifications = new SleepNotifications();
        notifications.Initialize(0);
        notifications.GetNotification(3600, TiredLevel.WellRested);
        notifications.GetNotification(4000, TiredLevel.Tired);
        notifications.NextWarningAt.Should().Be(4135); // 4000 + 135
    }

    [Test]
    public void SleepNotifications_AtExhaustedWarning_ReturnsDropWarning()
    {
        var notifications = new SleepNotifications();
        notifications.Initialize(0);
        notifications.GetNotification(3600, TiredLevel.WellRested);
        notifications.GetNotification(4000, TiredLevel.Tired);
        var result = notifications.GetNotification(4135, TiredLevel.VeryTired);
        result.Should().Contain("don't get some sleep");
        result.Should().Contain("probably drop");
    }

    [Test]
    public void SleepNotifications_ProgressionToAboutToDrop_SchedulesNext60Ticks()
    {
        var notifications = new SleepNotifications();
        notifications.Initialize(0);
        notifications.GetNotification(3600, TiredLevel.WellRested);
        notifications.GetNotification(4000, TiredLevel.Tired);
        notifications.GetNotification(4135, TiredLevel.VeryTired);
        notifications.NextWarningAt.Should().Be(4195); // 4135 + 60
    }

    [Test]
    public void SleepNotifications_AtAboutToDropWarning_ReturnsBarelyKeepEyesOpenWarning()
    {
        var notifications = new SleepNotifications();
        notifications.Initialize(0);
        notifications.GetNotification(3600, TiredLevel.WellRested);
        notifications.GetNotification(4000, TiredLevel.Tired);
        notifications.GetNotification(4135, TiredLevel.VeryTired);
        var result = notifications.GetNotification(4195, TiredLevel.Exhausted);
        result.Should().Contain("barely keep your eyes open");
    }

    [Test]
    public void SleepNotifications_ProgressionToForcedSleep_SchedulesNext50Ticks()
    {
        var notifications = new SleepNotifications();
        notifications.Initialize(0);
        notifications.GetNotification(3600, TiredLevel.WellRested);
        notifications.GetNotification(4000, TiredLevel.Tired);
        notifications.GetNotification(4135, TiredLevel.VeryTired);
        notifications.GetNotification(4195, TiredLevel.Exhausted);
        notifications.NextWarningAt.Should().Be(4245); // 4195 + 50
    }

    #endregion

    #region Daily Reset Tests

    [Test]
    public void SleepNotifications_ResetForNewDay_Day1_SetsCorrectInterval()
    {
        var notifications = new SleepNotifications();
        notifications.ResetForNewDay(5000, 1);
        notifications.NextWarningAt.Should().Be(8600); // 5000 + 3600
        notifications.FallAsleepQueued.Should().BeFalse();
        notifications.FallAsleepAt.Should().Be(0);
    }

    [Test]
    public void SleepNotifications_ResetForNewDay_Day2_SetsCorrectInterval()
    {
        var notifications = new SleepNotifications();
        notifications.ResetForNewDay(5000, 2);
        notifications.NextWarningAt.Should().Be(10800); // 5000 + 5800
    }

    [Test]
    public void SleepNotifications_ResetForNewDay_Day3_SetsCorrectInterval()
    {
        var notifications = new SleepNotifications();
        notifications.ResetForNewDay(5000, 3);
        notifications.NextWarningAt.Should().Be(10550); // 5000 + 5550
    }

    [Test]
    public void SleepNotifications_ResetForNewDay_Day4_SetsCorrectInterval()
    {
        var notifications = new SleepNotifications();
        notifications.ResetForNewDay(5000, 4);
        notifications.NextWarningAt.Should().Be(10200); // 5000 + 5200
    }

    [Test]
    public void SleepNotifications_ResetForNewDay_Day5_SetsCorrectInterval()
    {
        var notifications = new SleepNotifications();
        notifications.ResetForNewDay(5000, 5);
        notifications.NextWarningAt.Should().Be(9800); // 5000 + 4800
    }

    [Test]
    public void SleepNotifications_ResetForNewDay_Day6_SetsCorrectInterval()
    {
        var notifications = new SleepNotifications();
        notifications.ResetForNewDay(5000, 6);
        notifications.NextWarningAt.Should().Be(9300); // 5000 + 4300
    }

    [Test]
    public void SleepNotifications_ResetForNewDay_Day7_SetsCorrectInterval()
    {
        var notifications = new SleepNotifications();
        notifications.ResetForNewDay(5000, 7);
        notifications.NextWarningAt.Should().Be(8700); // 5000 + 3700
    }

    [Test]
    public void SleepNotifications_ResetForNewDay_Day8_SetsCorrectInterval()
    {
        var notifications = new SleepNotifications();
        notifications.ResetForNewDay(5000, 8);
        notifications.NextWarningAt.Should().Be(8000); // 5000 + 3000
    }

    [Test]
    public void SleepNotifications_ResetForNewDay_Day9_DefaultsToDay8Timing()
    {
        var notifications = new SleepNotifications();
        notifications.ResetForNewDay(5000, 9);
        notifications.NextWarningAt.Should().Be(8000); // 5000 + 3000 (same as day 8)
    }

    [Test]
    public void SleepNotifications_ResetForNewDay_CancelsFallAsleepQueue()
    {
        var notifications = new SleepNotifications();
        notifications.QueueFallAsleep(1000);
        notifications.FallAsleepQueued.Should().BeTrue();

        notifications.ResetForNewDay(2000, 2);

        notifications.FallAsleepQueued.Should().BeFalse();
        notifications.FallAsleepAt.Should().Be(0);
    }

    #endregion

    #region Next Tired Level Tests

    [Test]
    public void SleepNotifications_GetNextTiredLevel_BeforeWarningTime_ReturnsNull()
    {
        var notifications = new SleepNotifications();
        notifications.Initialize(0);
        var result = notifications.GetNextTiredLevel(3599, TiredLevel.WellRested);
        result.Should().BeNull();
    }

    [Test]
    public void SleepNotifications_GetNextTiredLevel_AtWarningTime_ReturnsNextLevel()
    {
        var notifications = new SleepNotifications();
        notifications.Initialize(0);
        var result = notifications.GetNextTiredLevel(3600, TiredLevel.WellRested);
        result.Should().Be(TiredLevel.Tired);
    }

    [Test]
    public void SleepNotifications_GetNextTiredLevel_AfterWarningTime_ReturnsNextLevel()
    {
        var notifications = new SleepNotifications();
        notifications.Initialize(0);
        var result = notifications.GetNextTiredLevel(4000, TiredLevel.WellRested);
        result.Should().Be(TiredLevel.Tired);
    }

    [Test]
    public void SleepNotifications_GetNextTiredLevel_AtAboutToDrop_ReturnsNull()
    {
        var notifications = new SleepNotifications();
        notifications.Initialize(0);
        notifications.NextWarningAt = 4000;
        var result = notifications.GetNextTiredLevel(4000, TiredLevel.AboutToDrop);
        result.Should().BeNull(); // Can't go beyond AboutToDrop
    }

    #endregion

    #region Fall Asleep Queue Tests

    [Test]
    public void SleepNotifications_QueueFallAsleep_SetsQueuedFlag()
    {
        var notifications = new SleepNotifications();
        notifications.QueueFallAsleep(1000);

        notifications.FallAsleepQueued.Should().BeTrue();
        notifications.FallAsleepAt.Should().Be(1016); // 1000 + 16
    }

    [Test]
    public void SleepNotifications_CancelFallAsleep_ClearsQueuedFlag()
    {
        var notifications = new SleepNotifications();
        notifications.QueueFallAsleep(1000);
        notifications.CancelFallAsleep();

        notifications.FallAsleepQueued.Should().BeFalse();
        notifications.FallAsleepAt.Should().Be(0);
    }

    [Test]
    public void SleepNotifications_ShouldFallAsleep_BeforeTime_ReturnsFalse()
    {
        var notifications = new SleepNotifications();
        notifications.QueueFallAsleep(1000);

        notifications.ShouldFallAsleep(1015).Should().BeFalse();
    }

    [Test]
    public void SleepNotifications_ShouldFallAsleep_AtTime_ReturnsTrue()
    {
        var notifications = new SleepNotifications();
        notifications.QueueFallAsleep(1000);

        notifications.ShouldFallAsleep(1016).Should().BeTrue();
    }

    [Test]
    public void SleepNotifications_ShouldFallAsleep_AfterTime_ReturnsTrue()
    {
        var notifications = new SleepNotifications();
        notifications.QueueFallAsleep(1000);

        notifications.ShouldFallAsleep(1100).Should().BeTrue();
    }

    [Test]
    public void SleepNotifications_ShouldFallAsleep_NotQueued_ReturnsFalse()
    {
        var notifications = new SleepNotifications();
        notifications.ShouldFallAsleep(1000).Should().BeFalse();
    }

    #endregion

    #region Forced Sleep Tests

    [Test]
    public void SleepNotifications_ShouldForceSleep_AtAboutToDropAndTimeReached_ReturnsTrue()
    {
        var notifications = new SleepNotifications();
        notifications.NextWarningAt = 1000;

        notifications.ShouldForceSleep(1000, TiredLevel.AboutToDrop).Should().BeTrue();
    }

    [Test]
    public void SleepNotifications_ShouldForceSleep_NotAboutToDrop_ReturnsFalse()
    {
        var notifications = new SleepNotifications();
        notifications.NextWarningAt = 1000;

        notifications.ShouldForceSleep(1000, TiredLevel.Exhausted).Should().BeFalse();
    }

    [Test]
    public void SleepNotifications_ShouldForceSleep_BeforeTime_ReturnsFalse()
    {
        var notifications = new SleepNotifications();
        notifications.NextWarningAt = 1000;

        notifications.ShouldForceSleep(999, TiredLevel.AboutToDrop).Should().BeFalse();
    }

    [Test]
    public void SleepNotifications_ShouldForceSleep_WellRested_ReturnsFalse()
    {
        var notifications = new SleepNotifications();
        notifications.NextWarningAt = 1000;

        notifications.ShouldForceSleep(1000, TiredLevel.WellRested).Should().BeFalse();
    }

    #endregion
}
