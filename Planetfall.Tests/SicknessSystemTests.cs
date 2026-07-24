using FluentAssertions;
using GameEngine;
using Model.Interface;
using Moq;
using Planetfall.Item.Feinstein;
using Planetfall.Item.Lawanda;
using Planetfall.Location.Kalamontee;
using Utilities;

namespace Planetfall.Tests;

/// <summary>
/// Tests for the Planetfall disease mechanic (issue #116). The disease is a death clock that
/// advances one level per day, steadily eats carrying capacity (10 per level, mirroring the
/// original's LOAD-ALLOWED), kills the player at level 9, and can be partially rolled back by
/// drinking the experimental medicine vial.
///
/// ZIL ground truth (read-only reference): globals.zil:2330-2369 (progression + death),
/// comptwo.zil:170-185 (the medicine effect).
/// </summary>
public class SicknessSystemTests : EngineTestsBase
{
    [SetUp]
    public void DisableRandomDreams()
    {
        // Wake-up triggers a random dream; force it off so the wake/death assertions are deterministic.
        var chooser = new Mock<IRandomChooser>();
        chooser.Setup(c => c.RollDice(It.IsAny<int>())).Returns(100);
        chooser.Setup(c => c.RollDiceSuccess(It.IsAny<int>())).Returns(false);
        SleepEngine.Chooser = chooser.Object;
    }

    [TearDown]
    public void ResetChooser()
    {
        SleepEngine.Chooser = new RandomChooser();
    }

    [Test]
    public void SicknessCounter_StartsAtOne()
    {
        var context = GetTarget().Context;
        context.SicknessCounter.Should().Be(1);
    }

    [Test]
    public void SicknessDescription_DerivesFromCounter_NotDay()
    {
        var context = GetTarget().Context;

        // Bumping the day must NOT change the health description - only the mutable counter does.
        context.Day = 7;
        context.SicknessCounter = 3;
        context.SicknessDescription.Should().Be("You feel a bit sick and feverish. ");

        context.SicknessCounter = 1;
        context.SicknessDescription.Should().Be("You are in perfect health. ");
    }

    [Test]
    public void EachSickDay_RaisesLevel_AndLowersCarryLimitByTen()
    {
        var target = GetTarget();
        var context = target.Context;
        StartHere<BedLocation>();

        context.SicknessCounter = 3;
        var limitBefore = context.EffectiveLoadAllowed;

        context.SleepNotifications.QueueFallAsleep(context.CurrentTime);
        var result = SleepEngine.ProcessFallAsleep(context);

        context.SicknessCounter.Should().Be(4);
        context.EffectiveLoadAllowed.Should().Be(limitBefore - 10);
        // Symptom/health description reflects the new level.
        context.SicknessDescription.Should().Be("You feel a bit sick and feverish. ");
        result.Should().NotBeNull();
    }

    [Test]
    public void CarryLimit_DropsTenPerSicknessLevel()
    {
        var context = GetTarget().Context;

        context.SicknessCounter = 1;
        var fullLimit = context.EffectiveLoadAllowed;

        context.SicknessCounter = 2;
        context.EffectiveLoadAllowed.Should().Be(fullLimit - 10);

        context.SicknessCounter = 5;
        context.EffectiveLoadAllowed.Should().Be(fullLimit - 40);
    }

    [Test]
    public void HaveRoomForItem_RejectsWhenOverEffectiveLimit()
    {
        var context = GetTarget().Context;
        var brush = GetItem<Brush>();

        // Healthy: there is plenty of room.
        context.SicknessCounter = 1;
        var fullLimit = context.EffectiveLoadAllowed;
        context.HaveRoomForItem(brush).Should().BeTrue();

        // Crank the disease high enough to drive the effective load limit to zero; now there is no room.
        context.SicknessCounter = 1 + fullLimit / 10 + 1;
        context.EffectiveLoadAllowed.Should().BeLessThanOrEqualTo(0);
        context.HaveRoomForItem(brush).Should().BeFalse();
    }

    [Test]
    public void ReachingLevelNine_KillsPlayerWithIllnessText()
    {
        var target = GetTarget();
        var context = target.Context;
        StartHere<BedLocation>();

        context.Day = 8;
        context.SicknessCounter = 8;
        context.SleepNotifications.QueueFallAsleep(context.CurrentTime);

        var result = SleepEngine.ProcessFallAsleep(context);

        context.SicknessCounter.Should().Be(9);
        result.Should().Contain("You finally succumb to the ravages of your illness and collapse.");
        result.Should().Contain("You have died");
    }

