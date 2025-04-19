using FluentAssertions;
using GameEngine;
using Planetfall.Item.Feinstein;
using Planetfall.Item.Kalamontee;
using Planetfall.Item.Kalamontee.Admin;
using Planetfall.Location.Kalamontee;

namespace Planetfall.Tests;

public class KitchenAndCanteenTests : EngineTestsBase
{
    [Test]
    public async Task CanteenDescription_Closed()
    {
        var target = GetTarget();
        StartHere<MessCorridor>();
        target.Context.ItemPlacedHere<Canteen>();

        var response = await target.GetResponse("examine canteen");
        response.Should().Contain("The canteen is closed");
    }

    [Test]
    public async Task OpenTheDoor()
    {
        var target = GetTarget();
        StartHere<Kitchen>();
        GetItem<KitchenDoor>().IsOpen = true;

        var response = await target.GetResponse("open door");
        response.Should().Contain("It is open");
    }

    [Test]
    public async Task CloseTheDoor()
    {
        var target = GetTarget();
        StartHere<Kitchen>();
        GetItem<KitchenDoor>().IsOpen = true;

        var response = await target.GetResponse("close door");
        response.Should().Contain("The door seems designed to slide shut on its own");
    }

    [Test]
    public async Task CanteenDescription_Open()
    {
        var target = GetTarget();
        StartHere<Kitchen>();
        target.Context.ItemPlacedHere<Canteen>();

        var response = await target.GetResponse("examine canteen");
        response.Should().Contain("The canteen is closed");
    }

    [Test]
    public async Task PutKitInMachine_Nope()
    {
        var target = GetTarget();
        StartHere<Kitchen>();
        target.Context.ItemPlacedHere<SurvivalKit>();

        var response = await target.GetResponse("put kit in machine");
        response.Should().Contain("It doesn't fit in the niche");
        GetItem<KitchenMachine>().Items.Should().BeEmpty();
    }

    [Test]
    public async Task PutCanteenInMachine()
    {
        var target = GetTarget();
        StartHere<Kitchen>();
        target.Context.ItemPlacedHere<Canteen>();

        var response = await target.GetResponse("put canteen in machine");
        response.Should()
            .Contain(
                "The canteen fits snugly into the octagonal niche, its mouth resting just below the spout of the machine");
        Repository.GetItem<KitchenMachine>().Items.Should().NotBeEmpty();
    }

    [Test]
    public async Task PressButton_MachineEmpty()
    {
        var target = GetTarget();
        StartHere<Kitchen>();
        target.Context.ItemPlacedHere<Canteen>();

        var response = await target.GetResponse("press button");
        response.Should()
            .Contain(
                "A thick, brownish liquid pours from the spout and splashes to the floor, where it quickly evaporates");
    }

    [Test]
    public async Task PressButton_CanteenClosed()
    {
        var target = GetTarget();
        StartHere<Kitchen>();
        GetItem<KitchenMachine>().ItemPlacedHere<Canteen>();

        var response = await target.GetResponse("press button");
        response.Should().Contain("A thick, brown liquid spills over the closed canteen, dribbles");
    }

    [Test]
    public async Task PressButton_Full_NoUniform()
    {
        var target = GetTarget();
        StartHere<Kitchen>();
        GetItem<Canteen>().Items.Add(GetItem<ProteinLiquid>());
        GetItem<Canteen>().IsOpen = true;
        GetItem<KitchenMachine>().ItemPlacedHere<Canteen>();

        var response = await target.GetResponse("press button");
        response.Should()
            .Contain("The brown liquid splashes over the mouth of the already-filled canteen, creating a mess");
        response.Should().NotContain("and staining your uniform");
    }

    [Test]
    public async Task PressButton_Full_Uniform()
    {
        var target = GetTarget();
        target.Context.ItemPlacedHere<PatrolUniform>();
        GetItem<PatrolUniform>().BeingWorn = true;
        StartHere<Kitchen>();
        GetItem<KitchenMachine>().ItemPlacedHere<Canteen>();
        GetItem<Canteen>().Items.Add(Repository.GetItem<ProteinLiquid>());
        GetItem<Canteen>().IsOpen = true;

        var response = await target.GetResponse("press button");
        response.Should()
            .Contain("The brown liquid splashes over the mouth of the already-filled canteen, creating a mess");
        response.Should().Contain("and staining your uniform");
    }

    [Test]
    public async Task PressButton_HappyPath()
    {
        var target = GetTarget();
        StartHere<Kitchen>();
        Take<Canteen>().IsOpen = true;
        GetItem<KitchenMachine>().ItemPlacedHere<Canteen>();

        var response = await target.GetResponse("press button");
        response.Should().Contain("The canteen fills almost to the brim with a brown liquid");
        Repository.GetItem<Canteen>().Items.Should().NotBeEmpty();
    }


    // TODO: drink dont have it
    // TODO: put something in canteen
}