namespace UnitTests.SingleNounProcessors;

public class DisambiguationTests : EngineTestsBase
{
    [Test]
    public async Task SoManyButtons()
    {
        var engine = GetTarget();
        var room = Repository.GetLocation<MaintenanceRoom>();
        room.IsNoLongerDark = true;
        
        engine.Context.CurrentLocation = room;

        var response = await engine.GetResponse("press button");

        response.Should().Contain("Which button do you mean, the blue button, the red button, the yellow button or the brown button?");
    }
}