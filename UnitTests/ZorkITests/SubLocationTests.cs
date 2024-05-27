namespace UnitTests.ZorkITests;

public class SubLocationTests : EngineTestsBase
{
    [Test]
    public async Task GetInTheBoat()
    {
        var target = GetTarget();

        target.Context.CurrentLocation = Repository.GetLocation<DamBase>();
        var boat = Repository.GetItem<PileOfPlastic>();
        boat.IsInflated = true;
 
        var response = await target.GetResponse("get in the boat");
        
        response.Should().Contain("You are now in the magic boat.");
        target.Context.CurrentLocation.SubLocation.Should().NotBeNull();
    }
    
    [Test]
    public async Task GetOutOfTheBoat_NotInIt()
    {
        var target = GetTarget();

        target.Context.CurrentLocation = Repository.GetLocation<DamBase>();
        var boat = Repository.GetItem<PileOfPlastic>();
        boat.IsInflated = true;
 
        var response = await target.GetResponse("get out of the boat");
        
        response.Should().Contain("You're not in the boat.");
        target.Context.CurrentLocation.SubLocation.Should().BeNull();
    }
    
    [Test]
    public async Task GetInTheBoat_AlreadyThere()
    {
        var target = GetTarget();

        target.Context.CurrentLocation = Repository.GetLocation<DamBase>();
        var boat = Repository.GetItem<PileOfPlastic>();
        boat.IsInflated = true;
 
        await target.GetResponse("get in the boat");
        var response = await target.GetResponse("get in the boat");
        
        response.Should().Contain("You're already in the boat");
        target.Context.CurrentLocation.SubLocation.Should().NotBeNull();
    }
    
    [Test]
    public async Task InTheBoat_Look()
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
    public async Task GetOutOfTheBoat_OnLand()
    {
        var target = GetTarget();

        target.Context.CurrentLocation = Repository.GetLocation<DamBase>();
        var boat = Repository.GetItem<PileOfPlastic>();
        boat.IsInflated = true;
        target.Context.CurrentLocation.SubLocation = boat;
        
        var response = await target.GetResponse("get out of the boat");
        
        response.Should().Contain("You are on your own feet again");
        target.Context.CurrentLocation.SubLocation.Should().BeNull();
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
    
    [Test]
    public async Task GetOutOfTheBoat_InRiver()
    {
        var target = GetTarget();

        target.Context.CurrentLocation = Repository.GetLocation<FrigidRiverOne>();
        var boat = Repository.GetItem<PileOfPlastic>();
        boat.IsInflated = true;
        target.Context.CurrentLocation.SubLocation = boat;
        
        var response = await target.GetResponse("get out of the boat");
        
        response.Should().Contain("You realize that getting out here would be fatal");
        target.Context.CurrentLocation.SubLocation.Should().NotBeNull();
    }
}