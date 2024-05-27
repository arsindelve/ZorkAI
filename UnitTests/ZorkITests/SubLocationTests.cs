namespace UnitTests.ZorkITests;

public class SubLocationTests : EngineTestsBase
{
    [Test]
    public async Task GetInTheBoat_Look()
    {
        var target = GetTarget();

        target.Context.CurrentLocation = Repository.GetLocation<DamBase>();
        var boat = Repository.GetItem<PileOfPlastic>();
        boat.IsInflated = true;
        target.Context.CurrentLocation.SubLocation = boat;
        
        var response = await target.GetResponse("look");
        
        response.Should().Contain("in the magic boat");

    }
    
    [Test]
    public async Task NotInTheBoat_Look()
    {
        var target = GetTarget();

        target.Context.CurrentLocation = Repository.GetLocation<DamBase>();
        var boat = Repository.GetItem<PileOfPlastic>();
        boat.IsInflated = true;
        target.Context.CurrentLocation.SubLocation = null;
        
        var response = await target.GetResponse("look");
        
        response.Should().NotContain("in the magic boat");

    }
}