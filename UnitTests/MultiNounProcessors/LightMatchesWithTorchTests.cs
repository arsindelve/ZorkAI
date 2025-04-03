using GameEngine;
using Model.Intent;
using Model.Interface;
using ZorkOne;
using ZorkOne.Item;

namespace UnitTests.MultiNounProcessors;

[TestFixture]
public class LightMatchesWithTorchTests : EngineTestsBase
{
    [Test]
    public async Task Matchbook_LightMatchesWithTorch_ShouldLightMatches()
    {
        var engine = GetTarget();
        var matchbook = Take<Matchbook>();
        var torch = Take<Torch>();
        
        engine.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();
        
        var result = await engine.GetResponse("light matches with torch");
        
        result.Should().Contain("One of the matches starts to burn.");
        matchbook.IsOn.Should().BeTrue();
    }
    
    [Test]
    public async Task Matchbook_LightMatchesWithTorch_ShouldNotLightWhenOutOfMatches()
    {
        var engine = GetTarget();
        var matchbook = Take<Matchbook>();
        var torch = Take<Torch>();
        
        matchbook.MatchesUsed = 5; // All matches used up
        
        engine.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();
        
        var result = await engine.GetResponse("light matches with torch");
        
        result.Should().Contain("I'm afraid that you have run out of matches.");
        matchbook.IsOn.Should().BeFalse();
    }
    
    [Test]
    public async Task Matchbook_LightMatchesWithTorch_ShouldNotLightWhenAlreadyLit()
    {
        var engine = GetTarget();
        var matchbook = Take<Matchbook>();
        var torch = Take<Torch>();
        
        matchbook.IsOn = true; // Matches already lit
        
        engine.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();
        
        var result = await engine.GetResponse("light matches with torch");
        
        result.Should().NotContain("One of the matches starts to burn.");
        matchbook.IsOn.Should().BeTrue();
    }
    
    [Test]
    public async Task Matchbook_LightMatchesWithTorch_ShouldNotLightWhenTorchNotInInventory()
    {
        var engine = GetTarget();
        var matchbook = Take<Matchbook>();
        
        var torch = Repository.GetItem<Torch>();
        
        var livingRoom = Repository.GetLocation<LivingRoom>();
        engine.Context.CurrentLocation = livingRoom;
        
        torch.CurrentLocation = livingRoom;
        livingRoom.ItemPlacedHere(torch);
        
        var result = await engine.GetResponse("light matches with torch");
        
        result.Should().Contain("You don't have the torch.");
        matchbook.IsOn.Should().BeFalse();
    }
}
