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
        Client.Setup(s => s.GenerateNarration(It.IsAny<TakeAllNothingHere>(), It.IsAny<string>())).ReturnsAsync("nothing here buddy");
        target.Context.CurrentLocation = Repository.GetLocation<Cellar>();
        var response = await target.GetResponse("take all");
        response.Should().Contain("nothing here buddy");
    }

    [Test]
    public async Task TakeAllNothingHereAtAll()
    {
        var target = GetTarget();
        Client.Setup(s => s.GenerateNarration(It.IsAny<TakeAllNothingHere>(), It.IsAny<string>())).ReturnsAsync("nothing here buddy");
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
        Client.Setup(s => s.GenerateNarration(It.IsAny<DropAllNothingHere>(), It.IsAny<string>())).ReturnsAsync("nothing here buddy");
        target.Context.CurrentLocation = Repository.GetLocation<MazeFour>();
        var response = await target.GetResponse("drop all");
        response.Should().Contain("nothing here buddy");
    }

    [Test]
    public async Task TakeMultipleItemsWithOneInvalid()
    {
        var target = GetTarget();
        // Mock the AI parser to return both a valid item (leaflet) and an invalid item (dragon)
        TakeAndDropParser.Setup(s => s.GetListOfItemsToTake(
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(new[] { "leaflet", "dragon" });

        // Mock the LLM response for the invalid item
        Client.Setup(s => s.GenerateNarration(It.IsAny<TakeSomethingThatIsNotPortable>(), It.IsAny<string>()))
            .ReturnsAsync("I don't see any dragon here!");

        Repository.GetItem<Mailbox>().IsOpen = true;
        var response = await target.GetResponse("take leaflet and dragon");

        // Should take the leaflet
        response.Should().Contain("leaflet: Taken");

        // Should provide LLM-generated feedback about the dragon not being present
        response.Should().Contain("dragon: I don't see any dragon here!");
    }

    [Test]
    public async Task DropMultipleItemsWithOneInvalid()
    {
        var target = GetTarget();

        // First, take the leaflet
        Repository.GetItem<Mailbox>().IsOpen = true;
        await target.GetResponse("take leaflet");

        // Mock the AI parser to return both a valid item (leaflet) and an invalid item (dragon)
        TakeAndDropParser.Setup(s => s.GetListOfItemsToDrop(
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(new[] { "leaflet", "dragon" });

        // Mock the LLM response for the invalid item
        Client.Setup(s => s.GenerateNarration(It.IsAny<DropSomethingTheyDoNotHave>(), It.IsAny<string>()))
            .ReturnsAsync("You don't have that mythical creature!");

        var response = await target.GetResponse("drop leaflet and dragon");

        // Should drop the leaflet
        response.Should().Contain("leaflet: Dropped");

        // Should provide LLM-generated feedback about not having the dragon
        response.Should().Contain("dragon: You don't have that mythical creature!");
    }
}