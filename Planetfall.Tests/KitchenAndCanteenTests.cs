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
    public async Task PutCanteenInNiche()
    {
        // Regression guard for issue #424: the "in niche" phrasing must keep working.
        var target = GetTarget();
        StartHere<Kitchen>();
        target.Context.ItemPlacedHere<Canteen>();

        var response = await target.GetResponse("put canteen in niche");
        response.Should()
            .Contain(
                "The canteen fits snugly into the octagonal niche, its mouth resting just below the spout of the machine");
        Repository.GetItem<KitchenMachine>().Items.Should().Contain(Repository.GetItem<Canteen>());
    }

    [Test]
    public async Task PutCanteenUnderSpout()
    {
        // Issue #424: the machine's own description calls the niche "beneath a spout" and the sibling
        // Machine Shop dispenser accepts "put flask under spout", but this dispenser used to no-op on the
        // equally-natural "put canteen under spout". It should land the canteen in the niche just like
        // "put canteen in niche".
        var target = GetTarget();
        StartHere<Kitchen>();
        target.Context.ItemPlacedHere<Canteen>();

        var response = await target.GetResponse("put canteen under spout");
        response.Should()
            .Contain(
                "The canteen fits snugly into the octagonal niche, its mouth resting just below the spout of the machine");
        Repository.GetItem<KitchenMachine>().Items.Should().Contain(Repository.GetItem<Canteen>());
    }

    [Test]
    public async Task PutCanteenUnderneathSpout()
    {
        // Issue #424: "underneath" is an equally-natural synonym for the same placement.
        var target = GetTarget();
        StartHere<Kitchen>();
        target.Context.ItemPlacedHere<Canteen>();

        var response = await target.GetResponse("put canteen underneath spout");
        response.Should()
            .Contain(
                "The canteen fits snugly into the octagonal niche, its mouth resting just below the spout of the machine");
        Repository.GetItem<KitchenMachine>().Items.Should().Contain(Repository.GetItem<Canteen>());
    }

    [Test]
    public async Task PutCanteenUnderSpout_CanteenNotInScope_FallsThroughToNarrator()
    {
        // Issue #424 review: when no canteen is in scope (not held, not in this room), the phrasing must
        // fall through to the narrator rather than claiming "You don't have the canteen." - mirroring the
        // Machine Shop and the "put canteen in niche" path.
        var target = GetTarget();
        StartHere<Kitchen>();
        // The canteen exists in the story but is neither held nor present in the Kitchen.

        var response = await target.GetResponse("put canteen under spout");

        Repository.GetItem<KitchenMachine>().Items.Should().BeEmpty();
        response.Should().NotContain("You don't have the canteen");
        response.Should().NotContain("fits snugly into the octagonal niche");
    }

    [Test]
    public async Task PutCanteenUnderSpout_OnFloorNotHeld()
    {
        // Issue #424 review: a canteen in scope but not held gets the same "you don't have it" guard as
        // the in-niche path, and is not placed.
        var target = GetTarget();
        var kitchen = StartHere<Kitchen>();
        kitchen.ItemPlacedHere(GetItem<Canteen>());

        var response = await target.GetResponse("put canteen under spout");

        response.Should().Contain("You don't have the canteen");
        Repository.GetItem<KitchenMachine>().Items.Should().BeEmpty();
    }

    [Test]
    public async Task PutKitUnderSpout_DoesNotFit()
    {
        // Issue #424 review: a wrong-type item under the spout gets the same "It doesn't fit in the niche."
        // refusal as "put kit in machine", instead of falling through to the generic narrator.
        var target = GetTarget();
        StartHere<Kitchen>();
        target.Context.ItemPlacedHere<SurvivalKit>();

        var response = await target.GetResponse("put kit under spout");

        response.Should().Contain("It doesn't fit in the niche");
        Repository.GetItem<KitchenMachine>().Items.Should().BeEmpty();
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

    [Test]
    public async Task OpenCanteen_Empty()
    {
        var target = GetTarget();
        StartHere<MessCorridor>();
        Take<Canteen>();

        var response = await target.GetResponse("open canteen");
        response.Should().Contain("Opened");
    }

    [Test]
    public async Task OpenCanteen_WithProteinLiquid()
    {
        var target = GetTarget();
        StartHere<MessCorridor>();
        Take<Canteen>();
        GetItem<Canteen>().Items.Add(GetItem<ProteinLiquid>());

        var response = await target.GetResponse("open canteen");
        response.Should().Contain("Opening the canteen reveals a quantity of protein-rich liquid.");
    }

    // TODO: drink dont have it

    [Test]
    public async Task PutBrushInCanteen_TypeRefusal_NamesTheItem_AndIsNotBlank()
    {
        var target = GetTarget();
        StartHere<MessCorridor>();
        Take<Canteen>().IsOpen = true;
        Take<Brush>();

        var response = await target.GetResponse("put brush in canteen");

        // Issue #422: the canteen only holds protein liquid, so a brush is refused on type. That
        // refusal must be a deterministic message that names the item - not a fall-through to the
        // AI narrator, which (with a stubbed generation client) leaves a blank line.
        response.Should().NotBeNullOrWhiteSpace();
        response.Should().Contain("brush");
        GetItem<Canteen>().Items.Should().BeEmpty();
    }
}