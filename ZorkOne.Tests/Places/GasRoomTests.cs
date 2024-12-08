using FluentAssertions;
using GameEngine;
using NUnit.Framework;
using UnitTests;
using ZorkOne.Item;
using ZorkOne.Location.CoalMineLocation;

namespace ZorkOne.Tests.Places;

public class GasRoomTests : EngineTestsBase
{
    [Test]
    public async Task WalkInWithATorch_YouExplode()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<SmellyRoom>();
        target.Context.Take(Repository.GetItem<Torch>());

        // Act
        var response = await target.GetResponse("down");
        Console.WriteLine(response);

        // Assert
        target.Context.DeathCounter.Should().Be(1);
        response.Should().Contain("BOOOOOOOOOOOM");
    }
    
    [Test]
    public async Task WalkInWithACandle_Unlit_YerGood()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<SmellyRoom>();
        target.Context.Take(Repository.GetItem<Candles>());
        target.Context.Take(Repository.GetItem<Lantern>());
        Repository.GetItem<Lantern>().IsOn = true;

        // Act
        var response = await target.GetResponse("down");
        Console.WriteLine(response);

        // Assert
        target.Context.DeathCounter.Should().Be(0);
        response.Should().NotContain("BOOOOOOOOOOOM");
    }
    
    [Test]
    public async Task WalkInWithACandle_Lit_YerDed()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<SmellyRoom>();
        target.Context.Take(Repository.GetItem<Candles>());
        target.Context.Take(Repository.GetItem<Lantern>());
        Repository.GetItem<Lantern>().IsOn = true;
        Repository.GetItem<Candles>().IsOn = true;

        // Act
        var response = await target.GetResponse("down");
        Console.WriteLine(response);

        // Assert
        target.Context.DeathCounter.Should().Be(1);
        response.Should().Contain("BOOOOOOOOOOOM");
    }
    
    [Test]
    public async Task WalkInWithAMatchbook_Lit_YerDed()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<SmellyRoom>();
        target.Context.Take(Repository.GetItem<Matchbook>());
        target.Context.Take(Repository.GetItem<Lantern>());
        Repository.GetItem<Lantern>().IsOn = true;
        Repository.GetItem<Matchbook>().IsOn = true;

        // Act
        var response = await target.GetResponse("down");
        Console.WriteLine(response);

        // Assert
        target.Context.DeathCounter.Should().Be(1);
        response.Should().Contain("BOOOOOOOOOOOM");
    }
    
    [Test]
    public async Task LightAMatch_YerDed()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<SmellyRoom>();
        target.Context.Take(Repository.GetItem<Matchbook>());
        target.Context.Take(Repository.GetItem<Lantern>());
        Repository.GetItem<Lantern>().IsOn = true;

        // Act
        await target.GetResponse("down");
        var response = await target.GetResponse("light matchbook");
        Console.WriteLine(response);

        // Assert
        target.Context.DeathCounter.Should().Be(1);
        response.Should().Contain("BOOOOOOOOOOOM");
    }
}