using FluentAssertions;
using Model.Interface;
using Moq;
using Planetfall.Item.Computer;
using Planetfall.Item.Kalamontee.Mech;
using Planetfall.Item.Lawanda.Lab;
using Planetfall.Location.Computer;
using Planetfall.Location.Lawanda;

namespace Planetfall.Tests;

public class RelayTests : EngineTestsBase
{
    private Mock<IRandomChooser> _mockRandomChooser = null!;

    private void SetupMockRandomChooser(bool shouldHit, string missMessage = "A near miss!")
    {
        _mockRandomChooser = new Mock<IRandomChooser>();
        // RollDice(100) returns 1-100. Hit if roll <= hitChance (20 + MarksmanshipCounter)
        // To hit: return low value (1). To miss: return high value (100)
        _mockRandomChooser.Setup(r => r.RollDice(100)).Returns(shouldHit ? 1 : 100);
        _mockRandomChooser.Setup(r => r.Choose(It.IsAny<List<string>>())).Returns(missMessage);
        GetItem<Laser>().Chooser = _mockRandomChooser.Object;
    }

    [Test]
    public async Task ExamineRelay_ShouldReturnDescription()
    {
        var target = GetTarget();
        StartHere<StripNearRelay>();

        var response = await target.GetResponse("examine relay");

        response.Should().Contain("vacuum-sealed microrelay");
        response.Should().Contain("red translucent plastic");
        response.Should().Contain("speck");
        response.Should().Contain("blue boulder");
    }

    [Test]
    public void StripNearRelay_RelayShouldBePresent()
    {
        GetTarget();
        var location = StartHere<StripNearRelay>();
        var relay = GetItem<Relay>();

        relay.CurrentLocation.Should().Be(location);
        location.Items.Should().Contain(relay);
    }

    [Test]
    public async Task ExamineMicrorelay_ShouldReturnDescription()
    {
        var target = GetTarget();
        StartHere<StripNearRelay>();

        var response = await target.GetResponse("examine microrelay");

        response.Should().Contain("vacuum-sealed microrelay");
    }

    [Test]
    public async Task LookAtRelay_ShouldReturnDescription()
    {
        var target = GetTarget();
        StartHere<StripNearRelay>();

        var response = await target.GetResponse("look at relay");

        response.Should().Contain("vacuum-sealed microrelay");
        response.Should().Contain("blue boulder");
    }

    [Test]
    public async Task LookIntoRelay_ShouldReturnDescription()
    {
        var target = GetTarget();
        StartHere<StripNearRelay>();

        var response = await target.GetResponse("look into relay");

        response.Should().Contain("vacuum-sealed microrelay");
        response.Should().Contain("blue boulder");
    }

    [Test]
    public async Task LookAtMicrorelay_ShouldReturnDescription()
    {
        var target = GetTarget();
        StartHere<StripNearRelay>();

        var response = await target.GetResponse("look at microrelay");

        response.Should().Contain("vacuum-sealed microrelay");
    }

    [Test]
    public async Task LookIntoMicrorelay_ShouldReturnDescription()
    {
        var target = GetTarget();
        StartHere<StripNearRelay>();

        var response = await target.GetResponse("look into micro-relay");

        response.Should().Contain("vacuum-sealed microrelay");
    }

    [Test]
    public async Task Type384_TeleportsToStation384_WithFullDescription()
    {
        var target = GetTarget();
        StartHere<MiniaturizationBooth>();
        Take<MiniaturizationAccessCard>();

        await target.GetResponse("slide miniaturization card through slot");
        var response = await target.GetResponse("type 384");

        // Teleportation message
        response.Should().Contain("walls of the booth sliding away in all directions");
        response.Should().Contain("momentary queasiness in the pit of your stomach");

        // Station 384 room name and description
        response.Should().Contain("Station 384");
        response.Should().Contain("standing on a square plate of heavy metal");
        response.Should().Contain("identical metal plate");
        response.Should().Contain("To the east is a wide, metallic strip");
    }

    #region Shooting Relay Tests

    [Test]
    public async Task ShootRelay_WithNonRedLaser_DestroysRelay()
    {
        var target = GetTarget();
        StartHere<StripNearRelay>();
        Take<Laser>();
        GetItem<OldBattery>().ChargesRemaining = 10;
        GetItem<Laser>().Setting = 2; // Orange

        var response = await target.GetResponse("shoot relay with laser");

        response.Should().Contain("slices through the red plastic");
        response.Should().Contain("collapses into a heap of plastic shards");
        GetItem<Relay>().RelayDestroyed.Should().BeTrue();
    }

