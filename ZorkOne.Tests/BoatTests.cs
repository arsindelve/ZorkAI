using FluentAssertions;
using GameEngine;
using ZorkOne.Item;
using ZorkOne.Location;
using ZorkOne.Location.RiverLocation;

namespace ZorkOne.Tests;

public class BoatTests : EngineTestsBase
{
    [Test]
    public async Task GetInTheBoat_SharpItem()
    {
        var target = GetTarget();
        Take<Sceptre>();
        target.Context.CurrentLocation = Repository.GetLocation<DamBase>();
        var boat = Repository.GetItem<PileOfPlastic>();
        boat.IsInflated = true;

        var response = await target.GetResponse("get in the boat");

        response.Should().Contain("Oops!");
        response.Should().Contain("hissing");
        target.Context.CurrentLocation.SubLocation.Should().BeNull();
        boat.IsPunctured.Should().BeTrue();
        boat.IsInflated.Should().BeFalse();
    }

    [Test]
    public async Task Deflated_OnTheGround()
    {
        var target = GetTarget();
        Take<Sceptre>();
        target.Context.CurrentLocation = Repository.GetLocation<DamBase>();
        var boat = Repository.GetItem<PileOfPlastic>();
        boat.IsInflated = false;

        var response = await target.GetResponse("look");

        response.Should().Contain("There is a folded pile of plastic here which has a small valve attached");
    }

    [Test]
    public async Task Deflated_Examine()
    {
        var target = GetTarget();
        Take<Sceptre>();
        target.Context.CurrentLocation = Repository.GetLocation<DamBase>();
        var boat = Repository.GetItem<PileOfPlastic>();
        boat.IsInflated = false;

        var response = await target.GetResponse("examine boat");

        response.Should().Contain("pile of plastic");
        response.Should().NotContain("punctured");
    }

    [Test]
    public async Task Deflated_InInventory()
    {
        var target = GetTarget();
        Take<Sceptre>();
        target.Context.CurrentLocation = Repository.GetLocation<DamBase>();
        var boat = Repository.GetItem<PileOfPlastic>();
        boat.IsInflated = false;

        await target.GetResponse("take boat");
        var response = await target.GetResponse("i");

        response.Should().Contain("A pile of plastic");
        response.Should().NotContain("punctured");
    }

    [Test]
    public async Task Punctured_InInventory()
    {
        var target = GetTarget();
        Take<Sceptre>();
        target.Context.CurrentLocation = Repository.GetLocation<DamBase>();
        var boat = Repository.GetItem<PileOfPlastic>();
        boat.IsPunctured = true;

        await target.GetResponse("take boat");
        var response = await target.GetResponse("i");

        response.Should().Contain("A punctured boat");
    }

    [Test]
    public async Task Inflated_InInventory()
    {
        var target = GetTarget();
        Take<AirPump>();
        target.Context.CurrentLocation = Repository.GetLocation<DamBase>();

        await target.GetResponse("inflate plastic with pump");
        await target.GetResponse("take boat");
        var response = await target.GetResponse("i");

        response.Should().Contain("A magic boat");
        response.Should().Contain("The magic boat contains");
        response.Should().Contain("tan label");
    }

    [Test]
    public async Task Punctured_Examine()
    {
        var target = GetTarget();
        Take<Sceptre>();
        target.Context.CurrentLocation = Repository.GetLocation<DamBase>();
        var boat = Repository.GetItem<PileOfPlastic>();
        boat.IsPunctured = true;

        var response = await target.GetResponse("examine boat");

        response.Should().Contain("punctured boat");
    }

    [Test]
    public async Task Inflated_Examine()
    {
        var target = GetTarget();
        Take<Sceptre>();
        target.Context.CurrentLocation = Repository.GetLocation<DamBase>();
        var boat = Repository.GetItem<PileOfPlastic>();
        boat.IsPunctured = false;
        boat.IsInflated = true;

        var response = await target.GetResponse("examine boat");

        response.Should().Contain("magic boat");
    }

    [Test]
    public async Task Inflated_OnTheGround()
    {
        var target = GetTarget();
        Take<AirPump>();

        target.Context.CurrentLocation = Repository.GetLocation<DamBase>();

        await target.GetResponse("inflate plastic with pump");
        var response = await target.GetResponse("look");

        response.Should().Contain("There is a magic boat here");
        response.Should().Contain("The magic boat contains");
        response.Should().Contain("tan label");
    }

    [Test]
    public async Task Punctured_OnTheGround()
    {
        var target = GetTarget();
        Take<Sceptre>();
        target.Context.CurrentLocation = Repository.GetLocation<DamBase>();
        var boat = Repository.GetItem<PileOfPlastic>();
        boat.IsPunctured = true;

        var response = await target.GetResponse("look");

        response.Should().Contain("There is a punctured boat here");
    }

