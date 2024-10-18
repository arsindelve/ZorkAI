using GameEngine;

namespace UnitTests.SingleNounProcessors;

public class DisambiguationTests : EngineTestsBase
{
    [Test]
    public async Task SoManyButtons_StepOne()
    {
        var engine = GetTarget();
        var room = Repository.GetLocation<MaintenanceRoom>();
        room.IsNoLongerDark = true;
        
        engine.Context.CurrentLocation = room;

        var response = await engine.GetResponse("press button");

        response.Should().Contain("Which button do you mean, the blue button, the red button, the yellow button or the brown button?");
    }
    
    [Test]
    public async Task SoManyButtons_StepTwo_Correct()
    {
        var engine = GetTarget();
        var room = Repository.GetLocation<MaintenanceRoom>();
        room.IsNoLongerDark = true;
        
        engine.Context.CurrentLocation = room;

        await engine.GetResponse("press button");
        var response = await engine.GetResponse("yellow one");

        response.Should().Contain("Click");
    }
    
    [Test]
    public async Task SoManyButtons_StepTwo_SomethingElse()
    {
        var engine = GetTarget();
        var room = Repository.GetLocation<MaintenanceRoom>();
        room.IsNoLongerDark = true;
        
        engine.Context.CurrentLocation = room;

        await engine.GetResponse("press button");
        var response = await engine.GetResponse("wait");

        response.Should().Contain("Time passes");
    }
    
    [Test]
    public async Task SoManyKnives()
    {
        var engine = GetTarget();
        var room = Repository.GetLocation<MaintenanceRoom>();
        room.IsNoLongerDark = true;
        engine.Context.ItemPlacedHere(Repository.GetItem<NastyKnife>());
        engine.Context.ItemPlacedHere(Repository.GetItem<RustyKnife>());
        
        engine.Context.CurrentLocation = room;
        
        var response = await engine.GetResponse("drop knife");

        response.Should().Contain("Do you mean the nasty knife or the rusty knife");
    }
    
    [Test]
    public async Task SoManyKnives_Dropped()
    {
        var engine = GetTarget();
        var room = Repository.GetLocation<MaintenanceRoom>();
        room.IsNoLongerDark = true;
        engine.Context.ItemPlacedHere(Repository.GetItem<NastyKnife>());
        engine.Context.ItemPlacedHere(Repository.GetItem<RustyKnife>());
        
        engine.Context.CurrentLocation = room;
        
        await engine.GetResponse("drop knife");
        var response = await engine.GetResponse("rusty");

        response.Should().Contain("Dropped");
    }
}