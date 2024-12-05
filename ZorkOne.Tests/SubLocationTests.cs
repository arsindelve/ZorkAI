using FluentAssertions;
using GameEngine;
using NUnit.Framework;
using UnitTests;
using ZorkOne.Item;
using ZorkOne.Location;
using ZorkOne.Location.RiverLocation;

namespace ZorkOne.Tests;

public class SubLocationTests : EngineTestsBase
{
    [Test]
    public async Task GetInTheBoat()
    {
        var target = GetTarget();

        target.Context.CurrentLocation = Repository.GetLocation<DamBase>();
        var boat = Repository.GetItem<PileOfPlastic>();
        boat.IsInflated = true;

        var response = await target.GetResponse("get in the boat");

        response.Should().Contain("You are now in the magic boat.");
        target.Context.CurrentLocation.SubLocation.Should().NotBeNull();
    }

    [Test]
    public async Task GetOutOfTheBoat_NotInIt()
    {
        var target = GetTarget();

        target.Context.CurrentLocation = Repository.GetLocation<DamBase>();
        var boat = Repository.GetItem<PileOfPlastic>();
        boat.IsInflated = true;

        var response = await target.GetResponse("get out of the boat");

        response.Should().Contain("You're not in the boat.");
        target.Context.CurrentLocation.SubLocation.Should().BeNull();
    }

    [Test]
    public async Task GetInTheBoat_AlreadyThere()
    {
        var target = GetTarget();

        target.Context.CurrentLocation = Repository.GetLocation<DamBase>();
        var boat = Repository.GetItem<PileOfPlastic>();
        boat.IsInflated = true;

        await target.GetResponse("get in the boat");
        var response = await target.GetResponse("get in the boat");

        response.Should().Contain("You're already in the boat");
        target.Context.CurrentLocation.SubLocation.Should().NotBeNull();
    }

    [Test]
    public async Task InTheBoat_Look()
    {
        var target = GetTarget();

        target.Context.CurrentLocation = Repository.GetLocation<DamBase>();
        var boat = Repository.GetItem<PileOfPlastic>();
        boat.IsInflated = true;
        target.Context.CurrentLocation.SubLocation = boat;

        var response = await target.GetResponse("look");

        response.Should().Contain("in the magic boat");
    }

    [Test]
    public async Task GetOutOfTheBoat_OnLand()
    {
        var target = GetTarget();

        target.Context.CurrentLocation = Repository.GetLocation<DamBase>();
        var boat = Repository.GetItem<PileOfPlastic>();
        boat.IsInflated = true;
        target.Context.CurrentLocation.SubLocation = boat;

        var response = await target.GetResponse("get out of the boat");

        response.Should().Contain("You are on your own feet again");
        target.Context.CurrentLocation.SubLocation.Should().BeNull();
    }

    [Test]
    public async Task NotInTheBoat_Look()
    {
        var target = GetTarget();

        target.Context.CurrentLocation = Repository.GetLocation<DamBase>();
        var boat = Repository.GetItem<PileOfPlastic>();
        boat.IsInflated = true;
        target.Context.CurrentLocation.SubLocation = null;

        var response = await target.GetResponse("look");

        response.Should().NotContain("in the magic boat");
    }

    [Test]
    public async Task GetOutOfTheBoat_InRiver()
    {
        var target = GetTarget();

        target.Context.CurrentLocation = Repository.GetLocation<FrigidRiverOne>();
        var boat = Repository.GetItem<PileOfPlastic>();
        boat.IsInflated = true;
        target.Context.CurrentLocation.SubLocation = boat;

        var response = await target.GetResponse("get out of the boat");

        response.Should().Contain("You realize that getting out here would be fatal");
        target.Context.CurrentLocation.SubLocation.Should().NotBeNull();
    }

    [Test]
    public async Task InTheBoat_GoSomewhereOnLand()
    {
        var target = GetTarget();

        target.Context.CurrentLocation = Repository.GetLocation<DamBase>();
        var boat = Repository.GetItem<PileOfPlastic>();
        boat.IsInflated = true;

        await target.GetResponse("get in the boat");
        var response = await target.GetResponse("N");

        response.Should().Contain("You can't go there in a magic boat");
        target.Context.CurrentLocation.SubLocation.Should().NotBeNull();
    }

    [Test]
    public async Task InTheBoat_GoDownRiver()
    {
        var target = GetTarget();

        target.Context.CurrentLocation = Repository.GetLocation<DamBase>();
        var boat = Repository.GetItem<PileOfPlastic>();
        boat.IsInflated = true;

        await target.GetResponse("get in the boat");
        await target.GetResponse("launch");
        var response = await target.GetResponse("wait");

        response.Should().Contain("The flow of the river carries you downstream");
        response.Should().Contain("in the magic boat");
        response.Should().Contain("The river turns a corner here making it impossible to see the Dam");

        var frigidRiverTwo = Repository.GetLocation<FrigidRiverTwo>();

        target.Context.CurrentLocation.SubLocation.Should().Be(boat);
        target.Context.CurrentLocation.Should().Be(frigidRiverTwo);
        boat.CurrentLocation.Should().Be(frigidRiverTwo);
    }
}