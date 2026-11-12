using FluentAssertions;
using GameEngine;
using ZorkOne.Item;
using ZorkOne.Location.CoalMineLocation;
using ZorkOne.Location.ForestLocation;

namespace ZorkOne.Tests.Things;

public class LanternTests : EngineTestsBase
{
    [Test]
    public async Task LightSourceDoesNotNeedToBeInPossession()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<ShaftRoom>();
        var torch = Repository.GetItem<Torch>();
        target.Context.Take(torch);

        var response = await target.GetResponse("put torch in basket");
        Console.WriteLine(response);

        target.Context.ItIsDarkHere.Should().BeFalse();
    }

    [Test]
    public async Task CounterIncreases_WhenLanternIsOn()
    {
        var target = GetTarget();

        target.Context.CurrentLocation = Repository.GetLocation<ForestOne>();
        var lantern = Repository.GetItem<Lantern>();
        target.Context.Take(lantern);

        await target.GetResponse("turn on lantern");
        await target.GetResponse("wait");

        lantern.TurnsWhileOn.Should().BeGreaterThan(0);
    }

    // Verbs.TurnOnVerbs / TurnOffVerbs are the same for a lamp as for a robot or a radio; Verbs.LightVerbs
    // and Verbs.ExtinguishVerbs are the extra words only a light answers.
    [TestCase("turn on lantern", "turn off lantern")]
    [TestCase("start lantern", "stop lantern")]
    [TestCase("enable lantern", "disable lantern")]
    [TestCase("light lantern", "extinguish lantern")]
    public async Task TheLantern_GoesOnAndOff_InAnyOfTheWordsForIt(string on, string off)
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<ForestOne>();
        var lantern = Repository.GetItem<Lantern>();
        target.Context.Take(lantern);

        await target.GetResponse(on);
        lantern.IsOn.Should().BeTrue($"\"{on}\" switches it on");

        await target.GetResponse(off);
        lantern.IsOn.Should().BeFalse($"\"{off}\" switches it off");
    }

    [Test]
    public async Task CounterDoesNotIncrease_WhenLanternIsOff()
    {
        var target = GetTarget();

        target.Context.CurrentLocation = Repository.GetLocation<ForestOne>();
        var lantern = Repository.GetItem<Lantern>();
        target.Context.Take(lantern);

        await target.GetResponse("turn on lantern");
        await target.GetResponse("wait");
        await target.GetResponse("turn off lantern");
        await target.GetResponse("wait");
        await target.GetResponse("wait");

        lantern.TurnsWhileOn.Should().Be(2);
    }

    [Test]
    public async Task GetWarningAtTwoHundredTurns()
    {
        var target = GetTarget();

        target.Context.CurrentLocation = Repository.GetLocation<ForestOne>();
        var lantern = Repository.GetItem<Lantern>();
        target.Context.Take(lantern);

        await target.GetResponse("turn on lantern");

        for (var i = 0; i < 198; i++)
            await target.GetResponse("wait");

        var response = await target.GetResponse("wait");

        response.Should().Contain("dimmer");
    }

    [Test]
    public async Task GetWarningAtThreeHundredTurns()
    {
        var target = GetTarget();

        target.Context.CurrentLocation = Repository.GetLocation<ForestOne>();
        var lantern = Repository.GetItem<Lantern>();
        target.Context.Take(lantern);

        await target.GetResponse("turn on lantern");

        for (var i = 0; i < 298; i++)
            await target.GetResponse("wait");

        var response = await target.GetResponse("wait");

        response.Should().Contain("definitely dimmer");
    }

    [Test]
    public async Task GetWarningAtThreeHundredSeventyTurns()
    {
        var target = GetTarget();

        target.Context.CurrentLocation = Repository.GetLocation<ForestOne>();
        var lantern = Repository.GetItem<Lantern>();
        target.Context.Take(lantern);

        await target.GetResponse("turn on lantern");

        for (var i = 0; i < 368; i++)
            await target.GetResponse("wait");

        var response = await target.GetResponse("wait");

        response.Should().Contain("nearly out");
    }

    [Test]
    public async Task LanternBusted()
    {
        var target = GetTarget();

        target.Context.CurrentLocation = Repository.GetLocation<ForestOne>();
        var lantern = Repository.GetItem<Lantern>();
        target.Context.Take(lantern);

        await target.GetResponse("turn on lantern");

        for (var i = 0; i < 500; i++)
            await target.GetResponse("wait");

        target.Context.HasLightSource.Should().BeFalse();

        var response = await target.GetResponse("turn on lantern");
        response.Should().Contain("A burned-out lamp won't light");
        lantern.IsOn.Should().BeFalse();
    }
}