    [Test]
    public void Untreated_StillDiesOnSchedule()
    {
        // Regression of current behavior: with no treatment the disease tracks the day and the
        // player dies on the ninth day's wake-up.
        var target = GetTarget();
        var context = target.Context;
        StartHere<BedLocation>();

        // Default counter equals the starting day (1); walk both forward in lock-step.
        for (var day = 1; day <= 7; day++)
        {
            context.SleepNotifications.QueueFallAsleep(context.CurrentTime);
            var midResult = SleepEngine.ProcessFallAsleep(context);
            midResult.Should().NotContain("You have died");
        }

        // Eighth sleep takes the counter to 9 -> death.
        context.SleepNotifications.QueueFallAsleep(context.CurrentTime);
        var result = SleepEngine.ProcessFallAsleep(context);
        result.Should().Contain("You have died");
    }

    [Test]
    public void DrinkingMedicine_LowersSicknessByTwo_AndRestoresTwentyCapacity()
    {
        var context = GetTarget().Context;
        var medicine = GetItem<Medicine>();

        context.SicknessCounter = 5;
        var limitBefore = context.EffectiveLoadAllowed;

        var (message, wasConsumed) = medicine.OnDrinking(context);

        context.SicknessCounter.Should().Be(3);
        context.EffectiveLoadAllowed.Should().Be(limitBefore + 20);
        message.Should().Contain("bitter");
        wasConsumed.Should().BeTrue(); // one-shot: the engine destroys the medicine after drinking
    }

    [Test]
    public void DrinkingMedicine_PushesDeathBackTwoDays()
    {
        var target = GetTarget();
        var context = target.Context;
        StartHere<BedLocation>();

        context.Day = 8;
        context.SicknessCounter = 8;

        // Treat the disease: level 8 -> 6, so it now takes three more sick days to reach 9.
        GetItem<Medicine>().OnDrinking(context);
        context.SicknessCounter.Should().Be(6);

        // The very next wake-up no longer kills - the clock was rolled back.
        context.SleepNotifications.QueueFallAsleep(context.CurrentTime);
        var result = SleepEngine.ProcessFallAsleep(context);

        context.SicknessCounter.Should().Be(7);
        result.Should().NotContain("You have died");
    }

    [Test]
    public void DrinkingMedicine_NeverDropsBelowLevelOne()
    {
        var context = GetTarget().Context;

        context.SicknessCounter = 1;
        GetItem<Medicine>().OnDrinking(context);

        context.SicknessCounter.Should().Be(1);
    }

    [Test]
    public async Task DrinkingMedicineThroughEngine_IsOneShot_AndEmptiesTheBottle()
    {
        var target = GetTarget();
        var context = target.Context;

        context.SicknessCounter = 5;

        // Hold the open bottle (with its medicine inside) so the drink command can reach it.
        var bottle = GetItem<MedicineBottle>();
        bottle.IsOpen = true;
        context.ItemPlacedHere(bottle);
        bottle.Items.Should().ContainSingle();

        var first = await target.GetResponse("drink medicine");
        first.Should().Contain("bitter");
        context.SicknessCounter.Should().Be(3);

        // One-shot: the medicine is gone and the bottle is now empty.
        bottle.Items.Should().BeEmpty();

        // A second drink does nothing to the disease.
        await target.GetResponse("drink medicine");
        context.SicknessCounter.Should().Be(3);
    }

    [Test]
    public async Task PutBrushInMedicineBottle_TypeRefusal_NamesTheItem_AndIsNotBlank()
    {
        var target = GetTarget();
        var context = target.Context;
        StartHere<MessCorridor>();

        // Hold the open bottle (with its medicine inside) plus a brush to try to stuff into it.
        var bottle = GetItem<MedicineBottle>();
        bottle.IsOpen = true;
        context.ItemPlacedHere(bottle);
        context.ItemPlacedHere(GetItem<Brush>());

        var response = await target.GetResponse("put brush in bottle");

        // Issue #422: the bottle only holds medicine, so a brush is refused on type. That refusal
        // must be a deterministic message that names the item - not a fall-through to the AI
        // narrator, which (with a stubbed generation client) leaves a blank line.
        response.Should().NotBeNullOrWhiteSpace();
        response.Should().Contain("brush");
        bottle.Items.Should().NotContain(GetItem<Brush>());
    }
}
