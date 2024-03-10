using Game.Item.ItemProcessor;
using Model;
using Model.Intent;
using Model.Item;
using OpenAI.Requests;
using ZorkOne.Item;
using ZorkOne.Location;

namespace UnitTests;

public class ProcessorTests : EngineTestsBase
{
    [Test]
    public async Task ReadProcessor()
    {
        var target = GetTarget();

        // Act
        await target.GetResponse("open mailbox");
        var result = await target.GetResponse("read leaflet");

        result.Should().Contain("low cunning");
    }

    [Test]
    public async Task ExamineProcessor()
    {
        var target = GetTarget();

        // Act
        await target.GetResponse("open mailbox");
        var result = await target.GetResponse("examine leaflet");

        result.Should().Contain("low cunning");
    }

    [Test]
    public async Task OpenProcessor_OpenSomething()
    {
        var target = GetTarget();

        // Act
        var result = await target.GetResponse("open mailbox");

        Repository.GetItem<Mailbox>().AmIOpen.Should().BeTrue();
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

        Repository.GetItem<BrownSack>().AmIOpen.Should().BeTrue();
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

        Repository.GetItem<BrownSack>().AmIOpen.Should().BeTrue();
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

        Repository.GetItem<Mailbox>().AmIOpen.Should().BeTrue();
        result.Should().Contain("already");
    }

    [Test]
    public async Task OpenProcessor_CloseSomething()
    {
        var target = GetTarget();

        // Act
        await target.GetResponse("open mailbox");
        var result = await target.GetResponse("close mailbox");

        Repository.GetItem<Mailbox>().AmIOpen.Should().BeFalse();
        result.Should().Contain("Closed");
    }

    [Test]
    public async Task OpenProcessor_CloseSomething_AlreadyClosed()
    {
        var target = GetTarget();

        // Act
        var result = await target.GetResponse("close mailbox");

        // Assert
        Repository.GetItem<Mailbox>().AmIOpen.Should().BeFalse();
        result.Should().Contain("already");
    }


    [Test]
    public async Task TurnOnProcessor_NowOn()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();

        // Act
        var result = await target.GetResponse("turn on lantern");

        // Assert
        Repository.GetItem<Lantern>().IsOn.Should().BeTrue();
        result.Should().Contain("now on");
    }

    [Test]
    public async Task TurnOnProcessor_NowOn_AlternateNoun()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();

        // Act
        var result = await target.GetResponse("turn on lamp");

        // Assert
        Repository.GetItem<Lantern>().IsOn.Should().BeTrue();
        result.Should().Contain("now on");
    }

    [Test]
    public async Task TurnOnProcessor_NowOff()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();

        // Act
        await target.GetResponse("turn on lantern");
        var result = await target.GetResponse("turn off lantern");

        // Assert
        Repository.GetItem<Lantern>().IsOn.Should().BeFalse();
        result.Should().Contain("now off");
    }

    [Test]
    public async Task TurnOnProcessor_AlreadyOff()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();

        // Act
        var result = await target.GetResponse("turn off lantern");

        // Assert
        Repository.GetItem<Lantern>().IsOn.Should().BeFalse();
        result.Should().Contain("already");
    }

    [Test]
    public async Task TurnOnProcessor_AlreadyOn()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();

        // Act
        await target.GetResponse("turn on lantern");
        var result = await target.GetResponse("turn on lantern");

        // Assert
        Repository.GetItem<Lantern>().IsOn.Should().BeTrue();
        result.Should().Contain("already");
    }

    [Test]
    public void TurnOnProcessor_WrongType()
    {
        IVerbProcessor target = new TurnOnOrOffProcessor();
        Assert.Throws<Exception>(() => target.Process(Mock.Of<SimpleIntent>(), Mock.Of<IContext>(), new Sword()));
    }

    [Test]
    public async Task Examine_ItemIsClosed()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<Kitchen>();

        // Act
        var result = await target.GetResponse("examine sack");

        // Assert
        result.Should().Contain("The brown sack is closed.");
    }

    [Test]
    public async Task Examine_ItemIsOpen_HasItems()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<Kitchen>();

        // Act
        await target.GetResponse("open sack");
        var result = await target.GetResponse("examine sack");

        // Assert
        result.Should().Contain("The brown sack contains:");
        result.Should().Contain("A lunch");
        result.Should().Contain("A clove of garlic");
    }

    [Test]
    public async Task Take_ItemInsideOpenItem()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<Kitchen>();

        // Act
        await target.GetResponse("open sack");
        var result = await target.GetResponse("take lunch");

        // Assert
        result.Should().Contain("Taken");
    }

    [Test]
    public void CannotBeTaken_WrongType()
    {
        IVerbProcessor target = new CannotBeTakenProcessor();
        Assert.Throws<Exception>(() => target.Process(Mock.Of<SimpleIntent>(), Mock.Of<IContext>(), new MadeUpItem()));
    }

    [Test]
    public async Task CannotBeTaken_PositiveInteraction()
    {
        var target = GetTarget();

        var result = await target.GetResponse("take mailbox");

        result.Should().Contain("securely");
    }

    [Test]
    public async Task Eat()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<Kitchen>();

        // Act
        await target.GetResponse("open sack");
        await target.GetResponse("take lunch");
        var result = await target.GetResponse("eat lunch");

        // Assert
        result.Should().Contain("hit the spot");
    }

    [Test]
    public async Task Eat_SecondTime()
    {
        var target = GetTarget();
        Client.Setup(s => s.CompleteChat(It.IsAny<NounNotPresentOperationRequest>()))
            .ReturnsAsync("BOB");
        target.Context.CurrentLocation = Repository.GetLocation<Kitchen>();

        // Act
        await target.GetResponse("open sack");
        await target.GetResponse("take lunch");
        await target.GetResponse("eat lunch");
        var result = await target.GetResponse("eat lunch");

        // Assert
        result.Should().Contain("BOB");
    }

    [Test]
    public async Task EatProcessor_WrongVerb()
    {
        var target = GetTarget();
        Client.Setup(s => s.CompleteChat(It.IsAny<VerbHasNoEffectOperationRequest>()))
            .ReturnsAsync("BOB");
        target.Context.CurrentLocation = Repository.GetLocation<Kitchen>();

        // Act
        await target.GetResponse("open sack");
        await target.GetResponse("take lunch");
        var result = await target.GetResponse("throw lunch");

        // Assert
        result.Should().Contain("BOB");
    }

    [Test]
    public void EatProcessor_WrongType()
    {
        IVerbProcessor target = new EatInteractionProcessor();
        Assert.Throws<Exception>(() => target.Process(Mock.Of<SimpleIntent>(), Mock.Of<IContext>(), new Lantern()));
    }

    [Test]
    public void EatProcessor_WrongBaseType()
    {
        IVerbProcessor target = new EatInteractionProcessor();
        Assert.Throws<Exception>(() => target.Process(Mock.Of<SimpleIntent>(), Mock.Of<IContext>(), new MadeUpItem()));
    }

    private class MadeUpItem : IInteractionTarget, ICanBeEaten, ICanBeTakenAndDropped
    {
        public string EatenDescription => "";

        public string OnTheGroundDescription => "";

        public string? NeverPickedUpDescription => "";

        public bool HasEverBeenPickedUp => false;
    }
}