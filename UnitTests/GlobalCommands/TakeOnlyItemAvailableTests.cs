namespace UnitTests.GlobalCommands;

public class TakeOnlyItemAvailableTests : EngineTestsBase
{
    [Test]
    public async Task Take_NoItemsToTake_SingleItemIsRooted()
    {
        var engine = GetTarget(new IntentParser());

        // Act
        var response = await engine.GetResponse("take");

        // Assert
        response.Should().Contain("What do you");
    }

    [Test]
    public async Task Take_NoItemsToTake()
    {
        var engine = GetTarget(new IntentParser());
        engine.Context.CurrentLocation = Repository.GetLocation<RockyLedge>();

        // Act
        var response = await engine.GetResponse("take");

        // Assert
        response.Should().Contain("What do you");
    }

    [Test]
    public async Task Take_MultipleItemsToTake()
    {
        var engine = GetTarget(new IntentParser());
        engine.Context.CurrentLocation = Repository.GetLocation<Attic>();

        // Act
        var response = await engine.GetResponse("take");

        // Assert
        response.Should().Contain("What do you");
    }

    [Test]
    public async Task Take_MultipleItemsToTake_SecondPass()
    {
        var engine = GetTarget(new IntentParser());
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
        var engine = GetTarget(new IntentParser());

        // Act
        await engine.GetResponse("take");
        var response = await engine.GetResponse("mailbox");

        // Assert
        response.Should().Contain("anchored");
    }

    [Test]
    public async Task Take_SingleItemToTake()
    {
        var engine = GetTarget(new IntentParser());
        // There is a single item to be taken in this location 
        engine.Context.CurrentLocation = Repository.GetLocation<Gallery>();

        // Act
        var response = await engine.GetResponse("take");

        // Assert
        response.Should().Contain("Taken");
        response.Should().Contain("(painting)");
    }
}