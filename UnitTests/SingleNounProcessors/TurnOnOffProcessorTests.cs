using Game.Item.ItemProcessor;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;

namespace UnitTests.SingleNounProcessors;

public class TurnOnOffProcessorTests : EngineTestsBase
{
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
        IVerbProcessor target = new TurnLightOnOrOffProcessor();
        target.Process(Mock.Of<SimpleIntent>(), Mock.Of<IContext>(), new Sword(), Mock.Of<IGenerationClient>()).Should()
            .BeNull();
    }
}