using GameEngine;
using Model.AIGeneration.Requests;
using ZorkOne.Location.MazeLocation;

namespace UnitTests.GlobalCommands;

/// <summary>
/// The take/drop global commands in all three of their forms: a bare "take" that must resolve or
/// prompt, "take all"/"drop all" over a whole room, and a named take whose noun comes back from the
/// AI list parser. Merged from the former TakeOnlyItemAvailableTests / TakeAllDropAllTests /
/// TakeSwordRepro (the last of which was also misnamed — a "Repro" is a test like any other).
/// </summary>
public class TakeCommandTests : EngineTestsBase
{
    [Test]
    public async Task Take_NoItemsToTake_SingleItemIsRooted()
    {
        var engine = GetTargetWithRealParser();

        // Act
        var response = await engine.GetResponse("take");

        // Assert
        response.Should().Contain("What do you");
    }

    [Test]
    public async Task Take_NoItemsToTake()
    {
        var engine = GetTargetWithRealParser();
        engine.Context.CurrentLocation = Repository.GetLocation<RockyLedge>();

        // Act
        var response = await engine.GetResponse("take");

        // Assert
        response.Should().Contain("What do you");
    }

    [Test]
    public async Task Take_MultipleItemsToTake()
    {
        var engine = GetTargetWithRealParser();
        engine.Context.CurrentLocation = Repository.GetLocation<Attic>();

        // Act
        var response = await engine.GetResponse("take");

        // Assert
        response.Should().Contain("What do you");
    }

    [Test]
    public async Task Take_MultipleItemsToTake_SecondPass()
    {
        var engine = GetTargetWithRealParser();
        Repository.GetItem<Lantern>().IsOn = true;
        engine.Context.Take(Repository.GetItem<Lantern>());
        engine.Context.CurrentLocation = Repository.GetLocation<Attic>();
        engine.Context.CurrentLocation = Repository.GetLocation<Attic>();

        // Act
        await engine.GetResponse("take");
        var response = await engine.GetResponse("rope");

        // Assert
        response.Should().Contain("Taken");
        engine.Context.HasItem<Rope>().Should().BeTrue();
    }

    [Test]
    public async Task Take_NoItemsToTake_ReplyWithCustomString()
    {
        var engine = GetTargetWithRealParser();

        // Act
        await engine.GetResponse("take");
        var response = await engine.GetResponse("mailbox");

        // Assert
        response.Should().Contain("anchored");
    }

    [Test]
    public async Task Take_SingleItemToTake()
    {
        var engine = GetTargetWithRealParser();
        // There is a single item to be taken in this location
        engine.Context.CurrentLocation = Repository.GetLocation<Gallery>();

        // Act
        var response = await engine.GetResponse("take");

        // Assert
        response.Should().Contain("Taken");
        response.Should().Contain("(painting)");
    }

    [Test]
    public async Task TakeSword_FromLivingRoom_ShouldSucceed()
    {
        var target = GetTarget();
        // Force AI parser to return "sword" plain
        TakeAndDropParser.Setup(s => s.GetListOfItemsToTake(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new[] { "sword" });

        target.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();
        var response = await target.GetResponse("take sword");

        response.Should().Contain("Taken");
    }

    [Test]
    public async Task TakeSword_AIReturnsElvishSword_ShouldSucceed()
    {
        var target = GetTarget();
        // Force AI parser to return "elvish sword" — what may happen in production
        TakeAndDropParser.Setup(s => s.GetListOfItemsToTake(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new[] { "elvish sword" });

        target.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();
        var response = await target.GetResponse("take sword");

        response.Should().Contain("Taken");
    }

    [Test]
    public async Task TakeAllNothingHereThatCanBeTaken()
    {
        var target = GetTarget();
        Client.Setup(s => s.GenerateNarration(It.IsAny<TakeAllNothingHere>(), It.IsAny<string>())).ReturnsAsync("nothing here buddy");
        // Cellar is a DarkLocation; bring a lit lantern so this exercises the empty-room narration
        // rather than the (correct, see issue #342) darkness guard added to TakeEverythingProcessor.
        target.Context.Take(Repository.GetItem<Lantern>());
        Repository.GetItem<Lantern>().IsOn = true;
        target.Context.CurrentLocation = Repository.GetLocation<Cellar>();
        var response = await target.GetResponse("take all");
        response.Should().Contain("nothing here buddy");
    }

    [Test]
    public async Task TakeAllNothingHereAtAll()
    {
        var target = GetTarget();
        Client.Setup(s => s.GenerateNarration(It.IsAny<TakeAllNothingHere>(), It.IsAny<string>())).ReturnsAsync("nothing here buddy");
        // MazeFour is a DarkLocation; bring a lit lantern so this exercises the empty-room narration
        // rather than the (correct, see issue #342) darkness guard added to TakeEverythingProcessor.
        target.Context.Take(Repository.GetItem<Lantern>());
        Repository.GetItem<Lantern>().IsOn = true;
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
