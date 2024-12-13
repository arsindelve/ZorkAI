using System;
using GameEngine;

namespace UnitTests.GlobalCommands;

public class ItProcessorTests : EngineTestsBase
{
    [Test]
    public async Task It_SuccessPath()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();

        await target.GetResponse("take lantern");
        var result = await target.GetResponse("drop it");

        target.Context.HasItem<Lantern>().Should().BeFalse();
        result.Should().Contain("Dropped");
    }

    [Test]
    public async Task Them_SuccessPath()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<Altar>();

        await target.GetResponse("take candles");
        var result = await target.GetResponse("drop them");

        target.Context.HasItem<Candles>().Should().BeFalse();
        result.Should().Contain("Dropped");
    }

    [Test]
    public async Task Them_WithItemThatIsNotPlural()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();

        await target.GetResponse("take lantern");
        var result = await target.GetResponse("drop them");

        target.Context.HasItem<Lantern>().Should().BeTrue();
        result.Should().Contain("What item are you referring to");
    }

    [Test]
    public async Task Them_WithItemThatIsNotPlural_Disambiguation()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();

        await target.GetResponse("take lantern");
        var result = await target.GetResponse("drop them");

        target.Context.HasItem<Lantern>().Should().BeTrue();
        result.Should().Contain("What item are you referring to");

        result = await target.GetResponse("lantern");

        target.Context.HasItem<Candles>().Should().BeFalse();
        result.Should().Contain("Dropped");
    }

    [Test]
    public async Task It_NoLastNoun()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();

        var result = await target.GetResponse("take it");
        result = await target.GetResponse("lantern");

        target.Context.HasItem<Lantern>().Should().BeTrue();
        result.Should().Contain("Taken");
    }
}