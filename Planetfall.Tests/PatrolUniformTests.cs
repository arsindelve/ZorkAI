using FluentAssertions;
using Planetfall.Item.Feinstein;
using Planetfall.Location.Feinstein;
using Planetfall.Location.Lawanda.Lab;

namespace Planetfall.Tests;

public class PatrolUniformTests : EngineTestsBase
{
    [Test]
    public async Task PatrolUniform_Examine()
    {
        var target = GetTarget();
        StartHere<DeckNine>();
        Take<PatrolUniform>();
        
        var response = await target.GetResponse("examine uniform");
        response.Should().Contain("insects");
    }
    
    [Test]
    public async Task PatrolUniform_OpenPocket()
    {
        var target = GetTarget();
        StartHere<DeckNine>();
        Take<PatrolUniform>();
        
        var response = await target.GetResponse("open pocket");
        response.Should().Contain("There's no way to open or close the pocket of the Patrol uniform");
    }
    
    [Test]
    public async Task PatrolUniform_ClosePocket()
    {
        var target = GetTarget();
        StartHere<DeckNine>();
        Take<PatrolUniform>();
        
        var response = await target.GetResponse("close pocket");
        response.Should().Contain("There's no way to open or close the pocket of the Patrol uniform");
    }
    
    [Test]
    public async Task Uniform_Disambiguation()
    {
        var target = GetTarget();
        StartHere<LabStorage>();
        Take<PatrolUniform>();
        
        var response = await target.GetResponse("examine uniform");
        response.Should().Contain("Do you mean the patrol uniform or the lab uniform");
    }
}