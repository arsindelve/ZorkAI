using GameEngine;
using GameEngine.Item.MultiItemProcessor;
using Model.Intent;
using Model.Interface;
using ZorkOne;
using ZorkOne.Item;

namespace UnitTests.MultiNounProcessors;

[TestFixture]
public class LightMatchesWithTorchTests : EngineTestsBase
{
    [Test]
    public async Task LightProcessor_LightMatchesWithTorch_ShouldLightMatches()
    {
        var engine = GetTarget();
        var matchbook = Repository.GetItem<Matchbook>();
        var torch = Repository.GetItem<Torch>();
        
        matchbook.IsOn = false;
        
        engine.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();
        
        var location = (ICanHoldItems)engine.Context.CurrentLocation;
        
        matchbook.CurrentLocation = location;
        location.ItemPlacedHere(matchbook);
        
        torch.CurrentLocation = location;
        location.ItemPlacedHere(torch);
        
        var result = await engine.GetResponse("light matches with torch");
        
        result.Should().Contain("One of the matches starts to burn.");
        matchbook.IsOn.Should().BeTrue();
    }
}
