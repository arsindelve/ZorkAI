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
        Client.Setup(s => s.CompleteChat(It.IsAny<NounNotPresentOperationRequest>()))
            .ReturnsAsync("BOB");
        target.Context.CurrentLocation = Repository.GetLocation<Kitchen>();

        // Act
        await target.GetResponse("take sack");
        await target.GetResponse("open sack");
        await target.GetResponse("take lunch");
        var result = await target.GetResponse("eat lunch");
        result = await target.GetResponse("eat lunch");

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
        IVerbProcessor target = new EatAndDrinkInteractionProcessor();
        target.Process(Mock.Of<SimpleIntent>(), Mock.Of<IContext>(), new Lantern(), Mock.Of<IGenerationClient>())
            .Should().BeNull();
    }

    [Test]
    public void EatProcessor_WrongBaseType()
    {
        IVerbProcessor target = new EatAndDrinkInteractionProcessor();
        Assert.Throws<Exception>(() => target.Process(Mock.Of<SimpleIntent>(), Mock.Of<IContext>(), new MadeUpItem(),
            Mock.Of<IGenerationClient>()));
    }
}