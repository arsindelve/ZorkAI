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

    // CardboardBox tests
    [Test]
    public async Task LookAtStorageEast_ShowsBoxWithContents()
    {
        var target = GetTarget();
        StartHere<StorageEast>();

        var response = await target.GetResponse("look");

        response.Should().Contain("Storage East");
        response.Should().Contain("A small room for storage");
        response.Should().Contain("On the floor beneath the shelves sits a small cardboard box");
        response.Should().Contain("The cardboard box contains:");
        response.Should().Contain("A good ninety-ohm bedistor");
        response.Should().Contain("A K-series megafuse");
        response.Should().Contain("A B-series megafuse");
        response.Should().Contain("A cracked seventeen-centimeter fromitz board");
        response.Should().Contain("One dusty shelf, otherwise bare, holds a small oil can");
    }

    [Test]
    public void CardboardBox_ExistsInStorageEast()
    {
        GetTarget();
        var location = StartHere<StorageEast>();

        location.HasItem<CardboardBox>().Should().BeTrue();
    }

    [Test]
    public void CardboardBox_ContainsFourItems()
    {
        GetTarget();
        StartHere<StorageEast>();
        var box = GetItem<CardboardBox>();

        box.Items.Count.Should().Be(4);
        box.HasItem<GoodBedistor>().Should().BeTrue();
        box.HasItem<KSeriesMegafuse>().Should().BeTrue();
        box.HasItem<BSeriesMegafuse>().Should().BeTrue();
        box.HasItem<CrackedFromitzBoard>().Should().BeTrue();
    }

    [Test]
    public async Task TakeCardboardBox_Success()
    {
        var target = GetTarget();
        StartHere<StorageEast>();

        var response = await target.GetResponse("take box");

        response.Should().Contain("Taken");
        target.Context.HasItem<CardboardBox>().Should().BeTrue();
    }

    [Test]
    public async Task TakeCardboardBox_ItemsStayInside()
    {
        var target = GetTarget();
        StartHere<StorageEast>();
        await target.GetResponse("take box");

        var box = GetItem<CardboardBox>();
        box.Items.Count.Should().Be(4);
        box.HasItem<GoodBedistor>().Should().BeTrue();
        box.HasItem<KSeriesMegafuse>().Should().BeTrue();
        box.HasItem<BSeriesMegafuse>().Should().BeTrue();
        box.HasItem<CrackedFromitzBoard>().Should().BeTrue();
    }

    [Test]
    public async Task ExamineBox_ShowsContents()
    {
        var target = GetTarget();
        StartHere<StorageEast>();

        var response = await target.GetResponse("examine box");

        // Should show the exact format specified by the user
        response.Should().Contain("The cardboard box contains:");
        response.Should().Contain("A good ninety-ohm bedistor");
        response.Should().Contain("A K-series megafuse");
        response.Should().Contain("A B-series megafuse");
        response.Should().Contain("A cracked seventeen-centimeter fromitz board");
        // The examine description doesn't include "small" prefix
    }

    [Test]
    public async Task TakeItemFromBox_Success()
    {
        var target = GetTarget();
        StartHere<StorageEast>();

        var response = await target.GetResponse("take bedistor");

        response.Should().Contain("Taken");
        target.Context.HasItem<GoodBedistor>().Should().BeTrue();
    }

    [Test]
    public async Task TakeItemFromBox_RemovedFromBox()
    {
        var target = GetTarget();
        StartHere<StorageEast>();
        await target.GetResponse("take bedistor");

        var box = GetItem<CardboardBox>();
        box.HasItem<GoodBedistor>().Should().BeFalse();
        box.Items.Count.Should().Be(3);
    }

    [Test]
    public async Task TakeMultipleItemsFromBox()
    {
        var target = GetTarget();
        StartHere<StorageEast>();

        await target.GetResponse("take bedistor");
        await target.GetResponse("take k-series megafuse");

        var box = GetItem<CardboardBox>();
        box.Items.Count.Should().Be(2);
        target.Context.HasItem<GoodBedistor>().Should().BeTrue();
        target.Context.HasItem<KSeriesMegafuse>().Should().BeTrue();
    }

    [Test]
    public void BoxCapacity_CanHold10Units()
    {
        GetTarget();
        StartHere<StorageEast>();
        var box = GetItem<CardboardBox>();

        // Verify box can hold all 4 items (total size should be <= 10)
        var totalSize = box.Items.Sum(i => i.Size);
        totalSize.Should().BeLessThanOrEqualTo(10);
    }

    [Test]
    public async Task DropBox_ShowsOnGroundDescription()
    {
        var target = GetTarget();
        StartHere<StorageEast>();
        await target.GetResponse("take box");
        await target.GetResponse("drop box");

        var response = await target.GetResponse("look");

        response.Should().Contain("There is a cardboard box here");
        response.Should().Contain("The cardboard box contains:");
    }

    [Test]
    public async Task EmptyBox_NoContentsList()
    {
        var target = GetTarget();
        StartHere<StorageEast>();

        // Take all items from box
        await target.GetResponse("take bedistor");
        await target.GetResponse("take k-series");
        await target.GetResponse("take b-series");
        await target.GetResponse("take fromitz board");

        var response = await target.GetResponse("examine box");

        response.Should().Contain("empty");
    }

    [Test]
    public async Task TakeBox_ThenTakeItemFromIt()
    {
        var target = GetTarget();
        StartHere<StorageEast>();
        await target.GetResponse("take box");

        var response = await target.GetResponse("take bedistor");

        response.Should().Contain("Taken");
        target.Context.HasItem<GoodBedistor>().Should().BeTrue();
    }

    [Test]
    public void BoxIsTransparent_CanSeeInside()
    {
        GetTarget();
        StartHere<StorageEast>();
        var box = GetItem<CardboardBox>();

        box.IsTransparent.Should().BeTrue();
    }

    [Test]
    public async Task InventoryWithBox_ShowsBoxAndContents()
    {
        var target = GetTarget();
        StartHere<StorageEast>();
        await target.GetResponse("take box");

        var response = await target.GetResponse("inventory");

        response.Should().Contain("You are carrying:");
        response.Should().Contain("A cardboard box");
        response.Should().Contain("The cardboard box contains:");
        response.Should().Contain("A good ninety-ohm bedistor");
        response.Should().Contain("A K-series megafuse");
        response.Should().Contain("A B-series megafuse");
        response.Should().Contain("A cracked seventeen-centimeter fromitz board");
    }

    [Test]
    public async Task TakeFromitzBoard_CanBeExamined()
    {
        var target = GetTarget();
        StartHere<StorageEast>();
        await target.GetResponse("take fromitz board");

        var response = await target.GetResponse("examine fromitz board");

        response.Should().Contain("twisted maze of silicon circuits");
        response.Should().Contain("seventeen centimeters");
        response.Should().Contain("looks as though it's been dropped");
    }
}
