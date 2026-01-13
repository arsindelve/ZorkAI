using FluentAssertions;
using Model.Interface;
using Moq;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Planetfall.Location.Kalamontee.Dorm;

namespace Planetfall.Tests;

/// <summary>
/// Tests for the Dreams system using mocked IRandomChooser for deterministic behavior.
/// </summary>
public class DreamsTests : EngineTestsBase
{
    [Test]
    public void Dreams_GetDream_WithFloydNeverOn_DoesNotReturnFloydDream()
    {
        var target = GetTarget();
        var pfContext = target.Context;

        var floyd = GetItem<Floyd>();
        floyd.HasEverBeenOn = false;

        // When Floyd is not on, first RollDice(100) is for normal dream check
        // Return 1 which would trigger normal dream
        var mockChooser = new Mock<IRandomChooser>();
        mockChooser.Setup(c => c.RollDice(100)).Returns(1); // Triggers normal dream
        mockChooser.Setup(c => c.RollDice(5)).Returns(1); // Feinstein dream

        var dream = Dreams.GetDream(pfContext, mockChooser.Object);

        // Should not contain Floyd since Floyd has never been on
        dream.Should().NotBeNull();
        dream.Should().NotContain("office");
        dream.Should().Contain("Feinstein"); // Got a normal dream instead
    }

    [Test]
    public void Dreams_GetDream_WithFloydOn_AndFloydRollSucceeds_ReturnsFloydDream()
    {
        var target = GetTarget();
        var pfContext = target.Context;

        var floyd = GetItem<Floyd>();
        floyd.HasEverBeenOn = true;

        // Mock: first roll is 13 or less (triggers Floyd dream)
        var mockChooser = new Mock<IRandomChooser>();
        mockChooser.Setup(c => c.RollDice(100)).Returns(5);

        var dream = Dreams.GetDream(pfContext, mockChooser.Object);

        dream.Should().NotBeNull();
        dream.Should().Contain("Floyd");
        dream.Should().Contain("office");
    }

    [Test]
    public void Dreams_GetDream_WithFloydOn_AndFloydRollFails_ChecksNormalDream()
    {
        var target = GetTarget();
        var pfContext = target.Context;

        var floyd = GetItem<Floyd>();
        floyd.HasEverBeenOn = true;

        // Mock: first roll is > 13 (Floyd check fails), second roll is <= 60 (normal dream), third roll picks dream index
        var mockChooser = new Mock<IRandomChooser>();
        var rollSequence = mockChooser.SetupSequence(c => c.RollDice(100));
        rollSequence.Returns(50);  // Floyd check - > 13, fails
        rollSequence.Returns(30);  // Normal dream check - <= 60, succeeds
        mockChooser.Setup(c => c.RollDice(5)).Returns(1); // Pick first dream (Feinstein)

        var dream = Dreams.GetDream(pfContext, mockChooser.Object);

        dream.Should().NotBeNull();
        dream.Should().Contain("Feinstein");
    }

    [Test]
    public void Dreams_GetDream_FloydDream_HasCorrectContent()
    {
        var target = GetTarget();
        var pfContext = target.Context;

        var floyd = GetItem<Floyd>();
        floyd.HasEverBeenOn = true;

        var mockChooser = new Mock<IRandomChooser>();
        mockChooser.Setup(c => c.RollDice(100)).Returns(10); // Triggers Floyd dream

        var dream = Dreams.GetDream(pfContext, mockChooser.Object);

        dream.Should().NotBeNull();
        dream.Should().Contain("busy office");
        dream.Should().Contain("carrying papers");
        dream.Should().Contain("delivering coffee");
        dream.Should().Contain("tell him a story");
        dream.Should().Contain("trusting eyes");
    }

    [Test]
    public void Dreams_GetDream_CanReturnNormalDream_Feinstein()
    {
        var target = GetTarget();
        var pfContext = target.Context;

        var mockChooser = new Mock<IRandomChooser>();
        var rollSequence = mockChooser.SetupSequence(c => c.RollDice(100));
        rollSequence.Returns(50);  // Floyd check fails (no Floyd on)
        rollSequence.Returns(30);  // Normal dream check succeeds
        mockChooser.Setup(c => c.RollDice(5)).Returns(1); // Pick first dream

        var dream = Dreams.GetDream(pfContext, mockChooser.Object);

        dream.Should().NotBeNull();
        dream.Should().Contain("Feinstein");
        dream.Should().Contain("Blather");
        dream.Should().Contain("self-destruct");
    }

    [Test]
    public void Dreams_GetDream_CanReturnNormalDream_Ramos()
    {
        var target = GetTarget();
        var pfContext = target.Context;

        var mockChooser = new Mock<IRandomChooser>();
        var rollSequence = mockChooser.SetupSequence(c => c.RollDice(100));
        rollSequence.Returns(50);  // Floyd check fails
        rollSequence.Returns(30);  // Normal dream check succeeds
        mockChooser.Setup(c => c.RollDice(5)).Returns(2); // Pick second dream

        var dream = Dreams.GetDream(pfContext, mockChooser.Object);

        dream.Should().NotBeNull();
        dream.Should().Contain("Ramos");
        dream.Should().Contain("Fire Nectar");
    }

