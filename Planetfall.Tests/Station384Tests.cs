using FluentAssertions;
using Planetfall.Item.Lawanda.Lab;
using Planetfall.Location.Computer;
using Planetfall.Location.Lawanda;

namespace Planetfall.Tests;

public class Station384Tests : EngineTestsBase
{
    [Test]
    public async Task Station384_ShouldHaveCorrectDescription()
    {
        var target = GetTarget();
        StartHere<Station384>();

        var response = await target.GetResponse("look");

        response.Should().Contain("standing on a square plate of heavy metal");
        response.Should().Contain("parallel to the plate beneath you");
        response.Should().Contain("identical metal plate");
        response.Should().Contain("To the east is a wide, metallic strip");
    }

    [Test]
    public async Task Station384_MoveEast_ShouldGoToStripNearStation()
    {
        var target = GetTarget();
        StartHere<Station384>();

        var response = await target.GetResponse("east");

        response.Should().Contain("Strip Near Station");
        target.Context.CurrentLocation.Should().BeOfType<StripNearStation>();
    }

    [Test]
    public async Task Station384_MoveWest_ShouldShowTransitionMessage()
    {
        var target = GetTarget();
        StartHere<Station384>();

        var response = await target.GetResponse("west");

        response.Should().Contain("You feel the familiar wrenching of your innards");
        response.Should().Contain("find yourself in a vast room whose distant walls are rushing straight toward you");
    }

    [Test]
    public async Task Station384_MoveWest_ShouldGoToMiniaturizationBooth()
    {
        var target = GetTarget();
        StartHere<Station384>();

        var response = await target.GetResponse("west");

        response.Should().Contain("Miniaturization Booth");
        target.Context.CurrentLocation.Should().BeOfType<MiniaturizationBooth>();
    }

    [Test]
    public async Task Station384_MoveWest_TransitionMessageShowsBeforeLocation()
    {
        var target = GetTarget();
        StartHere<Station384>();

        var response = await target.GetResponse("west");

        // The transition message should appear before the location description
        var transitionIndex = response.IndexOf("You feel the familiar wrenching of your innards");
        var locationIndex = response.IndexOf("Miniaturization Booth");

        transitionIndex.Should().BeGreaterThan(-1, "transition message should be present");
        locationIndex.Should().BeGreaterThan(-1, "location name should be present");
        transitionIndex.Should().BeLessThan(locationIndex, "transition message should appear before location name");
    }

    [Test]
    public async Task Station384_RoundTrip_FromBoothToStation384AndBack()
    {
        var target = GetTarget();
        StartHere<MiniaturizationBooth>();
        Take<MiniaturizationAccessCard>();

        // Activate the booth
        await target.GetResponse("slide miniaturization card through slot");

        // Type 384 to go to Station 384
        var response1 = await target.GetResponse("type 384");
        response1.Should().Contain("walls of the booth sliding away in all directions");
        target.Context.CurrentLocation.Should().BeOfType<Station384>();

        // Move back west to the booth
        var response2 = await target.GetResponse("west");
        response2.Should().Contain("You feel the familiar wrenching of your innards");
        response2.Should().Contain("find yourself in a vast room whose distant walls are rushing straight toward you");
        target.Context.CurrentLocation.Should().BeOfType<MiniaturizationBooth>();
    }

    [Test]
    public async Task Station384_BoothDisabledAfterTeleport()
    {
        var target = GetTarget();
        StartHere<MiniaturizationBooth>();
        Take<MiniaturizationAccessCard>();
        var booth = GetLocation<MiniaturizationBooth>();

        // Activate the booth
        await target.GetResponse("slide miniaturization card through slot");
        booth.IsEnabled.Should().BeTrue();

        // Type 384 to go to Station 384
        await target.GetResponse("type 384");
        booth.IsEnabled.Should().BeFalse("booth should be disabled after teleport");

        // Move back west to the booth
        await target.GetResponse("west");

        // Try to type a number - should say booth is not activated
        var response = await target.GetResponse("type 384");
        response.Should().Contain("Internal computer repair booth not activated");
    }
}