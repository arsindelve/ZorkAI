namespace UnitTests.SingleNounProcessors;

public class OpenProcessorTest : EngineTestsBase
{
    [Test]
    public async Task OpenProcessor_OpenSomething()
    {
        var target = GetTarget();

        // Act
        var result = await target.GetResponse("open mailbox");

        Repository.GetItem<Mailbox>().IsOpen.Should().BeTrue();
        result.Should().Contain("Opening");
    }

    [Test]
    public async Task OpenProcessor_DifferentFirstTime()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<Kitchen>();
        Repository.GetItem<BrownSack>().HasEverBeenOpened.Should().BeFalse();

        // Act
        var result = await target.GetResponse("open sack");

        Repository.GetItem<BrownSack>().IsOpen.Should().BeTrue();
        Repository.GetItem<BrownSack>().HasEverBeenOpened.Should().BeTrue();
        result.Should().Contain("Opening the brown sack reveals a lunch, and a clove of garlic");
    }

    [Test]
    public async Task OpenProcessor_DifferentSecondTime()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<Kitchen>();
        Repository.GetItem<BrownSack>().HasEverBeenOpened.Should().BeFalse();

        // Act
        await target.GetResponse("open sack");
        await target.GetResponse("close sack");
        var result = await target.GetResponse("open sack");

        Repository.GetItem<BrownSack>().IsOpen.Should().BeTrue();
        Repository.GetItem<BrownSack>().HasEverBeenOpened.Should().BeTrue();
        result.Should().Contain("Opened");
    }

    [Test]
    public async Task OpenProcessor_OpenSomething_AlreadyOpen()
    {
        var target = GetTarget();

        // Act
        await target.GetResponse("open mailbox");
        var result = await target.GetResponse("open mailbox");

        Repository.GetItem<Mailbox>().IsOpen.Should().BeTrue();
        result.Should().Contain("already");
    }

    [Test]
    public async Task OpenProcessor_CloseSomething()
    {
        var target = GetTarget();

        // Act
        await target.GetResponse("open mailbox");
        var result = await target.GetResponse("close mailbox");

        Repository.GetItem<Mailbox>().IsOpen.Should().BeFalse();
        result.Should().Contain("Closed");
    }

    [Test]
    public async Task OpenProcessor_CloseSomething_AlreadyClosed()
    {
        var target = GetTarget();

        // Act
        var result = await target.GetResponse("close mailbox");

        // Assert
        Repository.GetItem<Mailbox>().IsOpen.Should().BeFalse();
        result.Should().Contain("already");
    }
}