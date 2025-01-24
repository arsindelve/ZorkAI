using FluentAssertions;
using ZorkOne.Item;
using ZorkOne.Location;
using ZorkOne.Location.MazeLocation;

namespace ZorkOne.Tests;

public class FightSimulatorTests : EngineTestsBase
{

    [Test]
    public async Task Troll()
    {
        var winners  = new List<Winner>();
        
        for (var i = 0; i < 1000; i++)
            winners.Add(await FightTroll());

        var adventurer = winners.Count(w => w == Winner.Adventurer);
        var foe = winners.Count(w => w == Winner.Foe);
        
        Console.WriteLine($"Adventurer: {adventurer}");
        Console.WriteLine($"Foe: {foe}");

        foe.Should().BeGreaterThan(0);
        foe.Should().BeLessThan(1000);
        adventurer.Should().BeGreaterThan(0);
        adventurer.Should().BeLessThan(1000);
    }
    
    [Test]
    public async Task Thief()
    {
        var winners  = new List<Winner>();
        
        for (var i = 0; i < 1000; i++)
            winners.Add(await FightThief());
        
        var adventurer = winners.Count(w => w == Winner.Adventurer);
        var foe = winners.Count(w => w == Winner.Foe);
        
        Console.WriteLine($"Adventurer: {adventurer}");
        Console.WriteLine($"Foe: {foe}");

        foe.Should().BeGreaterThan(0);
        foe.Should().BeLessThan(1000);
        adventurer.Should().BeGreaterThan(0);
        adventurer.Should().BeLessThan(1000);
    }

    private async Task<Winner> FightTroll()
    {
        var target = GetTarget();
        StartHere<Cellar>();
        Take<Sword>();
        Take<Torch>();

        await target.GetResponse("i");
        await target.GetResponse("N");
            
        while (Context.DeathCounter == 0 && !GetItem<Troll>().IsDead)
        {
            if (!Context.HasWeapon)
                await target.GetResponse("take sword");
            else
                await target.GetResponse("kill the troll with the sword");
        }
        
        var winner = Context.DeathCounter == 0 ? Winner.Adventurer : Winner.Foe;
        return winner;
    }
    
    private async Task<Winner> FightThief()
    {
        var target = GetTarget();
        StartHere<CyclopsRoom>();
        GetItem<Cyclops>().IsSleeping = true;
        Take<Sword>();
        Take<Torch>();
        
        await target.GetResponse("up");
            
        while (Context.DeathCounter == 0 && !GetItem<Thief>().IsDead)
        {
            if (!Context.HasWeapon)
                await target.GetResponse("take sword");
            else
                await target.GetResponse("kill thief with sword");
        }
        
        var winner = Context.DeathCounter == 0 ? Winner.Adventurer : Winner.Foe;
        return winner;
    }

    private enum Winner
    {
        Adventurer, 
        Foe
    }
}