    [Test]
    [TestCase(2, "orange")]
    [TestCase(3, "yellow")]
    [TestCase(4, "green")]
    [TestCase(5, "blue")]
    [TestCase(6, "violet")]
    public async Task ShootRelay_WithNonRedLaser_AllNonRedColorsDestroyRelay(int setting, string color)
    {
        var target = GetTarget();
        StartHere<StripNearRelay>();
        Take<Laser>();
        GetItem<OldBattery>().ChargesRemaining = 10;
        GetItem<Laser>().Setting = setting;

        var response = await target.GetResponse("shoot relay with laser");

        response.Should().Contain(color);
        response.Should().Contain("slices through the red plastic");
        GetItem<Relay>().RelayDestroyed.Should().BeTrue();
    }

    [Test]
    public async Task ShootRelay_WithRedLaser_FirstHit_SetsSpeckHit()
    {
        var target = GetTarget();
        StartHere<StripNearRelay>();
        Take<Laser>();
        GetItem<OldBattery>().ChargesRemaining = 10;
        GetItem<Laser>().Setting = 1; // Red
        SetupMockRandomChooser(shouldHit: true);
        var relay = GetItem<Relay>();

        var response = await target.GetResponse("shoot relay with laser");

        response.Should().Contain("The speck is hit by the beam!");
        response.Should().Contain("sizzles a little, but isn't destroyed yet");
        relay.SpeckHit.Should().BeTrue();
        relay.SpeckDestroyed.Should().BeFalse();
    }

    [Test]
    public async Task ShootRelay_WithRedLaser_SecondHit_DestroysSpeck()
    {
        var target = GetTarget();
        StartHere<StripNearRelay>();
        Take<Laser>();
        GetItem<OldBattery>().ChargesRemaining = 10;
        GetItem<Laser>().Setting = 1; // Red
        SetupMockRandomChooser(shouldHit: true);
        var relay = GetItem<Relay>();
        relay.SpeckHit = true; // Already hit once

        var response = await target.GetResponse("shoot relay with laser");

        response.Should().Contain("The beam hits the speck again!");
        response.Should().Contain("vaporizes into a fine cloud of ash");
        response.Should().Contain("Sector 384 will activate in 200 millichrons");
        relay.SpeckDestroyed.Should().BeTrue();
    }

    [Test]
    public async Task ShootRelay_WithRedLaser_SecondHit_AwardsPoints()
    {
        var target = GetTarget();
        StartHere<StripNearRelay>();
        Take<Laser>();
        GetItem<OldBattery>().ChargesRemaining = 10;
        GetItem<Laser>().Setting = 1; // Red
        GetItem<Laser>().HasBeenFired = true; // Already fired before, no 2-point bonus
        SetupMockRandomChooser(shouldHit: true);
        var relay = GetItem<Relay>();
        relay.SpeckHit = true; // Already hit once
        var initialScore = target.Context.Score;

        await target.GetResponse("shoot relay with laser");

        // Exactly 8 points for destroying speck (no laser fire bonus since already fired)
        (target.Context.Score - initialScore).Should().Be(8);
    }

    [Test]
    public async Task ShootRelay_WithRedLaser_SecondHit_StartsTimer()
    {
        var target = GetTarget();
        StartHere<StripNearRelay>();
        Take<Laser>();
        GetItem<OldBattery>().ChargesRemaining = 10;
        GetItem<Laser>().Setting = 1; // Red
        SetupMockRandomChooser(shouldHit: true);
        var relay = GetItem<Relay>();
        relay.SpeckHit = true; // Already hit once

        await target.GetResponse("shoot relay with laser");

        var timer = GetItem<SectorActivationTimer>();
        target.Context.Actors.Should().Contain(timer);
    }

    [Test]
    public async Task ShootRelay_WithRedLaser_Miss_IncreasesMarksmanship()
    {
        var target = GetTarget();
        StartHere<StripNearRelay>();
        Take<Laser>();
        GetItem<OldBattery>().ChargesRemaining = 10;
        GetItem<Laser>().Setting = 1; // Red
        SetupMockRandomChooser(shouldHit: false);
        var relay = GetItem<Relay>();

        await target.GetResponse("shoot relay with laser");

        relay.SpeckHit.Should().BeFalse();
        relay.MarksmanshipCounter.Should().Be(12);
    }

