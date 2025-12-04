using FluentAssertions;
using Planetfall.Item.Kalamontee.Mech;
using Planetfall.Location.Kalamontee.Mech;

namespace Planetfall.Tests;

public class StorageEastTests : EngineTestsBase
{
    [Test]
    public async Task NavigateToStorageEast_FromMechCorridorNorth()
    {
        var target = GetTarget();
        StartHere<MechCorridorNorth>();

        var response = await target.GetResponse("east");

        response.Should().Contain("Storage East");
        target.Context.CurrentLocation.Should().BeOfType<StorageEast>();
    }

    [Test]
    public async Task NavigateBackToMechCorridorNorth_FromStorageEast()
    {
        var target = GetTarget();
        StartHere<StorageEast>();

        var response = await target.GetResponse("west");

        response.Should().Contain("Mech Corridor North");
        target.Context.CurrentLocation.Should().BeOfType<MechCorridorNorth>();
    }

    [Test]
    public async Task LookAtStorageEast_ContainsCorrectDescription()
    {
        var target = GetTarget();
        StartHere<StorageEast>();

        var response = await target.GetResponse("look");

        response.Should().Contain("A small room for storage");
        response.Should().Contain("The exit is to the west");
    }

    [Test]
    public async Task StorageEast_ContainsOilCan()
    {
        var target = GetTarget();
        StartHere<StorageEast>();

        var response = await target.GetResponse("look");

        response.Should().Contain("One dusty shelf, otherwise bare, holds a small oil can");
    }

    [Test]
    public void OilCan_ExistsInLocation()
    {
        var location = StartHere<StorageEast>();

        location.HasItem<OilCan>().Should().BeTrue();
        var oilCan = GetItem<OilCan>();
        oilCan.CurrentLocation.Should().Be(location);
    }

    [Test]
    public async Task TakeOilCan_ManuallyPlaced()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        Take<OilCan>();

        var response = await target.GetResponse("inventory");

        response.Should().Contain("An oil can");
    }

    [Test]
    public async Task TakeOilCan_Success()
    {
        var target = GetTarget();
        StartHere<StorageEast>();

        var response = await target.GetResponse("take oil can");

        response.Should().Contain("Taken");
        target.Context.HasItem<OilCan>().Should().BeTrue();
    }

    [Test]
    public async Task TakeOilCan_UsingShortName()
    {
        var target = GetTarget();
        StartHere<StorageEast>();

        var response = await target.GetResponse("take can");

        response.Should().Contain("Taken");
        target.Context.HasItem<OilCan>().Should().BeTrue();
    }

    [Test]
    public async Task DropOilCan_AfterTaking()
    {
        var target = GetTarget();
        StartHere<StorageEast>();
        await target.GetResponse("take oil can");

        var response = await target.GetResponse("drop oil can");

        response.Should().Contain("Dropped");
        target.Context.HasItem<OilCan>().Should().BeFalse();
        GetLocation<StorageEast>().HasItem<OilCan>().Should().BeTrue();
    }

    [Test]
    public async Task OilCanDescription_AfterBeingDropped()
    {
        var target = GetTarget();
        StartHere<StorageEast>();
        await target.GetResponse("take oil can");
        await target.GetResponse("drop oil can");

        var response = await target.GetResponse("look");

        response.Should().Contain("There is an oil can here");
        response.Should().NotContain("One dusty shelf");
    }

    [Test]
    public async Task Inventory_ShowsOilCan()
    {
        var target = GetTarget();
        StartHere<StorageEast>();
        await target.GetResponse("take oil can");

        var response = await target.GetResponse("inventory");

        response.Should().Contain("An oil can");
    }

    [Test]
    public async Task TakeOilCan_ThenNavigateAway_StillInInventory()
    {
        var target = GetTarget();
        StartHere<StorageEast>();
        await target.GetResponse("take oil can");
        await target.GetResponse("west");

        var response = await target.GetResponse("inventory");

        response.Should().Contain("An oil can");
        target.Context.HasItem<OilCan>().Should().BeTrue();
    }

    [Test]
    public async Task LeaveAndReturn_OilCanStillThere_IfNotTaken()
    {
        var target = GetTarget();
        StartHere<StorageEast>();
        await target.GetResponse("west");
        await target.GetResponse("east");

        var response = await target.GetResponse("look");

        response.Should().Contain("One dusty shelf, otherwise bare, holds a small oil can");
    }

    [Test]
    public async Task LeaveAndReturn_OilCanGone_IfTaken()
    {
        var target = GetTarget();
        StartHere<StorageEast>();
        await target.GetResponse("take oil can");
        await target.GetResponse("west");
        await target.GetResponse("east");

        var response = await target.GetResponse("look");

        response.Should().NotContain("oil can");
    }
}
