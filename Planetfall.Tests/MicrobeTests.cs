using FluentAssertions;
using Model.Interface;
using Moq;
using Planetfall.Item.Computer;
using Planetfall.Item.Kalamontee.Mech;
using Planetfall.Item.Lawanda.Lab;
using Planetfall.Location.Computer;

namespace Planetfall.Tests;

/// <summary>
/// Tests for the microbe battle on the silicon strip inside the computer (issue #113).
/// </summary>
public class MicrobeTests : EngineTestsBase
{
    /// <summary>
    /// Puts the player on the strip with the computer fixed and a fully-charged, controllable laser in
    /// hand. Returns the microbe (already spawned, since entering the Middle of Strip triggers it).
    /// </summary>
    private async Task<(GameEngine.GameEngine<PlanetfallGame, PlanetfallContext> target, Microbe microbe)> ArriveOnStripWithMicrobe()
    {
        var target = GetTarget();

        // Computer is fixed (speck destroyed) — this is the precondition for the microbe to appear.
        GetItem<Relay>().SpeckDestroyed = true;

        // Give the player a working laser with a fresh, deterministic battery.
        var laser = GetItem<Laser>();
        laser.Items.Clear();
        var battery = GetItem<FreshBattery>();
        battery.ChargesRemaining = 50;
        laser.ItemPlacedHere(battery);
        Take<Laser>();

        // Make the microbe's random flavor deterministic.
        var mockChooser = new Mock<IRandomChooser>();
        mockChooser.Setup(r => r.Choose(It.IsAny<List<string>>())).Returns((List<string> l) => l[0]);
        GetItem<Microbe>().Chooser = mockChooser.Object;
        laser.Chooser = mockChooser.Object;

        StartHere<StripNearRelay>();
        await target.GetResponse("south");

        return (target, GetItem<Microbe>());
    }

    [Test]
    public async Task Microbe_SpawnsOnStrip_WhenComputerFixedAndPlayerEntersMiddle()
    {
        var target = GetTarget();
        GetItem<Relay>().SpeckDestroyed = true;
        StartHere<StripNearRelay>();

        var response = await target.GetResponse("south");

        response.Should().Contain("giant elephant-sized monster lands on the strip");
        GetItem<Microbe>().IsActive.Should().BeTrue();
        target.Context.CurrentLocation.Should().BeOfType<MiddleOfStrip>();
    }

    [Test]
    public async Task Microbe_DoesNotSpawn_WhenComputerNotFixed()
    {
        var target = GetTarget();
        // Speck NOT destroyed.
        StartHere<StripNearRelay>();

        var response = await target.GetResponse("south");

        response.Should().NotContain("monster lands on the strip");
        GetItem<Microbe>().IsActive.Should().BeFalse();
    }

    [Test]
    public async Task Microbe_ClosesIn_AndDigestsPlayer_WhenLeftAlone()
    {
        var (target, microbe) = await ArriveOnStripWithMicrobe();

        // Three idle turns: closer, closer, then digested.
        var t1 = await target.GetResponse("wait");
        t1.Should().Contain("microbe");
        microbe.Counter.Should().Be(1);

        await target.GetResponse("wait");
        microbe.Counter.Should().Be(2);

        var death = await target.GetResponse("wait");
        death.Should().Contain("shoves you into its mucus-covered gullet");
        death.Should().Contain("You have died");
    }

    [Test]
    public async Task Microbe_BlocksStripExits_WhilePresent()
    {
        var (target, _) = await ArriveOnStripWithMicrobe();

        // Player is at Middle of Strip with the microbe; south (toward the station) is blocked.
        var response = await target.GetResponse("south");

        response.Should().Contain("gullet of a hungry microbe");
        target.Context.CurrentLocation.Should().BeOfType<MiddleOfStrip>();
    }

    [Test]
    public async Task Microbe_FollowsPlayer_BetweenStripRooms()
    {
        var (target, microbe) = await ArriveOnStripWithMicrobe();

        var response = await target.GetResponse("north");

        response.Should().Contain("follows you northward");
        target.Context.CurrentLocation.Should().BeOfType<StripNearRelay>();
        microbe.CurrentLocation.Should().Be(target.Context.CurrentLocation);
    }

    [Test]
    public async Task ShootMicrobe_WithRedBeam_PassesHarmlessly_AndDoesNotRepel()
    {
        var (target, microbe) = await ArriveOnStripWithMicrobe();
        await target.GetResponse("set laser to 1");
        var counterBefore = microbe.Counter;

        var response = await target.GetResponse("shoot microbe with laser");

        response.Should().Contain("passes harmlessly through its red skin");
        // Red beam doesn't repel it, so it still closes in this turn.
        microbe.Counter.Should().Be(counterBefore + 1);
    }

    [Test]
    public async Task ShootMicrobe_WithHotBeam_RepelsIt_AndSuppressesClosing()
    {
        var (target, microbe) = await ArriveOnStripWithMicrobe();
        await target.GetResponse("set laser to 3");
        var counterBefore = microbe.Counter;

        var response = await target.GetResponse("shoot microbe with laser");

        response.Should().Contain("The laser beam strikes the microbe");
        // A struck microbe lashes out instead of closing — counter does not advance this turn.
        microbe.Counter.Should().Be(counterBefore);
    }

    [Test]
    public async Task ShootMicrobe_NeverKillsIt_Directly()
    {
        var (target, microbe) = await ArriveOnStripWithMicrobe();
        await target.GetResponse("set laser to 4");

        for (var i = 0; i < 5; i++)
            await target.GetResponse("shoot microbe with laser");

        microbe.IsActive.Should().BeTrue();
    }