    [Test]
    public async Task ShootRelay_WithRedLaser_Miss_ShowsMissMessage()
    {
        var target = GetTarget();
        StartHere<StripNearRelay>();
        Take<Laser>();
        GetItem<OldBattery>().ChargesRemaining = 10;
        GetItem<Laser>().Setting = 1; // Red
        SetupMockRandomChooser(shouldHit: false, missMessage: "The beam just misses the speck!");

        var response = await target.GetResponse("shoot relay with laser");

        response.Should().Contain("just misses the speck");
    }

    [Test]
    public async Task ShootRelay_AlreadyDestroyed_JustEmitsBeam()
    {
        var target = GetTarget();
        StartHere<StripNearRelay>();
        Take<Laser>();
        GetItem<OldBattery>().ChargesRemaining = 10;
        var relay = GetItem<Relay>();
        relay.RelayDestroyed = true;

        var response = await target.GetResponse("shoot relay with laser");

        response.Should().Contain("beam of light");
        response.Should().NotContain("slices through");
        response.Should().NotContain("speck");
    }

    [Test]
    public async Task ShootRelay_SpeckAlreadyDestroyed_JustEmitsBeam()
    {
        var target = GetTarget();
        StartHere<StripNearRelay>();
        Take<Laser>();
        GetItem<OldBattery>().ChargesRemaining = 10;
        var relay = GetItem<Relay>();
        relay.SpeckDestroyed = true;

        var response = await target.GetResponse("shoot relay with laser");

        response.Should().Contain("beam of light");
        response.Should().NotContain("vaporizes");
        response.Should().NotContain("sizzles");
    }

    [Test]
    public async Task ExamineRelay_AfterSpeckDestroyed_ShowsDescription()
    {
        var target = GetTarget();
        StartHere<StripNearRelay>();
        var relay = GetItem<Relay>();
        relay.SpeckDestroyed = true;

        var response = await target.GetResponse("examine relay");

        response.Should().Contain("vacuum-sealed microrelay");
        response.Should().NotContain("blue boulder");
    }

    [Test]
    public async Task ShootRelay_WithNonRedLaser_AfterSpeckDestroyed_StillDestroysRelay()
    {
        var target = GetTarget();
        StartHere<StripNearRelay>();
        Take<Laser>();
        GetItem<OldBattery>().ChargesRemaining = 10;
        GetItem<Laser>().Setting = 2; // Orange (non-red)
        var relay = GetItem<Relay>();
        relay.SpeckDestroyed = true; // Speck already gone

        var response = await target.GetResponse("shoot relay with laser");

        response.Should().Contain("slices through the red plastic");
        response.Should().Contain("collapses into a heap of plastic shards");
        relay.RelayDestroyed.Should().BeTrue();
    }

    [Test]
    public async Task ShootRelay_WithRedLaser_AfterSpeckDestroyed_GivesGenericResponse()
    {
        var target = GetTarget();
        StartHere<StripNearRelay>();
        Take<Laser>();
        GetItem<OldBattery>().ChargesRemaining = 10;
        GetItem<Laser>().Setting = 1; // Red
        var relay = GetItem<Relay>();
        relay.SpeckDestroyed = true; // Speck already gone

        var response = await target.GetResponse("shoot relay with laser");

        response.Should().Contain("strikes the relay");
        response.Should().Contain("grows a bit warm");
        relay.RelayDestroyed.Should().BeFalse();
    }

    [Test]
    public async Task ShootRelay_AfterRelayDestroyed_GivesGenericResponse()
    {
        var target = GetTarget();
        StartHere<StripNearRelay>();
        Take<Laser>();
        GetItem<OldBattery>().ChargesRemaining = 10;
        var relay = GetItem<Relay>();
        relay.RelayDestroyed = true;

        var response = await target.GetResponse("shoot relay with laser");

        response.Should().Contain("strikes the relay");
        response.Should().Contain("grows a bit warm");
    }

