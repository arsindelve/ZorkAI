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
