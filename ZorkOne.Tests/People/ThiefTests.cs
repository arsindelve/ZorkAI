using FluentAssertions;
using ZorkOne.Item;
using ZorkOne.Location.MazeLocation;

namespace ZorkOne.Tests.People;

public class ThiefTests : EngineTestsBase
{
    [Test]
    public async Task CannotTakeChaliceWhenAlive()
    {
        var target = GetTarget();
        StartHere<TreasureRoom>();

        var response = await target.GetResponse("take chalice");

        response.Should().Contain("stabbed in the back");
    }
    
    [Test]
    public async Task CanTakeChaliceWhenDead()
    {
        var target = GetTarget();
        StartHere<TreasureRoom>();
        GetItem<Thief>().IsDead = true;

        var response = await target.GetResponse("take chalice");

        response.Should().Contain("Taken");
    }
    
    [Test]
    public async Task CanTakeChaliceWhenUnconscious()
    {
        var target = GetTarget();
        StartHere<TreasureRoom>();
        GetItem<Thief>().IsUnconscious = true;

        var response = await target.GetResponse("take chalice");

        response.Should().Contain("Taken");
    }
    
    [Test]
    public async Task RunsToDefendRoom()
    {
        var target = GetTarget();
        StartHere<CyclopsRoom>();
        GetItem<Cyclops>().IsSleeping = true;

        var response = await target.GetResponse("up");

        response.Should().Contain("rushes to its defense");
    }
    
    [Test]
    public async Task Dead_DoesNotDefendRoom()
    {
        var target = GetTarget();
        StartHere<CyclopsRoom>();
        GetItem<Cyclops>().IsSleeping = true;
        GetItem<Thief>().IsDead = true;

        var response = await target.GetResponse("up");

        response.Should().NotContain("rushes to its defense");
    }
}