    [Test]
    public async Task GetInTheBoat_Punctured()
    {
        var target = GetTarget();

        target.Context.CurrentLocation = Repository.GetLocation<DamBase>();
        var boat = Repository.GetItem<PileOfPlastic>();
        boat.IsPunctured = true;

        var response = await target.GetResponse("get in the boat");

        response.Should().Contain("theory");
        response.Should().Contain("punctured boat");
        target.Context.CurrentLocation.SubLocation.Should().BeNull();
    }

    [Test]
    public async Task GetInTheBoat_NotInflated()
    {
        var target = GetTarget();

        target.Context.CurrentLocation = Repository.GetLocation<DamBase>();
        var boat = Repository.GetItem<PileOfPlastic>();
        boat.IsInflated = false;

        var response = await target.GetResponse("get in the boat");

        response.Should().Contain("theory");
        target.Context.CurrentLocation.SubLocation.Should().BeNull();
    }

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
        boat.CurrentLocation = Repository.GetLocation<FrigidRiverOne>();
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

    [Test]
    public async Task Inflate_InInventory()
    {
        var target = GetTarget();
        Take<AirPump>();
        Take<PileOfPlastic>();
        target.Context.CurrentLocation = Repository.GetLocation<DamBase>();

        var response = await target.GetResponse("inflate plastic with pump");
        response.Should().Contain("The boat must be on the ground to be inflated");
    }

    [Test]
    public async Task Inflate_NoPump()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<DamBase>();

        var response = await target.GetResponse("inflate plastic with pump");
        response.Should().Contain("You don't have");
    }

    [Test]
    public async Task Inflate_AlreadyInflated()
    {
        var target = GetTarget();
        Take<AirPump>();
        GetItem<PileOfPlastic>().IsInflated = true;
        target.Context.CurrentLocation = Repository.GetLocation<DamBase>();

        var response = await target.GetResponse("inflate plastic with pump");
        response.Should().Contain("Inflating it further would probably burst it");
    }

    [Test]
    public async Task Inflate_Punctured()
    {
        var target = GetTarget();
        Take<AirPump>();
        GetItem<PileOfPlastic>().IsPunctured = true;
        target.Context.CurrentLocation = Repository.GetLocation<DamBase>();

        var response = await target.GetResponse("inflate plastic with pump");
        response.Should().Contain("moron");
    }
    
    [Test]
    public async Task Inflate_DontSpecifyPump()
    {
        var target = GetTarget();
        Take<AirPump>();
        GetItem<PileOfPlastic>().IsPunctured = true;
        target.Context.CurrentLocation = Repository.GetLocation<DamBase>();

        var response = await target.GetResponse("inflate plastic");
        response.Should().Contain("lung power");
    }
    
    [Test]
    public async Task Inflate_DontSpecifyPump_InInventory()
    {
        var target = GetTarget();
        Take<PileOfPlastic>();
        GetItem<PileOfPlastic>().IsPunctured = true;
        target.Context.CurrentLocation = Repository.GetLocation<DamBase>();

        var response = await target.GetResponse("inflate plastic");
        response.Should().Contain("on the ground");
    }

    [Test]
    public async Task Fix()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<DamBase>();
        GetItem<PileOfPlastic>().IsPunctured = true;
        Take<ViscousMaterial>();

        var response = await target.GetResponse("fix the plastic with the gunk");
        response.Should().Contain("Well done. The boat is repaired");
        GetItem<PileOfPlastic>().IsPunctured.Should().BeFalse();
    }
    
    [Test]
    public async Task Fix_NotPunctured()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<DamBase>();
        GetItem<PileOfPlastic>().IsPunctured = false;
        Take<ViscousMaterial>();

        var response = await target.GetResponse("fix the plastic with the gunk");
        // Empty because it will go to the LLM
        response!.Trim().Should().BeEmpty();
        GetItem<PileOfPlastic>().IsPunctured.Should().BeFalse();
    }
    
    [Test]
    public async Task Fix_NoGunk()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<DamBase>();
        GetItem<PileOfPlastic>().IsPunctured = true;

        var response = await target.GetResponse("fix the plastic with the gunk");
        response.Should().Contain("You don't have");
        GetItem<PileOfPlastic>().IsPunctured.Should().BeTrue();
    }

    [Test]
    public async Task EnterTheBoat_ShouldWork_LikeGetIn()
    {
        var target = GetTarget();

        target.Context.CurrentLocation = Repository.GetLocation<DamBase>();
        var boat = Repository.GetItem<PileOfPlastic>();
        boat.IsInflated = true;

        var response = await target.GetResponse("enter the boat");

        response.Should().Contain("You are now in the magic boat.");
        target.Context.CurrentLocation.SubLocation.Should().NotBeNull();
    }

    [Test]
    public async Task EnterBoat_ShouldWork_LikeGetIn()
    {
        var target = GetTarget();

        target.Context.CurrentLocation = Repository.GetLocation<DamBase>();
        var boat = Repository.GetItem<PileOfPlastic>();
        boat.IsInflated = true;

        var response = await target.GetResponse("enter boat");

        response.Should().Contain("You are now in the magic boat.");
        target.Context.CurrentLocation.SubLocation.Should().NotBeNull();
    }
}