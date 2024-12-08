using FluentAssertions;
using GameEngine;
using NUnit.Framework;
using UnitTests;
using ZorkOne.Item;
using ZorkOne.Location;

namespace ZorkOne.Tests.Things;

public class CandlesTests : EngineTestsBase
{
    [Test]
    public async Task LightThem_DoNotSpecify_NoMatchLit()
    {
        var target = GetTarget();

        target.Context.Take(Repository.GetItem<Candles>());
        target.Context.Take(Repository.GetItem<Matchbook>());

        var response = await target.GetResponse("light candles");
        response.Should().Contain("You should say");
    }
    
    [Test]
    public async Task LightThem_DoNotSpecify_MatchLit()
    {
        var target = GetTarget();

        target.Context.Take(Repository.GetItem<Candles>());
        target.Context.Take(Repository.GetItem<Matchbook>());
        Repository.GetItem<Matchbook>().IsOn = true;

        var response = await target.GetResponse("light candles");
        response.Should().Contain("The candles are lit.");
        Repository.GetItem<Candles>().IsOn.Should().BeTrue();
    }
    
    [Test]
    public async Task LightThem_DoNotSpecify_MatchLit_ButNoCandles()
    {
        var target = GetTarget();
        
        target.Context.Take(Repository.GetItem<Matchbook>());
        Repository.GetItem<Matchbook>().IsOn = true;

        var response = await target.GetResponse("light candles");
        Repository.GetItem<Candles>().IsOn.Should().BeFalse();
    }

    [Test]
    public async Task LightThem_WithMatchbook()
    {
        var target = GetTarget();

        target.Context.Take(Repository.GetItem<Candles>());
        target.Context.Take(Repository.GetItem<Matchbook>());

        var response = await target.GetResponse("light candles with match");
        response.Should().Contain("You'll need to light a match first");
        Repository.GetItem<Candles>().IsOn.Should().BeFalse();
    }
    
    [Test]
    public async Task LightThem_WithMatchbook_DoNotHaveIt()
    {
        var target = GetTarget();

        target.Context.Take(Repository.GetItem<Candles>());

        var response = await target.GetResponse("light candles with match");
        Repository.GetItem<Candles>().IsOn.Should().BeFalse();
    }
    
    [Test]
    public async Task LightThem_WithTorch()
    {
        var target = GetTarget();

        target.Context.Take(Repository.GetItem<Candles>());
        target.Context.Take(Repository.GetItem<Torch>());

        var response = await target.GetResponse("light candles with torch");
        response.Should().Contain("The heat from the torch is so intense that the candles are vaporized.");
        Repository.GetItem<Candles>().IsOn.Should().BeFalse();
        Repository.GetItem<Candles>().CurrentLocation.Should().BeNull();
        target.Context.HasItem<Candles>().Should().BeFalse();
    }
}