    [Test]
    public void Dreams_GetDream_CanReturnNormalDream_Gallium()
    {
        var target = GetTarget();
        var pfContext = target.Context;

        var mockChooser = new Mock<IRandomChooser>();
        var rollSequence = mockChooser.SetupSequence(c => c.RollDice(100));
        rollSequence.Returns(50);  // Floyd check fails
        rollSequence.Returns(30);  // Normal dream check succeeds
        mockChooser.Setup(c => c.RollDice(5)).Returns(3); // Pick third dream

        var dream = Dreams.GetDream(pfContext, mockChooser.Object);

        dream.Should().NotBeNull();
        dream.Should().Contain("Gallium");
        dream.Should().Contain("sponge-cat");
    }

    [Test]
    public void Dreams_GetDream_CanReturnNormalDream_Waterfall()
    {
        var target = GetTarget();
        var pfContext = target.Context;

        var mockChooser = new Mock<IRandomChooser>();
        var rollSequence = mockChooser.SetupSequence(c => c.RollDice(100));
        rollSequence.Returns(50);  // Floyd check fails
        rollSequence.Returns(30);  // Normal dream check succeeds
        mockChooser.Setup(c => c.RollDice(5)).Returns(4); // Pick fourth dream

        var dream = Dreams.GetDream(pfContext, mockChooser.Object);

        dream.Should().NotBeNull();
        dream.Should().Contain("waterfall");
        dream.Should().Contain("rainbow");
    }

    [Test]
    public void Dreams_GetDream_CanReturnNormalDream_Nebulon()
    {
        var target = GetTarget();
        var pfContext = target.Context;

        var mockChooser = new Mock<IRandomChooser>();
        var rollSequence = mockChooser.SetupSequence(c => c.RollDice(100));
        rollSequence.Returns(50);  // Floyd check fails
        rollSequence.Returns(30);  // Normal dream check succeeds
        mockChooser.Setup(c => c.RollDice(5)).Returns(5); // Pick fifth dream

        var dream = Dreams.GetDream(pfContext, mockChooser.Object);

        dream.Should().NotBeNull();
        dream.Should().Contain("Nebulon");
        dream.Should().Contain("spider");
    }

    [Test]
    public void Dreams_GetDream_CanReturnNull_WhenNormalDreamRollFails()
    {
        var target = GetTarget();
        var pfContext = target.Context;

        // Floyd is not on, so only normal dream check happens
        var floyd = GetItem<Floyd>();
        floyd.HasEverBeenOn = false;

        var mockChooser = new Mock<IRandomChooser>();
        mockChooser.Setup(c => c.RollDice(100)).Returns(80); // > 60, no dream

        var dream = Dreams.GetDream(pfContext, mockChooser.Object);

        dream.Should().BeNull();
    }

    [Test]
    public void Dreams_GetDream_CanReturnNull_WhenFloydOnButBothRollsFail()
    {
        var target = GetTarget();
        var pfContext = target.Context;

        var floyd = GetItem<Floyd>();
        floyd.HasEverBeenOn = true;

        var mockChooser = new Mock<IRandomChooser>();
        var rollSequence = mockChooser.SetupSequence(c => c.RollDice(100));
        rollSequence.Returns(50);  // Floyd check fails (> 13)
        rollSequence.Returns(80);  // Normal dream check fails (> 60)

        var dream = Dreams.GetDream(pfContext, mockChooser.Object);

        dream.Should().BeNull();
    }

    [Test]
    public void Dreams_IntegrationWithSleep_AppearsInSleepMessage()
    {
        var target = GetTarget();
        var pfContext = target.Context;
        StartHere<DormA>();

        pfContext.Tired = TiredLevel.Tired;
        pfContext.Day = 1;

        // Set up mock chooser for deterministic dream
        var mockChooser = new Mock<IRandomChooser>();
        var rollSequence = mockChooser.SetupSequence(c => c.RollDice(100));
        rollSequence.Returns(50);  // Floyd check fails
        rollSequence.Returns(30);  // Normal dream check succeeds
        mockChooser.Setup(c => c.RollDice(5)).Returns(1); // Feinstein dream
        SleepEngine.Chooser = mockChooser.Object;

        // Get in bed and trigger forced sleep
        pfContext.Tired = TiredLevel.AboutToDrop;
        pfContext.SleepNotifications.NextWarningAt = pfContext.CurrentTime;

        var result = SleepEngine.ProcessForcedSleep(pfContext);

        result.Should().Contain("Feinstein");

        // Verify dream is between sleep and wake messages
        var sleepIndex = result.IndexOf("fall asleep", StringComparison.Ordinal);
        var dreamIndex = result.IndexOf("Feinstein", StringComparison.Ordinal);
        var wakeIndex = result.IndexOf("SEPTEM", StringComparison.Ordinal);

        dreamIndex.Should().BeGreaterThan(sleepIndex);
        dreamIndex.Should().BeLessThan(wakeIndex);

        // Reset static chooser
        SleepEngine.Chooser = new GameEngine.RandomChooser();
    }
}
