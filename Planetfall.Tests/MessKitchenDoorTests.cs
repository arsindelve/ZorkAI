using FluentAssertions;
using GameEngine;
using Planetfall.Item.Kalamontee;
using Planetfall.Item.Kalamontee.Admin;
using Planetfall.Location.Kalamontee;

namespace Planetfall.Tests;

/// <summary>
/// issue #262 / #266: the mess door (Mess Corridor -> Storage West) and the kitchen door (Mess Hall
/// -> Kitchen) are plain room-to-room doors. "enter door" routes to Direction.In, so each door's
/// passage is exposed under "in". The kitchen door's card/auto-close mechanic is unaffected — "enter
/// door" only walks through when the door is already open.
/// </summary>
public class MessKitchenDoorTests : EngineTestsBase
{
    [Test]
    public async Task EnterDoor_FromMessCorridor_Blocked_WhenMessDoorClosed()
    {
        var target = GetTarget();
        var room = Repository.GetLocation<MessCorridor>();
        room.Init();
        Repository.GetItem<MessDoor>().IsOpen = false;
        target.Context.CurrentLocation = room;

        var response = await target.GetResponse("enter door");

        response.Should().Contain("The door is closed.");
        target.Context.CurrentLocation.Should().BeOfType<MessCorridor>();
    }

    [Test]
    public async Task EnterDoor_FromMessCorridor_GoesToStorageWest_WhenMessDoorOpen()
    {
        var target = GetTarget();
        var room = Repository.GetLocation<MessCorridor>();
        room.Init();
        Repository.GetItem<MessDoor>().IsOpen = true;
        target.Context.CurrentLocation = room;

        await target.GetResponse("enter door");

        target.Context.CurrentLocation.Should().BeOfType<StorageWest>();
    }

    [Test]
    public async Task EnterDoor_FromMessHall_GoesToKitchen_WhenKitchenDoorOpen()
    {
        var target = GetTarget();
        var room = Repository.GetLocation<MessHall>();
        room.Init();
        Repository.GetItem<KitchenDoor>().IsOpen = true;
        target.Context.CurrentLocation = room;

        await target.GetResponse("enter door");

        target.Context.CurrentLocation.Should().BeOfType<Kitchen>();
    }
}
