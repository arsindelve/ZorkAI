using FluentAssertions;
using GameEngine;
using Planetfall.Item.Feinstein;
using Planetfall.Location.Feinstein;

namespace Planetfall.Tests;

public class BrigCellDoorTests : EngineTestsBase
{
    [Test]
    public async Task CannotOpenDoor_ReturnsNoWayJose()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<Brig>();

        var response = await target.GetResponse("open door");
        response.Should().Contain("No way, Jose.");
    }

    [Test]
    public async Task CannotOpenCellDoor_ReturnsNoWayJose()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<Brig>();

        var response = await target.GetResponse("open cell door");
        response.Should().Contain("No way, Jose.");
    }

    [Test]
    public void CellDoor_ExistsInBrig()
    {
        var brig = Repository.GetLocation<Brig>();
        brig.HasItem<CellDoor>().Should().BeTrue();
    }

    [Test]
    public void CellDoor_IsNotOpen()
    {
        var door = Repository.GetItem<CellDoor>();
        door.IsOpen.Should().BeFalse();
    }

    [Test]
    public async Task CannotGoSouth_DoorLocked()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<Brig>();

        var response = await target.GetResponse("S");
        response.Should().Contain("The cell door is locked");
    }
}
