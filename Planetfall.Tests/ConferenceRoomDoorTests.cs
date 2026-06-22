using FluentAssertions;
using GameEngine;
using Planetfall.Item.Kalamontee;
using Planetfall.Location.Kalamontee;

namespace Planetfall.Tests;

/// <summary>
/// issue #262 / #266: the conference room door is a plain room-to-room door. "enter door" routes to
/// Direction.In, so the door passage (ConferenceRoom -> RecArea, and back) is exposed under "in".
/// </summary>
public class ConferenceRoomDoorTests : EngineTestsBase
{
    [Test]
    public async Task EnterDoor_FromConferenceRoom_Blocked_WhenClosed()
    {
        var target = GetTarget();
        var room = Repository.GetLocation<ConferenceRoom>();
        room.Init(); // place the conference room door
        Repository.GetItem<ConferenceRoomDoor>().IsOpen = false;
        target.Context.CurrentLocation = room;

        var response = await target.GetResponse("enter door");

        response.Should().Contain("The door is closed.");
        target.Context.CurrentLocation.Should().BeOfType<ConferenceRoom>();
    }

    [Test]
    public async Task EnterDoor_FromConferenceRoom_GoesToRecArea_WhenOpen()
    {
        var target = GetTarget();
        var room = Repository.GetLocation<ConferenceRoom>();
        room.Init();
        Repository.GetItem<ConferenceRoomDoor>().IsOpen = true;
        target.Context.CurrentLocation = room;

        await target.GetResponse("enter door");

        target.Context.CurrentLocation.Should().BeOfType<RecArea>();
    }

    [Test]
    public async Task EnterDoor_FromRecArea_GoesToConferenceRoom_WhenOpen()
    {
        var target = GetTarget();
        var room = Repository.GetLocation<RecArea>();
        room.Init(); // place the conference room door on this side too
        Repository.GetItem<ConferenceRoomDoor>().IsOpen = true;
        target.Context.CurrentLocation = room;

        await target.GetResponse("enter door");

        target.Context.CurrentLocation.Should().BeOfType<ConferenceRoom>();
    }
}
