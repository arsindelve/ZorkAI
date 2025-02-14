using FluentAssertions;
using GameEngine;
using ZorkOne.Item;
using ZorkOne.Location;
using ZorkOne.Location.ForestLocation;

namespace ZorkOne.Tests.Things;

[TestFixture]
public class SceptreTests : EngineTestsBase
{
    [Test]
    public async Task RainbowBecomesSolid()
    {
        var target = GetTarget();

        target.Context.CurrentLocation = Repository.GetLocation<EndOfRainbow>();
        var sceptre = Repository.GetItem<Sceptre>();
        target.Context.Take(sceptre);

        var response = await target.GetResponse("wave sceptre");
        response.Should().Contain("Suddenly, the rainbow appears to become solid");
        Repository.GetLocation<EndOfRainbow>().RainbowIsSolid.Should().BeTrue();
    }

    [Test]
    public async Task SecondTimeRainbowBecomesRegular()
    {
        var target = GetTarget();

        target.Context.CurrentLocation = Repository.GetLocation<EndOfRainbow>();
        var sceptre = Repository.GetItem<Sceptre>();
        target.Context.Take(sceptre);

        await target.GetResponse("wave sceptre");
        var response = await target.GetResponse("wave sceptre");

        response.Should().Contain("The rainbow seems to have become somewhat run-of-the-mill.");
        Repository.GetLocation<EndOfRainbow>().RainbowIsSolid.Should().BeFalse();
    }

    [Test]
    public async Task WaveSomewhereElse()
    {
        var target = GetTarget();

        target.Context.CurrentLocation = Repository.GetLocation<ForestOne>();
        var sceptre = Repository.GetItem<Sceptre>();
        target.Context.Take(sceptre);

        var response = await target.GetResponse("wave sceptre");
        response.Should().Contain("A dazzling display of color briefly");
    }

    [Test]
    public async Task PotAppears()
    {
        var target = GetTarget();

        target.Context.CurrentLocation = Repository.GetLocation<EndOfRainbow>();
        var sceptre = Repository.GetItem<Sceptre>();
        target.Context.Take(sceptre);

        var response = await target.GetResponse("wave sceptre");

        response.Should().Contain("A shimmering pot of gold appears at the end of the rainbow.");
        Repository.GetLocation<EndOfRainbow>().HasItem<PotOfGold>().Should().BeTrue();
        Repository.GetItem<PotOfGold>().CurrentLocation.Should().Be(Repository.GetLocation<EndOfRainbow>());
    }

    [Test]
    public async Task ThirdTimePotDoesNotAppearAgain()
    {
        var target = GetTarget();

        target.Context.CurrentLocation = Repository.GetLocation<EndOfRainbow>();
        var sceptre = Repository.GetItem<Sceptre>();
        target.Context.Take(sceptre);

        await target.GetResponse("wave sceptre");
        await target.GetResponse("take gold");
        await target.GetResponse("wave sceptre");
        var response = await target.GetResponse("wave sceptre");

        response.Should().NotContain("A shimmering pot of gold appears at the end of the rainbow.");
        target.Context.HasItem<PotOfGold>().Should().BeTrue();
    }

    [Test]
    public async Task OnRainbow_WaveIt_Die()
    {
        var target = GetTarget();

        target.Context.CurrentLocation = Repository.GetLocation<OnTheRainbow>();
        var sceptre = Repository.GetItem<Sceptre>();
        target.Context.Take(sceptre);

        var response = await target.GetResponse("wave sceptre");

        response.Should().Contain("Bye");
        target.Context.DeathCounter.Should().Be(1);
    }
}