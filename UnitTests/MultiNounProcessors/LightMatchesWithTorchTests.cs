using GameEngine;
using Model.Intent;
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
        
        matchbook.IsOn = false;
        
        engine.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();
        
        var result = await engine.GetResponse("light matches with torch");
        
        result.Should().Contain("One of the matches starts to burn.");
        matchbook.IsOn.Should().BeTrue();
    }
}
