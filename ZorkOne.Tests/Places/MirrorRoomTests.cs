using FluentAssertions;
using GameEngine;
using ZorkOne.Item;
using ZorkOne.Location;

namespace ZorkOne.Tests.Places;

[TestFixture]
public class MirrorRoomTests : EngineTestsBase
{
    [Test]
    public async Task TouchMirror_NotBroken_Teleports()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<MirrorRoomNorth>();

        var response = await target.GetResponse("touch mirror");
        Console.WriteLine(response);

        response.Should().Contain("rumble from deep within the earth");
        target.Context.CurrentLocation.Should().BeOfType<MirrorRoomSouth>();
    }

    [Test]
    public async Task BreakMirror_GivesBadLuckMessage_AndSetsBrokenFlag()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<MirrorRoomNorth>();

        var response = await target.GetResponse("kick mirror");
        Console.WriteLine(response);

        response.Should().Contain("broken the mirror");
        response.Should().Contain("seven years");
        Repository.GetItem<Mirror>().IsBroken.Should().BeTrue();
    }

    [Test]
    public async Task ThrowMirror_BreaksIt()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<MirrorRoomSouth>();

        var response = await target.GetResponse("throw mirror");
        Console.WriteLine(response);

        response.Should().Contain("broken the mirror");
        Repository.GetItem<Mirror>().IsBroken.Should().BeTrue();
    }

    [Test]
    public async Task TouchMirror_AfterBroken_DoesNotTeleport()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<MirrorRoomNorth>();

        await target.GetResponse("kick mirror");
        var response = await target.GetResponse("touch mirror");
        Console.WriteLine(response);

        response.Should().NotContain("rumble from deep within the earth");
        response.Should().Contain("broken");
        target.Context.CurrentLocation.Should().BeOfType<MirrorRoomNorth>();
    }

    [Test]
    public async Task BrokenInOneRoom_DisablesTeleportInOtherRoom()
    {
        var target = GetTarget();

        // Break the mirror in the north room.
        target.Context.CurrentLocation = Repository.GetLocation<MirrorRoomNorth>();
        await target.GetResponse("kick mirror");

        // The single shared mirror is now broken; the south room's teleport is disabled too.
        target.Context.CurrentLocation = Repository.GetLocation<MirrorRoomSouth>();
        var response = await target.GetResponse("touch mirror");
        Console.WriteLine(response);

        response.Should().NotContain("rumble from deep within the earth");
        target.Context.CurrentLocation.Should().BeOfType<MirrorRoomSouth>();
    }

    [Test]
    public async Task ExamineMirror_AfterBroken_ShowsBrokenDescription()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<MirrorRoomNorth>();

        await target.GetResponse("kick mirror");
        var response = await target.GetResponse("examine mirror");
        Console.WriteLine(response);

        response.Should().Contain("broken");
        response.Should().NotContain("ugly person");
    }

    [Test]
    public async Task RoomDescription_AfterBroken_NotesDestruction()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<MirrorRoomNorth>();

        await target.GetResponse("kick mirror");
        var response = await target.GetResponse("look");
        Console.WriteLine(response);

        response.Should().Contain("shattered");
    }
}
