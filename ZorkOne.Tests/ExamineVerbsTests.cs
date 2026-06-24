using FluentAssertions;
using GameEngine;
using ZorkOne.Item;
using ZorkOne.Location;
using ZorkOne.Location.ForestLocation;

namespace ZorkOne.Tests;

/// <summary>
///     Issue #207: the scattered inline "examine / look at" verb arrays were centralized into
///     <see cref="Model.Verbs.ExamineVerbs" />. These tests pin the widening: a synonym like
///     "inspect" — which several of the old inline arrays did not include — must now route to the
///     same custom responses as "examine".
/// </summary>
public class ExamineVerbsTests : EngineTestsBase
{
    [Test]
    public async Task Inspect_Door_AtWestOfHouse_RoutesLikeExamine()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<WestOfHouse>();

        var response = await target.GetResponse("inspect door");

        response.Should().Contain("The door is closed.");
    }

    [Test]
    public async Task Inspect_House_AtWestOfHouse_RoutesLikeExamine()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<WestOfHouse>();

        var response = await target.GetResponse("inspect house");

        response.Should().Contain("beautiful colonial house");
    }

    [Test]
    public async Task Inspect_Tree_AtForestPath_RoutesLikeExamine()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<ForestPath>();

        var response = await target.GetResponse("inspect tree");

        response.Should().Contain("nothing special about the tree");
    }

    [Test]
    public async Task Inspect_Mirror_AtMirrorRoom_RoutesLikeExamine()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<MirrorRoomNorth>();
        // The mirror room is underground; give the player a lit lamp so the room is visible.
        target.Context.Take(Repository.GetItem<Lantern>());
        Repository.GetItem<Lantern>().IsOn = true;

        var response = await target.GetResponse("inspect mirror");

        response.Should().Contain("ugly person staring back at you");
    }

    /// <summary>
    ///     Issue #312: "look at &lt;single-word noun&gt;" was collapsing to the bare-room LOOK command,
    ///     re-describing the room instead of examining the named object. "look at" must route to the
    ///     same examine-with-noun path as "examine" / "inspect", including the single-word-noun case.
    /// </summary>
    [Test]
    public async Task LookAt_Mailbox_AtWestOfHouse_RoutesLikeExamine()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<WestOfHouse>();

        var response = await target.GetResponse("look at mailbox");

        response.Should().Contain("The small mailbox is closed.");
        response.Should().NotContain("open field");
    }

    [Test]
    public async Task LookAt_Door_AtWestOfHouse_RoutesLikeExamine()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<WestOfHouse>();

        var response = await target.GetResponse("look at door");

        response.Should().Contain("The door is closed.");
    }

    [Test]
    public async Task LookAt_House_AtWestOfHouse_RoutesLikeExamine()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<WestOfHouse>();

        var response = await target.GetResponse("look at house");

        response.Should().Contain("beautiful colonial house");
    }
}
