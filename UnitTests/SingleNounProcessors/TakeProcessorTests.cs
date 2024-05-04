using Game.Item.ItemProcessor;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;

namespace UnitTests.SingleNounProcessors;

public class TakeProcessorTests : EngineTestsBase
{
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
    public async Task Take_ItemInsideClosedItem()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<Kitchen>();

        // Act
        var result = await target.GetResponse("take lunch");

        // Assert
        result.Should().NotContain("Taken");
    }

    [Test]
    public void CannotBeTaken_WrongType()
    {
        IVerbProcessor target = new CannotBeTakenProcessor();
        Assert.Throws<Exception>(() => target.Process(Mock.Of<SimpleIntent>(), Mock.Of<IContext>(), new MadeUpItem(),
            Mock.Of<IGenerationClient>()));
    }

    [Test]
    public async Task CannotBeTaken_PositiveInteraction()
    {
        var target = GetTarget();

        var result = await target.GetResponse("take mailbox");

        result.Should().Contain("securely");
    }

    [Test]
    public async Task TakeSecondItemFromContainer()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<Kitchen>();
        await target.GetResponse("open sack");
        var result = await target.GetResponse("take garlic");
        result.Should().Contain("Taken");
    }

    [Test]
    public async Task TakeItemFromClosedTransparentContainer()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();
        Repository.GetItem<TrophyCase>().ItemPlacedHere(Repository.GetItem<Torch>());
        Repository.GetItem<Torch>().CurrentLocation = Repository.GetItem<TrophyCase>();

        var result = await target.GetResponse("take torch");
        result.Should().Contain("closed container");
    }

    [Test]
    public async Task TakeFirstItemFromContainer()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<Kitchen>();
        await target.GetResponse("open sack");
        var result = await target.GetResponse("take lunch");
        result.Should().Contain("Taken");
    }

    [Test]
    public async Task DropItemIDoNotHave()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<Attic>();
        target.Context.Take(Repository.GetItem<Lantern>());
        Repository.GetItem<Lantern>().IsOn = true;
        var result = await target.GetResponse("drop rope");
        result.Should().Contain("don't have that");
    }

    [Test]
    public async Task TakeItemIAlreadyHave()
    {
        var target = GetTarget();
        Repository.GetItem<Lantern>().IsOn = true;
        target.Context.Take(Repository.GetItem<Lantern>());
        target.Context.CurrentLocation = Repository.GetLocation<Attic>();
        await target.GetResponse("take rope");
        var result = await target.GetResponse("take rope");
        result.Should().Contain("already have that");
    }

    [Test]
    public async Task TakeItemIAlreadyHaveInsideAContainer()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<Kitchen>();
        await target.GetResponse("take sack");
        await target.GetResponse("open sack");
        var result = await target.GetResponse("take garlic");
        result.Should().Contain("Taken");
    }
}