using FluentAssertions;
using GameEngine;
using ZorkOne.Item;
using ZorkOne.Location;

namespace ZorkOne.Tests;

/// <summary>
/// issue #262: "enter trap door" is grammatically odd but players type it. The trap door gates the
/// Living Room's "down" passage (and the Cellar's "up"); exposing that passage as "in" too lets the
/// enter-a-door routing carry the player through it, just like "enter window" / "enter pod".
/// </summary>
public class EnterTrapDoorTests : EngineTestsBase
{
    [Test]
    public async Task EnterTrapDoor_FromLivingRoom_Blocked_WhenClosed()
    {
        var target = GetTarget();
        var livingRoom = Repository.GetLocation<LivingRoom>();
        livingRoom.ItemPlacedHere(Repository.GetItem<TrapDoor>()); // normally revealed by moving the rug
        target.Context.CurrentLocation = livingRoom;

        var response = await target.GetResponse("enter trap door");

        response.Should().Contain("The trap door is closed.");
        target.Context.CurrentLocation.Should().BeOfType<LivingRoom>();
    }

    [Test]
    public async Task EnterTrapDoor_FromLivingRoom_GoesDownToCellar_WhenOpen()
    {
        var target = GetTarget();
        var livingRoom = Repository.GetLocation<LivingRoom>();
        livingRoom.ItemPlacedHere(Repository.GetItem<TrapDoor>());
        Repository.GetItem<TrapDoor>().IsOpen = true;

        // Carry a lit lamp so the dark cellar doesn't complicate the move.
        var lamp = Repository.GetItem<Lantern>();
        lamp.IsOn = true;
        target.Context.Take(lamp);
        target.Context.CurrentLocation = livingRoom;

        await target.GetResponse("enter trap door");

        target.Context.CurrentLocation.Should().BeOfType<Cellar>();
    }
}
