using FluentAssertions;
using Planetfall.Item.Kalamontee.Admin;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Planetfall.Item.Lawanda.Lab;
using Planetfall.Location;
using Planetfall.Location.Computer;
using Planetfall.Location.Lawanda;

namespace Planetfall.Tests;

public class MiniaturizationBoothTests : EngineTestsBase
{
    [Test]
    public async Task ActivateBooth_WithMiniaturizationCard()
    {
        var target = GetTarget();
        StartHere<MiniaturizationBooth>();
        Take<MiniaturizationAccessCard>();

        var response = await target.GetResponse("slide miniaturization access card through slot");

        response.Should().Contain("melodic high-pitched voice");
        response.Should().Contain("Miniaturization and teleportation booth activated");
        response.Should().Contain("Please type in damaged sector number");
    }

    [Test]
    public async Task ActivateBooth_WithMiniaturizationCard_VersionTwo()
    {
        var target = GetTarget();
        StartHere<MiniaturizationBooth>();
        Take<MiniaturizationAccessCard>();

        var response = await target.GetResponse("slide miniaturization card through slot");

        response.Should().Contain("melodic high-pitched voice");
        response.Should().Contain("Miniaturization and teleportation booth activated");
        response.Should().Contain("Please type in damaged sector number");
    }

    [Test]
    public async Task ActivateBooth_WithMiniaturizationCard_VersionThree()
    {
        var target = GetTarget();
        StartHere<MiniaturizationBooth>();
        Take<MiniaturizationAccessCard>();

        var response = await target.GetResponse("slide miniaturization through slot");

        response.Should().Contain("melodic high-pitched voice");
        response.Should().Contain("Miniaturization and teleportation booth activated");
        response.Should().Contain("Please type in damaged sector number");
    }

    [Test]
    public async Task ActivateBooth_WithMiniaturizationCard_ShortVersion()
    {
        var target = GetTarget();
        StartHere<MiniaturizationBooth>();
        Take<MiniaturizationAccessCard>();

        var response = await target.GetResponse("slide mini card through slot");

        response.Should().Contain("melodic high-pitched voice");
        response.Should().Contain("Miniaturization and teleportation booth activated");
        response.Should().Contain("Please type in damaged sector number");
    }

    [Test]
    public async Task ActivateBooth_WrongCard()
    {
        var target = GetTarget();
        StartHere<MiniaturizationBooth>();
        Take<KitchenAccessCard>();

        var response = await target.GetResponse("slide kitchen access card through slot");

        response.Should().Contain("Inkorekt awtharazaashun");
    }

    [Test]
    public async Task ExamineSlot()
    {
        var target = GetTarget();
        StartHere<MiniaturizationBooth>();

        var response = await target.GetResponse("examine slot");

        response.Should().Contain("ten centimeters wide");
        response.Should().Contain("two centimeters deep");
        response.Should().Contain("ridges of metal");
    }

    [Test]
    public async Task BoothSetsIsEnabledFlag()
    {
        var target = GetTarget();
        StartHere<MiniaturizationBooth>();
        Take<MiniaturizationAccessCard>();

        var booth = GetLocation<MiniaturizationBooth>();
        booth.IsEnabled.Should().BeFalse();

        await target.GetResponse("slide miniaturization card through slot");

        booth.IsEnabled.Should().BeTrue();
    }

    [Test]
    public async Task TypeNumber_BoothNotActivated()
    {
        var target = GetTarget();
        StartHere<MiniaturizationBooth>();

        var response = await target.GetResponse("type 5");

        response.Should().Contain("A recording says \"Internal computer repair booth not activated.\"");
    }

    [Test]
    public async Task TypeWrongNumber_PlayerDies()
    {
        var target = GetTarget();
        StartHere<MiniaturizationBooth>();
        Take<MiniaturizationAccessCard>();

        await target.GetResponse("slide miniaturization card through slot");
        var response = await target.GetResponse("type 5");

        response.Should().Contain("Ooops! You seem to have transported yourself into an active sector of the computer");
        response.Should().Contain("fried by powerful electric currents");
        response.Should().Contain("You have died");
    }

    [Test]
    public async Task TypeCorrectNumber_TeleportsToStation384()
    {
        var target = GetTarget();
        StartHere<MiniaturizationBooth>();
        Take<MiniaturizationAccessCard>();

        await target.GetResponse("slide miniaturization card through slot");
        var response = await target.GetResponse("type 384");

        response.Should().Contain("walls of the booth sliding away in all directions");
        response.Should().Contain("momentary queasiness in the pit of your stomach");
        response.Should().NotContain("died");
        target.Context.CurrentLocation.Should().BeOfType<Station384>();
    }

    [Test]
    public async Task PressWrongNumber_PlayerDies()
    {
        var target = GetTarget();
        StartHere<MiniaturizationBooth>();
        Take<MiniaturizationAccessCard>();

        await target.GetResponse("slide miniaturization card through slot");
        var response = await target.GetResponse("press 7");

        response.Should().Contain("Ooops! You seem to have transported yourself into an active sector of the computer");
        response.Should().Contain("fried by powerful electric currents");
        response.Should().Contain("You have died");
    }

    [Test]
    public async Task KeyWrongNumber_PlayerDies()
    {
        var target = GetTarget();
        StartHere<MiniaturizationBooth>();
        Take<MiniaturizationAccessCard>();

        await target.GetResponse("slide miniaturization card through slot");
        var response = await target.GetResponse("key 3");

        response.Should().Contain("Ooops! You seem to have transported yourself into an active sector of the computer");
        response.Should().Contain("fried by powerful electric currents");
        response.Should().Contain("You have died");
    }
}
