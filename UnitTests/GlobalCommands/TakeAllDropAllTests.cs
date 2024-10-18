using GameEngine;

namespace UnitTests.GlobalCommands;

public class TakeAllDropAllTests : EngineTestsBase
{
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
}