    [Test]
    public async Task ThrowHotLaserOffStrip_DispatchesMicrobe_AndClearsPath()
    {
        var (target, microbe) = await ArriveOnStripWithMicrobe();
        await target.GetResponse("set laser to 3");

        // Heat the laser past the threshold (WARMTH-FLAG > 7) by firing it repeatedly.
        for (var i = 0; i < 9; i++)
            await target.GetResponse("shoot microbe with laser");

        GetItem<Laser>().WarmthLevel.Should().BeGreaterThan(7);

        var response = await target.GetResponse("throw laser off strip");

        response.Should().Contain("Both the laser and the microbe plummet into the void");
        microbe.IsActive.Should().BeFalse();
        microbe.Dispatched.Should().BeTrue();

        // Path is now clear: the player can leave the strip to the south.
        var moveResponse = await target.GetResponse("south");
        moveResponse.Should().NotContain("gullet of a hungry microbe");
        target.Context.CurrentLocation.Should().BeOfType<StripNearStation>();
    }

    [Test]
    public async Task ThrowColdLaserOffStrip_LosesLaser_ButMicrobeRemains()
    {
        var (target, microbe) = await ArriveOnStripWithMicrobe();

        var response = await target.GetResponse("throw laser off strip");

        response.Should().Contain("disappears into the void");
        microbe.IsActive.Should().BeTrue();
        microbe.Dispatched.Should().BeFalse();
    }

    [Test]
    public async Task GiveVeryHotLaserToMicrobe_KillsIt()
    {
        var (target, microbe) = await ArriveOnStripWithMicrobe();
        await target.GetResponse("set laser to 3");

        // Heat past WARMTH-FLAG > 10.
        for (var i = 0; i < 12; i++)
            await target.GetResponse("shoot microbe with laser");

        GetItem<Laser>().WarmthLevel.Should().BeGreaterThan(10);

        var response = await target.GetResponse("give laser to microbe");

        response.Should().Contain("writhing");
        response.Should().Contain("rolls off the edge of the strip");
        microbe.Dispatched.Should().BeTrue();
    }

    [Test]
    public async Task GiveColdLaserToMicrobe_IsIgnored_AndMicrobeRemains()
    {
        var (target, microbe) = await ArriveOnStripWithMicrobe();
        await target.GetResponse("set laser to 3");

        // Warmth <= 7: the microbe won't bother with the laser.
        for (var i = 0; i < 4; i++)
            await target.GetResponse("shoot microbe with laser");

        GetItem<Laser>().WarmthLevel.Should().BeLessThanOrEqualTo(7);

        var response = await target.GetResponse("give laser to microbe");

        response.Should().Contain("ignores the laser");
        microbe.IsActive.Should().BeTrue();
        microbe.Dispatched.Should().BeFalse();
        // The laser was not consumed, so the player still has it.
        target.Context.HasItem<Laser>().Should().BeTrue();
    }

    [Test]
    public async Task GiveWarmButNotHotLaserToMicrobe_IsDevoured_ButMicrobeSurvives()
    {
        var (target, microbe) = await ArriveOnStripWithMicrobe();
        await target.GetResponse("set laser to 3");

        // Warmth in the (7, 10] band: the microbe eats the laser but isn't killed by the heat.
        for (var i = 0; i < 9; i++)
            await target.GetResponse("shoot microbe with laser");

        GetItem<Laser>().WarmthLevel.Should().BeInRange(8, 10);

        var response = await target.GetResponse("give laser to microbe");

        response.Should().Contain("devours the laser");
        response.Should().Contain("turns toward you");
        microbe.IsActive.Should().BeTrue();
        microbe.Dispatched.Should().BeFalse();
        // The laser is gone (eaten), even though the microbe survived.
        target.Context.HasItem<Laser>().Should().BeFalse();
    }

    [Test]
    public async Task ShootMicrobe_WhileHoldingHotLaser_TriesToSnatchTheWeapon()
    {
        var (target, _) = await ArriveOnStripWithMicrobe();
        await target.GetResponse("set laser to 3");

        // Drive warmth above 7 (but below the deadly 13) while still holding the laser.
        string response = string.Empty;
        for (var i = 0; i < 9; i++)
            response = await target.GetResponse("shoot microbe with laser");

        GetItem<Laser>().WarmthLevel.Should().BeInRange(8, 13);
        response.Should().Contain("snatch it away from the monster's grasp");
    }

    [Test]
    public async Task ShootMicrobe_WhileHoldingScaldingLaser_LungesAndKillsPlayer()
    {
        var (target, _) = await ArriveOnStripWithMicrobe();
        await target.GetResponse("set laser to 3");

        // Keep firing while holding the laser; once warmth passes 13 the next hit is fatal.
        string death = string.Empty;
        for (var i = 0; i < 20; i++)
        {
            var response = await target.GetResponse("shoot microbe with laser");
            if (response.Contains("You have died"))
            {
                death = response;
                break;
            }
        }

        death.Should().Contain("losing your balance, fall over the edge of the strip");
        death.Should().Contain("You have died");
    }

    [Test]
    public async Task SectorReactivation_KillsPlayer_WhileOnStrip()
    {
        var (target, _) = await ArriveOnStripWithMicrobe();

        // Force the sector-activation timer to the brink.
        var timer = GetItem<SectorActivationTimer>();
        target.Context.RegisterActor(timer);
        timer.TurnsRemaining = 1;

        var response = await target.GetResponse("shoot microbe with laser");

        response.Should().Contain("comes back to life with a surge of electric current");
        response.Should().Contain("You have died");
    }
}
