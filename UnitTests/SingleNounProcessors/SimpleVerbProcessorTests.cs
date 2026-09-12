using GameEngine;
using GameEngine.Item.ItemProcessor;
using Model.AIGeneration;
using Model.AIGeneration.Requests;
using Model.Intent;
using Model.Interface;

namespace UnitTests.SingleNounProcessors;

/// <summary>
/// The single-noun state-changing verb processors — open/close, turn on/off, eat/drink. Each is the
/// same shape: issue "&lt;verb&gt; &lt;noun&gt;", assert the item's flag flipped and the right message
/// came back, then the already-in-that-state and wrong-item-type branches. Merged from the former
/// OpenProcessorTest / TurnOnOffProcessorTests / EatProcessorTests.
/// </summary>
public class SimpleVerbProcessorTests : EngineTestsBase
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
    public async Task TurnOnProcessor_NowOn_WithAdverb()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();

        // Act
        var result = await target.GetResponse("turn the lamp on");

        // Assert
        Repository.GetItem<Lantern>().IsOn.Should().BeTrue();
        result.Should().Contain("now on");
    }

    [Test]
    public async Task TurnOnProcessor_NowOn_WithWrongAdverb()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();

        // Act
        var result = await target.GetResponse("turn the lamp around");

        // Assert
        result?.Trim().Should().BeEmpty();
    }

    [Test]
    public async Task TurnOnProcessor_NowOn_WithNoAdverb()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();

        // Act
        var result = await target.GetResponse("turn the lamp");

        // Assert
        result?.Trim().Should().BeEmpty();
    }

    [Test]
    public async Task TurnOnProcessor_NowOff_WithAdverb()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();

        // Act
        await target.GetResponse("turn on lantern");
        var result = await target.GetResponse("turn the lamp off");

        // Assert
        Repository.GetItem<Lantern>().IsOn.Should().BeFalse();
        result.Should().Contain("now off");
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
        target.Process(Mock.Of<SimpleIntent>(), Mock.Of<IContext>(), new Sword(), Mock.Of<IGenerationClient>()).Result
            .Should()
            .BeNull();
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
    public async Task Eat_BeforeTakingIt()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<Kitchen>();

        // Act
        await target.GetResponse("open sack");
        var result = await target.GetResponse("eat lunch");

        // Assert
        result.Should().Contain("hit the spot");
        result.Should().Contain("Taken");
    }

    [Test]
    public async Task Eat_SecondTime()
    {
        var target = GetTarget();
        Client.Setup(s => s.GenerateNarration(It.IsAny<NounNotPresentOperationRequest>(), It.IsAny<string>()))
            .ReturnsAsync("BOB");
        target.Context.CurrentLocation = Repository.GetLocation<Kitchen>();

        // Act
        await target.GetResponse("take sack");
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
        Client.Setup(s => s.GenerateNarration(It.IsAny<VerbHasNoEffectOperationRequest>(), It.IsAny<string>()))
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
        IVerbProcessor target = new EatAndDrinkInteractionProcessor();
        target.Process(Mock.Of<SimpleIntent>(), Mock.Of<IContext>(), new Lantern(), Mock.Of<IGenerationClient>())
            .Result.Should().BeNull();
    }

    [Test]
    public void EatProcessor_WrongBaseType()
    {
        IVerbProcessor target = new EatAndDrinkInteractionProcessor();
        Assert.Throws<Exception>(() => target.Process(Mock.Of<SimpleIntent>(), Mock.Of<IContext>(), new MadeUpItem(),
            Mock.Of<IGenerationClient>()));
    }
}
