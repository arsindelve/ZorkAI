using GameEngine;
using GameEngine.Item.MultiItemProcessor;
using Model.Intent;
using Model.Interface;
using Model.Item;
using ZorkOne;
using ZorkOne.Item;

namespace UnitTests.MultiNounProcessors;

[TestFixture]
public class PutSceptreInCaseTests : EngineTestsBase
{
    [Test]
    public async Task PutProcessor_SceptreAlreadyInCase_ShouldNotPutCaseInCase()
    {
        var engine = GetTarget();
        var sceptre = Repository.GetItem<Sceptre>();
        var trophyCase = Repository.GetItem<TrophyCase>();
        
        ((IOpenAndClose)trophyCase).IsOpen = true;
        
        sceptre.CurrentLocation = trophyCase;
        
        engine.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();
        
        var result = await engine.GetResponse("put sceptre in case");

        result.Should().NotContain("put the case in the case");
        
        sceptre.CurrentLocation.Should().Be(trophyCase);
    }
}
