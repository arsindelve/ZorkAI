using Game.Item.ItemProcessor;
using Model.Intent;

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
    public async Task TakeSecondItemFromContainer()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<Kitchen>();
        await target.GetResponse("open sack");
        var result = await target.GetResponse("take garlic");
        result.Should().Contain("Taken");
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
    
}