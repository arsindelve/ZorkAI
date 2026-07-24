using FluentAssertions;
using GameEngine;
using ZorkOne.Location;

namespace ZorkOne.Tests.Things;

public class MailboxTests : EngineTestsBase
{
    [Test]
    public async Task Should_ListContents_When_ExaminingOpenMailbox()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<WestOfHouse>();

        await target.GetResponse("open mailbox");
        var response = await target.GetResponse("examine mailbox");

        response.Should().Contain("leaflet");
        response.Should().NotBe("It's open. ");
    }

    [Test]
    public async Task Should_ListContents_When_LookingInOpenMailbox()
    {
        // Issue #396: "look in <container>" is the canonical Infocom phrasing for inspecting an open
        // container's contents (ZIL: LOOK IN OBJECT -> V-LOOK-INSIDE). It must list the contents, not
        // be mis-parsed as an "in" movement ("You cannot go that way.") or collapse to the room LOOK.
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<WestOfHouse>();

        await target.GetResponse("open mailbox");
        var response = await target.GetResponse("look in mailbox");

        response.Should().Contain("leaflet");
        response.Should().NotContain("cannot go that way");
    }

    [Test]
    public async Task Should_SayEmpty_When_ExaminingOpenEmptyMailbox()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<WestOfHouse>();

        await target.GetResponse("open mailbox");
        await target.GetResponse("take leaflet");
        var response = await target.GetResponse("examine mailbox");

        response.Should().Contain("open");
        response.Should().Contain("empty");
        response.Should().NotContain("leaflet");
    }
}
