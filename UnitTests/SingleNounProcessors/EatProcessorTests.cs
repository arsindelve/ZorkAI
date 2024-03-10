using Game.Item.ItemProcessor;
using Model.Intent;
using OpenAI.Requests;

namespace UnitTests.SingleNounProcessors;

public class EatProcessorTests : EngineTestsBase
{
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
}