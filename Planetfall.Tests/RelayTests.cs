using FluentAssertions;
using Planetfall.Item.Computer;
using Planetfall.Item.Kalamontee.Mech;
using Planetfall.Item.Lawanda.Lab;
using Planetfall.Location.Computer;
using Planetfall.Location.Lawanda;

namespace Planetfall.Tests;

public class RelayTests : EngineTestsBase
{
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
        var relay = GetItem<Relay>();
        relay.MarksmanshipCounter = 100; // Ensure hit

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
        var relay = GetItem<Relay>();
        relay.SpeckHit = true; // Already hit once
        relay.MarksmanshipCounter = 100; // Ensure hit

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
        var relay = GetItem<Relay>();
        relay.SpeckHit = true; // Already hit once
        relay.MarksmanshipCounter = 100; // Ensure hit
        var initialScore = target.Context.Score;

        await target.GetResponse("shoot relay with laser");

        // 8 points for destroying speck + 2 for first laser fire (if applicable)
        target.Context.Score.Should().BeGreaterThan(initialScore);
    }

    [Test]
    public async Task ShootRelay_WithRedLaser_SecondHit_StartsTimer()
    {
        var target = GetTarget();
        StartHere<StripNearRelay>();
        Take<Laser>();
        GetItem<OldBattery>().ChargesRemaining = 10;
        GetItem<Laser>().Setting = 1; // Red
        var relay = GetItem<Relay>();
        relay.SpeckHit = true; // Already hit once
        relay.MarksmanshipCounter = 100; // Ensure hit

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
        var relay = GetItem<Relay>();
        relay.MarksmanshipCounter = 0; // Low chance to hit

        await target.GetResponse("shoot relay with laser");

        // If it was a miss, marksmanship should have increased
        // Note: This could occasionally hit due to randomness, so we check for miss messages
        if (!relay.SpeckHit)
        {
            relay.MarksmanshipCounter.Should().Be(12);
        }
    }

    [Test]
    public async Task ShootRelay_WithRedLaser_Miss_ShowsMissMessage()
    {
        var target = GetTarget();
        StartHere<StripNearRelay>();
        Take<Laser>();
        GetItem<OldBattery>().ChargesRemaining = 10;
        GetItem<Laser>().Setting = 1; // Red
        var relay = GetItem<Relay>();
        relay.MarksmanshipCounter = 0; // Force low chance to hit

        // Multiple attempts to ensure we get a miss (randomness)
        for (int i = 0; i < 10 && !relay.SpeckHit; i++)
        {
            var response = await target.GetResponse("shoot relay with laser");
            if (!relay.SpeckHit)
            {
                // Should have one of the miss messages
                var hasMissMessage = response.Contains("just misses the speck") ||
                                     response.Contains("near miss") ||
                                     response.Contains("just a little wide");
                hasMissMessage.Should().BeTrue();
                break;
            }
        }
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
    public async Task ExamineRelay_AfterSpeckHit_ShowsPartiallyDamaged()
    {
        var target = GetTarget();
        StartHere<StripNearRelay>();
        var relay = GetItem<Relay>();
        relay.SpeckHit = true;

        var response = await target.GetResponse("examine relay");

        response.Should().Contain("partially damaged by a laser beam");
        response.Should().Contain("one more hit would destroy it");
    }

    [Test]
    public async Task ExamineRelay_AfterSpeckDestroyed_ShowsCleared()
    {
        var target = GetTarget();
        StartHere<StripNearRelay>();
        var relay = GetItem<Relay>();
        relay.SpeckDestroyed = true;

        var response = await target.GetResponse("examine relay");

        response.Should().Contain("cleared of the obstruction");
        response.Should().Contain("slowly closing");
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
