using GameEngine;
using Model.AIGeneration.Requests;
using ZorkOne.Location.MazeLocation;

namespace UnitTests.GlobalCommands;

public class TakeAllDropAllTests : EngineTestsBase
{
    [Test]
    public async Task TakeAllNothingHereThatCanBeTaken()
    {
        var target = GetTarget();
        Client.Setup(s => s.CompleteChat(It.IsAny<TakeAllNothingHere>())).ReturnsAsync("nothing here buddy");
        target.Context.CurrentLocation = Repository.GetLocation<Cellar>();
        var response = await target.GetResponse("take all");
        response.Should().Contain("nothing here buddy");
    }
    
    [Test]
    public async Task TakeAllNothingHereAtAll()
    {
        var target = GetTarget();
        Client.Setup(s => s.CompleteChat(It.IsAny<TakeAllNothingHere>())).ReturnsAsync("nothing here buddy");
        target.Context.CurrentLocation = Repository.GetLocation<MazeFour>();
        var response = await target.GetResponse("take all");
        response.Should().Contain("nothing here buddy");
    }

    [Test]
    public async Task TakeTheMailbox()
    {
        var target = GetTarget();
        var response = await target.GetResponse("take all");
        response.Should().Contain("anchored");
        response.Should().NotContain("leaflet: Taken");
    }

    [Test]
    public async Task TakeTheMailboxAndTheLeaflet()
    {
        var target = GetTarget();
        Repository.GetItem<Mailbox>().IsOpen = true;
        var response = await target.GetResponse("take all");
        response.Should().Contain("anchored");
        response.Should().Contain("leaflet: Taken");
    }

    [Test]
    public async Task TakeTheMailboxAndTheLeafletThenDropIt()
    {
        var target = GetTarget();
        Repository.GetItem<Mailbox>().IsOpen = true;
        await target.GetResponse("take all");
        var response = await target.GetResponse("drop all");
        response.Should().Contain("leaflet: Dropped");
    }
    
    [Test]
    public async Task DropAllNothingInInventory()
    {
        var target = GetTarget();
        Client.Setup(s => s.CompleteChat(It.IsAny<DropAllNothingHere>())).ReturnsAsync("nothing here buddy");
        target.Context.CurrentLocation = Repository.GetLocation<MazeFour>();
        var response = await target.GetResponse("drop all");
        response.Should().Contain("nothing here buddy");
    }
}