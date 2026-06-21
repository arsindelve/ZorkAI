using FluentAssertions;
using GameEngine;
using Model.AIGeneration.Requests;
using Moq;
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
    public async Task ThrowMirror_DoesNotBreakIt_AndFallsBackToAi()
    {
        // You can't "throw" a wall-sized mirror with no second noun; throwing something AT it is a
        // separate (multi-noun) interaction. So a bare "throw mirror" must not break it — it falls
        // through to the normal AI "that verb has no effect" narration.
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<MirrorRoomSouth>();
        Client.Setup(c => c.GenerateNarration(It.IsAny<VerbHasNoEffectOperationRequest>(), It.IsAny<string>()))
            .ReturnsAsync("Throwing the mirror accomplishes nothing. ");

        var response = await target.GetResponse("throw mirror");
        Console.WriteLine(response);

        response.Should().Contain("Throwing the mirror accomplishes nothing.");
        response.Should().NotContain("broken the mirror");
        Repository.GetItem<Mirror>().IsBroken.Should().BeFalse();
    }

    [Test]
    public async Task ThrowObjectYouDoNotHaveAtMirror_DoesNotBreakIt()
    {
        // You can only throw what you're holding. The player is not carrying the sword here.
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<MirrorRoomNorth>();

        var response = await target.GetResponse("throw sword at mirror");
        Console.WriteLine(response);

        response.Should().Contain("don't have");
        Repository.GetItem<Mirror>().IsBroken.Should().BeFalse();
    }

    [Test]
    public async Task ThrowObjectAtAlreadyBrokenMirror_DropsItOnFloor()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<MirrorRoomNorth>();
        Repository.GetItem<Mirror>().IsBroken = true;
        target.Context.ItemPlacedHere(Repository.GetItem<Sword>());

        var response = await target.GetResponse("throw sword at mirror");
        Console.WriteLine(response);

        response.Should().Contain("enough damage");
        // The item still ends up on the floor.
        target.Context.HasItem<Sword>().Should().BeFalse();
        Repository.GetLocation<MirrorRoomNorth>().HasItem<Sword>().Should().BeTrue();
    }

    [Test]
    public async Task ThrowHardObjectAtMirror_BreaksIt_AndDropsItOnFloor()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<MirrorRoomNorth>();
        target.Context.ItemPlacedHere(Repository.GetItem<Sword>());

        var response = await target.GetResponse("throw sword at mirror");
        Console.WriteLine(response);

        response.Should().Contain("seven years");
        Repository.GetItem<Mirror>().IsBroken.Should().BeTrue();
        // The thrown item leaves your hands and lands on the floor.
        target.Context.HasItem<Sword>().Should().BeFalse();
        Repository.GetLocation<MirrorRoomNorth>().HasItem<Sword>().Should().BeTrue();
    }

    [Test]
    public void SoftItems_AreFlaggedSoft_AndHardItemsAreNot()
    {
        // Locks the soft-item audit and the IsSoft default. The mirror reads IItem.IsSoft rather than
        // keeping its own list of item types, so this is what decides bounce-vs-shatter.
        GetTarget(); // resets the repository

        Repository.GetItem<Garlic>().IsSoft.Should().BeTrue();
        Repository.GetItem<Lunch>().IsSoft.Should().BeTrue();
        Repository.GetItem<BrownSack>().IsSoft.Should().BeTrue();
        Repository.GetItem<Leaflet>().IsSoft.Should().BeTrue();
        Repository.GetItem<Map>().IsSoft.Should().BeTrue();
        Repository.GetItem<Rope>().IsSoft.Should().BeTrue();

        Repository.GetItem<Sword>().IsSoft.Should().BeFalse();
        Repository.GetItem<Diamond>().IsSoft.Should().BeFalse();
        Repository.GetItem<Lantern>().IsSoft.Should().BeFalse();
    }

    [Test]
    public async Task ThrowSoftObjectAtMirror_BouncesOff_AndDropsItOnFloor()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<MirrorRoomNorth>();
        target.Context.ItemPlacedHere(Repository.GetItem<Garlic>());

        var response = await target.GetResponse("throw garlic at mirror");
        Console.WriteLine(response);

        response.Should().Contain("bounces");
        Repository.GetItem<Mirror>().IsBroken.Should().BeFalse();
        target.Context.HasItem<Garlic>().Should().BeFalse();
        Repository.GetLocation<MirrorRoomNorth>().HasItem<Garlic>().Should().BeTrue();
    }

    [Test]
    public async Task AttackVerb_BreaksMirror()
    {
        // "punch" comes from the shared Verbs.KillVerbs set, proving the consolidated verb list is wired.
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<MirrorRoomNorth>();

        var response = await target.GetResponse("punch mirror");
        Console.WriteLine(response);

        response.Should().Contain("broken the mirror");
        Repository.GetItem<Mirror>().IsBroken.Should().BeTrue();
    }

    [Test]
    public async Task BreakMirror_WhenAlreadyBroken_GivesEnoughDamageMessage()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<MirrorRoomNorth>();

        await target.GetResponse("kick mirror");
        var response = await target.GetResponse("kick mirror");
        Console.WriteLine(response);

        response.Should().Contain("Haven't you done enough damage already?");
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
