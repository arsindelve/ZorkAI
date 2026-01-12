using FluentAssertions;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Planetfall.Location.Kalamontee.Dorm;

namespace Planetfall.Tests;

/// <summary>
/// Tests for the Dreams system.
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

        // Try many times to ensure Floyd dream doesn't appear
        var foundFloydDream = false;
        for (int i = 0; i < 100; i++)
        {
            var dream = Dreams.GetDream(pfContext, new Random(i));
            if (dream != null && dream.Contains("Floyd"))
            {
                foundFloydDream = true;
                break;
            }
        }

        foundFloydDream.Should().BeFalse();
    }

    [Test]
    public void Dreams_GetDream_WithFloydOn_CanReturnFloydDream()
    {
        var target = GetTarget();
        var pfContext = target.Context;

        var floyd = GetItem<Floyd>();
        floyd.HasEverBeenOn = true;

        // Try many times to find Floyd dream (13% chance)
        var foundFloydDream = false;
        for (int i = 0; i < 200; i++)
        {
            var dream = Dreams.GetDream(pfContext, new Random(i));
            if (dream != null && dream.Contains("Floyd") && dream.Contains("office"))
            {
                foundFloydDream = true;
                break;
            }
        }

        foundFloydDream.Should().BeTrue();
    }

    [Test]
    public void Dreams_GetDream_FloydDream_HasCorrectContent()
    {
        var target = GetTarget();
        var pfContext = target.Context;

        var floyd = GetItem<Floyd>();
        floyd.HasEverBeenOn = true;

        // Try to find Floyd dream
        string? floydDream = null;
        for (int i = 0; i < 200; i++)
        {
            var dream = Dreams.GetDream(pfContext, new Random(i));
            if (dream != null && dream.Contains("Floyd") && dream.Contains("office"))
            {
                floydDream = dream;
                break;
            }
        }

        floydDream.Should().NotBeNull();
        floydDream.Should().Contain("busy office");
        floydDream.Should().Contain("carrying papers");
        floydDream.Should().Contain("delivering coffee");
        floydDream.Should().Contain("tell him a story");
        floydDream.Should().Contain("trusting eyes");
    }

    [Test]
    public void Dreams_GetDream_CanReturnNormalDreams()
    {
        var target = GetTarget();
        var pfContext = target.Context;

        // Try many times to find at least one normal dream
        var foundDream = false;
        for (int i = 0; i < 100; i++)
        {
            var dream = Dreams.GetDream(pfContext, new Random(i));
            if (dream != null && !dream.Contains("office")) // Not Floyd dream
            {
                foundDream = true;
                break;
            }
        }

        foundDream.Should().BeTrue();
    }

    [Test]
    public void Dreams_GetDream_CanReturnNull()
    {
        var target = GetTarget();
        var pfContext = target.Context;

        // 27% chance of no dream, should find null in reasonable attempts
        var foundNull = false;
        for (int i = 0; i < 50; i++)
        {
            var dream = Dreams.GetDream(pfContext, new Random(i));
            if (dream == null)
            {
                foundNull = true;
                break;
            }
        }

        foundNull.Should().BeTrue();
    }

    [Test]
    public void Dreams_NormalDreams_ContainExpectedThemes()
    {
        var target = GetTarget();
        var pfContext = target.Context;

        var dreams = new HashSet<string>();

        // Collect a variety of dreams
        for (int i = 0; i < 500; i++)
        {
            var dream = Dreams.GetDream(pfContext, new Random(i));
            if (dream != null && !dream.Contains("office"))
            {
                dreams.Add(dream);
            }
        }

        // Should have found multiple different dreams
        dreams.Should().HaveCountGreaterThan(3);

        var allDreamsText = string.Join(" ", dreams);

        // Check for known dream themes
        var hasFeinstein = allDreamsText.Contains("Feinstein");
        var hasRamos = allDreamsText.Contains("Ramos");
        var hasGallium = allDreamsText.Contains("Gallium");
        var hasWaterfall = allDreamsText.Contains("waterfall");
        var hasNebulon = allDreamsText.Contains("Nebulon");

        // Should have found at least 3 of the 5 different dreams
        var themeCount = new[] { hasFeinstein, hasRamos, hasGallium, hasWaterfall, hasNebulon }
            .Count(x => x);
        themeCount.Should().BeGreaterThanOrEqualTo(3);
    }

    [Test]
    public void Dreams_Feinstein_ContainsExpectedContent()
    {
        var target = GetTarget();
        var pfContext = target.Context;

        string? feinsteinDream = null;
        for (int i = 0; i < 500; i++)
        {
            var dream = Dreams.GetDream(pfContext, new Random(i));
            if (dream != null && dream.Contains("Feinstein") && dream.Contains("bridge"))
            {
                feinsteinDream = dream;
                break;
            }
        }

        feinsteinDream.Should().NotBeNull();
        feinsteinDream.Should().Contain("Blather");
        feinsteinDream.Should().Contain("scrub");
        feinsteinDream.Should().Contain("self-destruct");
    }

    [Test]
    public void Dreams_IntegrationWithSleep_AppearsInSleepMessage()
    {
        var target = GetTarget();
        var pfContext = target.Context;
        StartHere<DormA>();

        pfContext.Tired = TiredLevel.Tired;
        pfContext.Day = 1;

        // Get in bed and trigger forced sleep
        pfContext.Tired = TiredLevel.AboutToDrop;
        pfContext.SleepNotifications.NextWarningAt = pfContext.CurrentTime;

        var result = SleepEngine.ProcessForcedSleep(pfContext);

        // Should either contain a dream or not (27% chance of no dream)
        // But if there's a dream, it should be formatted correctly
        if (result.Contains("Feinstein") || result.Contains("Floyd") ||
            result.Contains("Ramos") || result.Contains("Gallium") ||
            result.Contains("Nebulon") || result.Contains("waterfall"))
        {
            // Dream appeared - verify it's between sleep and wake messages
            var sleepIndex = result.IndexOf("fall asleep");
            var wakeIndex = result.IndexOf("SEPTEM");
            var dreamIndex = Math.Max(
                Math.Max(result.IndexOf("Feinstein"), result.IndexOf("Floyd")),
                Math.Max(
                    Math.Max(result.IndexOf("Ramos"), result.IndexOf("Gallium")),
                    Math.Max(result.IndexOf("Nebulon"), result.IndexOf("waterfall"))
                )
            );

            if (dreamIndex > 0)
            {
                dreamIndex.Should().BeGreaterThan(sleepIndex);
                dreamIndex.Should().BeLessThan(wakeIndex);
            }
        }
    }
}
