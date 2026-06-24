using FluentAssertions;
using GameEngine;
using ZorkOne.Item;
using ZorkOne.Location.CoalMineLocation;

namespace ZorkOne.Tests.Places;

public class TimberRoomTests : EngineTestsBase
{
    [Test]
    public async Task WestExit_WithItems_ReturnsNarrowPassageMessage()
    {
        var target = GetTarget();
        Repository.GetLocation<TimberRoom>().IsNoLongerDark = true;
        target.Context.CurrentLocation = Repository.GetLocation<TimberRoom>();
        target.Context.Take(Repository.GetItem<Lantern>());

        var response = await target.GetResponse("W");
        Console.WriteLine(response);

        response.Should().NotBeNullOrEmpty();
        response.Should().Contain("squeeze");
    }

    [Test]
    public async Task WestExit_WithNoItems_EntersDraftyRoom()
    {
        var target = GetTarget();
        Repository.GetLocation<TimberRoom>().IsNoLongerDark = true;
        Repository.GetLocation<DraftyRoom>().IsNoLongerDark = true;
        target.Context.CurrentLocation = Repository.GetLocation<TimberRoom>();

        var response = await target.GetResponse("W");
        Console.WriteLine(response);

        target.Context.CurrentLocation.Should().BeOfType<DraftyRoom>();
    }

    [Test]
    public async Task DraftyRoom_EastExit_WithItems_ReturnsNarrowPassageMessage()
    {
        var target = GetTarget();
        Repository.GetLocation<DraftyRoom>().IsNoLongerDark = true;
        target.Context.CurrentLocation = Repository.GetLocation<DraftyRoom>();
        target.Context.Take(Repository.GetItem<Lantern>());

        var response = await target.GetResponse("E");
        Console.WriteLine(response);

        response.Should().NotBeNullOrEmpty();
        response.Should().Contain("squeeze");
    }

    [Test]
    public async Task DraftyRoom_EastExit_WithNoItems_EntersTimberRoom()
    {
        var target = GetTarget();
        Repository.GetLocation<DraftyRoom>().IsNoLongerDark = true;
        Repository.GetLocation<TimberRoom>().IsNoLongerDark = true;
        target.Context.CurrentLocation = Repository.GetLocation<DraftyRoom>();

        var response = await target.GetResponse("E");
        Console.WriteLine(response);

        target.Context.CurrentLocation.Should().BeOfType<TimberRoom>();
    }
}
