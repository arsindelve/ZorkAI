using FluentAssertions;
using Planetfall.Item.Computer;
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
}
