using FluentAssertions;
using GameEngine;
using ZorkOne.Item;
using ZorkOne.Location;
using ZorkOne.Location.MazeLocation;

namespace ZorkOne.Tests;

/// <summary>
/// issue #262 / #266: the grating is a fixed openable that gates the passage up from the Grating Room
/// to the Clearing. "enter grate" routes to Direction.In, so the passage is exposed under "in" too.
/// </summary>
public class GratingDoorTests : EngineTestsBase
{
    [Test]
    public async Task EnterGrate_FromGratingRoom_Blocked_WhenClosed()
    {
        var target = GetTarget();
        var room = Repository.GetLocation<GratingRoom>();
        room.Init();
        // A lit lamp so the dark Grating Room is visible.
        var lamp = Repository.GetItem<Lantern>();
        lamp.IsOn = true;
        target.Context.Take(lamp);
        target.Context.CurrentLocation = room;

        var response = await target.GetResponse("enter grate");

        response.Should().Contain("The grating is closed.");
        target.Context.CurrentLocation.Should().BeOfType<GratingRoom>();
    }

    [Test]
    public async Task EnterGrate_FromGratingRoom_GoesUpToClearing_WhenOpen()
    {
        var target = GetTarget();
        var room = Repository.GetLocation<GratingRoom>();
        room.Init();
        Repository.GetItem<Grating>().IsOpen = true;
        var lamp = Repository.GetItem<Lantern>();
        lamp.IsOn = true;
        target.Context.Take(lamp);
        target.Context.CurrentLocation = room;

        await target.GetResponse("enter grate");

        target.Context.CurrentLocation.Should().BeOfType<Clearing>();
    }
}
