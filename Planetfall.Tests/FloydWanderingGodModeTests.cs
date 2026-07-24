using FluentAssertions;
using Model.Interface;
using Moq;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Planetfall.Location.Kalamontee.Mech;

namespace Planetfall.Tests;

/// <summary>
/// God-mode toggle to disable Floyd's random wandering, a deterministic playtesting affordance
/// (the same effect the walkthrough tests get by mocking IRandomChooser). Covers the command wiring,
/// that the two random wandering triggers are suppressed, that scripted wandering is unaffected, and
/// persistence through save/restore.
/// </summary>
public class FloydWanderingGodModeTests : EngineTestsBase
{
    #region Command wiring

    [Test]
    public async Task GodMode_NoWander_DisablesWandering()
    {
        var engine = GetTarget();
        StartHere<RobotShop>();

        var response = await engine.GetResponse("god mode no wander");

        engine.Context.FloydWanderingDisabled.Should().BeTrue();
        response.Should().Contain("wandering disabled");
    }

    [Test]
    public async Task GodMode_Wander_ReEnablesWandering()
    {
        var engine = GetTarget();
        StartHere<RobotShop>();
        engine.Context.FloydWanderingDisabled = true;

        var response = await engine.GetResponse("god mode wander");

        engine.Context.FloydWanderingDisabled.Should().BeFalse();
        response.Should().Contain("wandering enabled");
    }

    [Test]
    public async Task GodMode_NoWandering_DisablesWandering()
    {
        var engine = GetTarget();
        StartHere<RobotShop>();

        var response = await engine.GetResponse("god mode no wandering");

        engine.Context.FloydWanderingDisabled.Should().BeTrue();
        response.Should().Contain("wandering disabled");
    }

    #endregion

    #region Wandering guards

    [Test]
    public async Task WanderingDisabled_SuppressesSpontaneousWandering_EvenWhenDiceWouldTrigger()
    {
        var target = GetTarget();
        var robotShop = StartHere<RobotShop>();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        floyd.TurnOnCountdown = 0;
        floyd.CurrentLocation = robotShop;
        robotShop.ItemPlacedHere(floyd);
        target.Context.RegisterActor(floyd);
        target.Context.FloydWanderingDisabled = true;

        var mockChooser = new Mock<IRandomChooser>();
        mockChooser.Setup(r => r.RollDiceSuccess(20)).Returns(true); // Would normally trigger wandering
        floyd.Chooser = mockChooser.Object;

        await target.GetResponse("wait");

        floyd.IsOffWandering.Should().BeFalse();
        floyd.CurrentLocation.Should().Be(robotShop);
    }

    [Test]
    public async Task WanderingDisabled_SuppressesStopFollowing_EvenWhenDiceWouldTrigger()
    {
        var target = GetTarget();
        var robotShop = StartHere<RobotShop>();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        floyd.TurnOnCountdown = 0;
        floyd.CurrentLocation = robotShop;
        robotShop.ItemPlacedHere(floyd);
        target.Context.RegisterActor(floyd);
        target.Context.FloydWanderingDisabled = true;

        var mockChooser = new Mock<IRandomChooser>();
        mockChooser.Setup(r => r.RollDiceSuccess(5)).Returns(true); // Would normally stop him following
        floyd.Chooser = mockChooser.Object;

        var response = await target.GetResponse("w");

        response.Should().Contain("Floyd follows you");
        floyd.IsOffWandering.Should().BeFalse();
        floyd.CurrentLocation.Should().Be(GetLocation<MachineShop>());
    }

    [Test]
    public async Task WanderingDisabled_DoesNotBlockScriptedWandering()
    {
        // Control: FloydWanderingDisabled only suppresses the two random triggers, not a deliberate
        // StartWandering() call (e.g. Floyd storming off when upset).
        var target = GetTarget();
        var robotShop = StartHere<RobotShop>();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        floyd.CurrentLocation = robotShop;
        target.Context.FloydWanderingDisabled = true;

        floyd.StartWandering(target.Context);

        floyd.IsOffWandering.Should().BeTrue();
    }

    #endregion

    #region Persistence

    [Test]
    public void FloydWanderingDisabled_SurvivesSaveAndRestore()
    {
        var engine = GetTarget();
        StartHere<RobotShop>();
        engine.Context.FloydWanderingDisabled = true;

        var saved = engine.SaveGame();

        var freshEngine = GetTarget();
        var restored = (PlanetfallContext)freshEngine.RestoreGame(saved);

        restored.FloydWanderingDisabled.Should().BeTrue();
    }

    #endregion
}
