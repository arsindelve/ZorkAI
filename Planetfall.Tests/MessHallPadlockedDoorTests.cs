using FluentAssertions;
using GameEngine;
using Planetfall.Item.Kalamontee;
using Planetfall.Item.Kalamontee.Admin;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Planetfall.Location.Kalamontee;

namespace Planetfall.Tests;

public class MessHallPadlockedDoorTests : EngineTestsBase
{
    [Test]
    public async Task CannotGoNorth_DoorClosed()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<MessCorridor>();

        var response = await target.GetResponse("N");
        response.Should().Contain("The door is closed");
    }

    [Test]
    public async Task OpenDoor_WhileLocked()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<MessCorridor>();

        var response = await target.GetResponse("open door");
        response.Should().Contain("The door cannot be opened until the padlock is removed");
    }

    [Test]
    public async Task UnlockPadlockWithKey()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<MessCorridor>();
        target.Context.ItemPlacedHere<Key>();

        var response = await target.GetResponse("unlock padlock with key");
        response.Should().Contain("springs open");
        Repository.GetItem<Padlock>().Locked.Should().BeFalse();
        Repository.GetItem<Padlock>().AttachedToDoor.Should().BeTrue();
    }

    [Test]
    public async Task UnlockDoorWithKey_UnlocksPadlock()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<MessCorridor>();
        target.Context.ItemPlacedHere<Key>();

        var response = await target.GetResponse("unlock door with key");
        response.Should().Contain("springs open");
        Repository.GetItem<Padlock>().Locked.Should().BeFalse();
        Repository.GetItem<Padlock>().AttachedToDoor.Should().BeTrue();
    }

    [Test]
    public async Task OpenDoor_WhileAttached()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<MessCorridor>();
        target.Context.ItemPlacedHere<Key>();

        await target.GetResponse("unlock padlock with key");
        Repository.GetItem<Padlock>().Locked.Should().BeFalse();
        var response = await target.GetResponse("open door");
        response.Should().Contain("The door cannot be opened until the padlock is removed");
    }

    [Test]
    public async Task OpenDoor_RemoveLock()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<MessCorridor>();
        target.Context.ItemPlacedHere<Key>();

        await target.GetResponse("unlock padlock with key");
        Repository.GetItem<Padlock>().Locked.Should().BeFalse();

        await target.GetResponse("remove padlock");
        Repository.GetItem<Padlock>().AttachedToDoor.Should().BeFalse();

        var response = await target.GetResponse("open door");
        response.Should().Contain("Opened");
        Repository.GetItem<MessDoor>().IsOpen.Should().BeTrue();
    }

    [Test]
    public async Task OpenDoor_TakeLock()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<MessCorridor>();
        target.Context.ItemPlacedHere<Key>();

        await target.GetResponse("unlock padlock with key");
        Repository.GetItem<Padlock>().Locked.Should().BeFalse();

        await target.GetResponse("take padlock");
        Repository.GetItem<Padlock>().AttachedToDoor.Should().BeFalse();

        var response = await target.GetResponse("open door");
        response.Should().Contain("Opened");
        Repository.GetItem<MessDoor>().IsOpen.Should().BeTrue();
    }

    [Test]
    public async Task CanGoNorth_DoorOpen()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<MessCorridor>();
        Repository.GetItem<MessDoor>().IsOpen = true;

        var response = await target.GetResponse("N");
        response.Should().Contain("Storage West");
    }

    [Test]
    public async Task LookUnderTable_ShowsJokeMessage()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<MessHall>();

        var response = await target.GetResponse("look under table");

        response.Should().Contain("Wow!!! Under the table are three keys");
        response.Should().Contain("a sack of food");
        response.Should().Contain("a reactor elevator access pass");
        response.Should().Contain("just kidding");
        response.Should().Contain("Actually, there's nothing there.");
    }

    [Test]
    public async Task UnlockPadlock_FloydCommentsWhenPresent()
    {
        var target = GetTarget();
        var messCorridor = GetLocation<MessCorridor>();
        target.Context.CurrentLocation = messCorridor;
        target.Context.ItemPlacedHere<Key>();

        // Set up Floyd as present and active
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        floyd.CurrentLocation = messCorridor;
        messCorridor.ItemPlacedHere(floyd);

        await target.GetResponse("unlock padlock with key");

        // Verify Floyd's pending comment was set
        var pfContext = (PlanetfallContext)target.Context;
        pfContext.PendingFloydActionCommentPrompt.Should().NotBeNull();
        pfContext.PendingFloydActionCommentPrompt.Should().Contain("padlock");
    }

    [Test]
    public async Task UnlockPadlock_FloydDoesNotComment_WhenNotPresent()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<MessCorridor>();
        target.Context.ItemPlacedHere<Key>();

        // Floyd is NOT in the room
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        floyd.CurrentLocation = GetLocation<MessHall>(); // Different location

        await target.GetResponse("unlock padlock with key");

        // Verify Floyd's pending comment was NOT set
        var pfContext = (PlanetfallContext)target.Context;
        pfContext.PendingFloydActionCommentPrompt.Should().BeNull();
    }
}