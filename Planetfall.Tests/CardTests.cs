using FluentAssertions;
using GameEngine;
using Planetfall.Item.Kalamontee.Admin;
using Planetfall.Location.Kalamontee;

namespace Planetfall.Tests;

public class CardTests : EngineTestsBase
{
    
    [Test]
    [TestCase("kitchen")]
    [TestCase("card")]
    [TestCase("kitchen access")]
    [TestCase("kitchen card")]
    [TestCase("kitchen access card")]
    public async Task Single_Card(string reply)
    {
        var engine = GetTarget();
        var room = Repository.GetLocation<MessCorridor>();
        engine.Context.ItemPlacedHere(Repository.GetItem<KitchenAccessCard>());
        
        engine.Context.CurrentLocation = room;
        
        string? response = await engine.GetResponse($"drop {reply}");

        response.Should().Contain("Dropped");
        engine.Context.Items.Should().HaveCount(0);
        Repository.GetItem<KitchenAccessCard>().CurrentLocation.Should().BeOfType<MessCorridor>();
    }
    
    [Test]
    [TestCase("kitchen")]
    [TestCase("kitchen access")]
    [TestCase("kitchen card")]
    [TestCase("kitchen access card")]
    public async Task Double_Card(string reply)
    {
        var engine = GetTarget();
        var room = Repository.GetLocation<MessCorridor>();
        engine.Context.ItemPlacedHere(Repository.GetItem<KitchenAccessCard>());
        engine.Context.ItemPlacedHere(Repository.GetItem<UpperElevatorAccessCard>());
        
        engine.Context.CurrentLocation = room;
        
        string? response = await engine.GetResponse($"drop {reply}");

        response.Should().Contain("Dropped");
        engine.Context.Items.Should().HaveCount(1);
        Repository.GetItem<KitchenAccessCard>().CurrentLocation.Should().BeOfType<MessCorridor>();
    }
    
    [Test]
    [TestCase("kitchen")]
    [TestCase("kitchen access")]
    [TestCase("kitchen card")]
    [TestCase("kitchen access card")]
    public async Task Disambiguation_Card_Drop_Kitchen(string reply)
    {
        var engine = GetTarget();
        var room = Repository.GetLocation<MessCorridor>();
        engine.Context.ItemPlacedHere(Repository.GetItem<ShuttleAccessCard>());
        engine.Context.ItemPlacedHere(Repository.GetItem<UpperElevatorAccessCard>());
        engine.Context.ItemPlacedHere(Repository.GetItem<KitchenAccessCard>());
        
        engine.Context.CurrentLocation = room;
        
        await engine.GetResponse("drop card");
        string? response = await engine.GetResponse(reply);

        response.Should().Contain("Dropped");
        engine.Context.Items.Should().HaveCount(2);
        Repository.GetItem<KitchenAccessCard>().CurrentLocation.Should().BeOfType<MessCorridor>();
    }
    
    [Test]
    [TestCase("shuttle")]
    [TestCase("shuttle access")]
    [TestCase("shuttle card")]
    [TestCase("shuttle access card")]
    public async Task Disambiguation_Card_Drop_Shuttle(string reply)
    {
        var engine = GetTarget();
        var room = Repository.GetLocation<MessCorridor>();
        engine.Context.ItemPlacedHere(Repository.GetItem<ShuttleAccessCard>());
        engine.Context.ItemPlacedHere(Repository.GetItem<UpperElevatorAccessCard>());
        engine.Context.ItemPlacedHere(Repository.GetItem<KitchenAccessCard>());
        
        engine.Context.CurrentLocation = room;
        
        Console.WriteLine(await engine.GetResponse("drop card"));
        string? response = await engine.GetResponse(reply);

        response.Should().Contain("Dropped");
        engine.Context.Items.Should().HaveCount(2);
        Repository.GetItem<ShuttleAccessCard>().CurrentLocation.Should().BeOfType<MessCorridor>();
    }
    
    [Test]
    [TestCase("shuttle")]
    [TestCase("shuttle access")]
    [TestCase("shuttle card")]
    [TestCase("shuttle access card")]
    public async Task Disambiguation_CardAccess_Drop_Shuttle(string reply)
    {
        var engine = GetTarget();
        var room = Repository.GetLocation<MessCorridor>();
        engine.Context.ItemPlacedHere(Repository.GetItem<ShuttleAccessCard>());
        engine.Context.ItemPlacedHere(Repository.GetItem<UpperElevatorAccessCard>());
        engine.Context.ItemPlacedHere(Repository.GetItem<KitchenAccessCard>());
        
        engine.Context.CurrentLocation = room;
        
        Console.WriteLine(await engine.GetResponse("drop the access card"));
        string? response = await engine.GetResponse(reply);

        response.Should().Contain("Dropped");
        engine.Context.Items.Should().HaveCount(2);
        Repository.GetItem<ShuttleAccessCard>().CurrentLocation.Should().BeOfType<MessCorridor>();
    }
    
    [Test]
    [TestCase("upper")]
    [TestCase("upper access")]
    [TestCase("upper card")]
    [TestCase("upper access card")]
    [TestCase("upper elevator access card")]
    [TestCase("upper elevator card")]
    public async Task Disambiguation_Card_Drop_UpperElevator(string reply)
    {
        var engine = GetTarget();
        var room = Repository.GetLocation<MessCorridor>();
        engine.Context.ItemPlacedHere(Repository.GetItem<ShuttleAccessCard>());
        engine.Context.ItemPlacedHere(Repository.GetItem<UpperElevatorAccessCard>());
        engine.Context.ItemPlacedHere(Repository.GetItem<KitchenAccessCard>());
        
        engine.Context.CurrentLocation = room;
        
        Console.WriteLine(await engine.GetResponse("drop card"));
        string? response = await engine.GetResponse(reply);

        response.Should().Contain("Dropped");
        engine.Context.Items.Should().HaveCount(2);
        Repository.GetItem<UpperElevatorAccessCard>().CurrentLocation.Should().BeOfType<MessCorridor>();
    }
}