using ZorkOne.Location.CoalMineLocation;

namespace UnitTests.ZorkITests;

public class LanternTests : EngineTestsBase
{
    [Test]
    public async Task LightSourceDoesNotNeedToBeInPossession()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<ShaftRoom>();
        var torch = Repository.GetItem<Torch>();
        target.Context.Take(torch);

        string? response = await target.GetResponse("put torch in basket");
        Console.WriteLine(response);

        target.Context.ItIsDarkHere.Should().BeFalse();
    }
    
    [Test]
    public async Task CounterIncreases_WhenLanternIsOn()
    {
        var target = GetTarget();

        target.Context.CurrentLocation = Repository.GetLocation<Cellar>();
        var lantern = Repository.GetItem<Lantern>();
        target.Context.Take(lantern);

        await target.GetResponse("turn on lantern");
        await target.GetResponse("wait");

        lantern.TurnsWhileOn.Should().BeGreaterThan(0);
    }

    [Test]
    public async Task CounterDoesNotIncrease_WhenLanternIsOff()
    {
        var target = GetTarget();

        target.Context.CurrentLocation = Repository.GetLocation<Cellar>();
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

        target.Context.CurrentLocation = Repository.GetLocation<Cellar>();
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

        target.Context.CurrentLocation = Repository.GetLocation<Cellar>();
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

        target.Context.CurrentLocation = Repository.GetLocation<Cellar>();
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

        target.Context.CurrentLocation = Repository.GetLocation<Cellar>();
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