    [Test]
    public async Task ExamineRelay_AfterRelayDestroyed_ShowsDestroyed()
    {
        var target = GetTarget();
        StartHere<StripNearRelay>();
        var relay = GetItem<Relay>();
        relay.RelayDestroyed = true;

        var response = await target.GetResponse("examine relay");

        response.Should().Contain("heap of melted plastic shards");
    }

    [Test]
    public async Task Look_AfterRelayDestroyed_ShowsShatteredRemains()
    {
        var target = GetTarget();
        StartHere<StripNearRelay>();
        var relay = GetItem<Relay>();
        relay.RelayDestroyed = true;

        var response = await target.GetResponse("look");

        response.Should().Contain("shattered remains of the microrelay");
    }

    [Test]
    public async Task GoEast_AfterRelayDestroyed_ShowsSliceMessage()
    {
        var target = GetTarget();
        StartHere<StripNearRelay>();
        var relay = GetItem<Relay>();
        relay.RelayDestroyed = true;

        var response = await target.GetResponse("east");

        response.Should().Contain("slice yourself to ribbons on the shattered relay");
    }

    [Test]
    public async Task GoEast_RelayNotDestroyed_ShowsSealedMessage()
    {
        var target = GetTarget();
        StartHere<StripNearRelay>();

        var response = await target.GetResponse("east");

        response.Should().Contain("relay is sealed");
        response.Should().Contain("look into it");
    }

    #endregion

    #region Timer Tests

    [Test]
    public async Task SectorActivationTimer_CountsDown()
    {
        var target = GetTarget();
        StartHere<StripNearRelay>();
        var timer = GetItem<SectorActivationTimer>();
        timer.TurnsRemaining = 5;
        target.Context.RegisterActor(timer);

        await target.GetResponse("wait");

        timer.TurnsRemaining.Should().Be(4);
    }

    [Test]
    public async Task SectorActivationTimer_AtZero_PlayerOnStrip_Dies()
    {
        var target = GetTarget();
        StartHere<StripNearRelay>();
        var timer = GetItem<SectorActivationTimer>();
        timer.TurnsRemaining = 1;
        target.Context.RegisterActor(timer);

        var response = await target.GetResponse("wait");

        response.Should().Contain("computer comes back to life");
        response.Should().Contain("surge of electric current");
        response.Should().Contain("still standing on one of its circuits");
    }

    [Test]
    public async Task SectorActivationTimer_AtZero_PlayerOffStrip_Survives()
    {
        var target = GetTarget();
        StartHere<MiniaturizationBooth>(); // Not on the silicon strip
        var timer = GetItem<SectorActivationTimer>();
        timer.TurnsRemaining = 1;
        target.Context.RegisterActor(timer);

        var response = await target.GetResponse("wait");

        response.Should().NotContain("electric current");
        response.Should().NotContain("circuits");
    }

    [Test]
    public async Task SectorActivationTimer_RemovesItselfAfterExpiring()
    {
        var target = GetTarget();
        StartHere<MiniaturizationBooth>(); // Not on the silicon strip
        var timer = GetItem<SectorActivationTimer>();
        timer.TurnsRemaining = 1;
        target.Context.RegisterActor(timer);

        await target.GetResponse("wait");

        target.Context.Actors.Should().NotContain(timer);
    }

    [Test]
    public async Task SectorActivationTimer_AllSiliconStripLocations_AreDangerous()
    {
        // Test Station384
        var target = GetTarget();
        StartHere<Station384>();
        var timer = GetItem<SectorActivationTimer>();
        timer.TurnsRemaining = 1;
        target.Context.RegisterActor(timer);

        var response = await target.GetResponse("wait");
        response.Should().Contain("electric current");
    }

    [Test]
    public async Task SectorActivationTimer_StripNearStation_IsDangerous()
    {
        var target = GetTarget();
        StartHere<StripNearStation>();
        var timer = GetItem<SectorActivationTimer>();
        timer.TurnsRemaining = 1;
        target.Context.RegisterActor(timer);

        var response = await target.GetResponse("wait");
        response.Should().Contain("electric current");
    }

    [Test]
    public async Task SectorActivationTimer_MiddleOfStrip_IsDangerous()
    {
        var target = GetTarget();
        StartHere<MiddleOfStrip>();
        var timer = GetItem<SectorActivationTimer>();
        timer.TurnsRemaining = 1;
        target.Context.RegisterActor(timer);

        var response = await target.GetResponse("wait");
        response.Should().Contain("electric current");
    }

    #endregion
}
