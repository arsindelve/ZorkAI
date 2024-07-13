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
}