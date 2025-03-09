using FluentAssertions;
using GameEngine;
using Planetfall.Item.Kalamontee.Admin;
using Planetfall.Location.Shuttle;

namespace Planetfall.Tests;

public class ShuttleTests : EngineTestsBase
{
    [Test]
    public async Task PushLeverWhenNotActive()
    {
        GameEngine<PlanetfallGame, PlanetfallContext> target = GetTarget();
        StartHere<AlfieControlEast>();

        var response = await target.GetResponse("push lever");

        response.Should().Contain("A recorded voice says \"Shuttle controls are not currently activated.\"");
    }
    
    [Test]
    public async Task PullLeverWhenNotActive()
    {
        GameEngine<PlanetfallGame, PlanetfallContext> target = GetTarget();
        StartHere<AlfieControlEast>();

        var response = await target.GetResponse("push lever");

        response.Should().Contain("A recorded voice says \"Shuttle controls are not currently activated.\"");
    }
    
    [Test]
    public async Task Look_WhileInStation_Outbound()
    {
        GameEngine<PlanetfallGame, PlanetfallContext> target = GetTarget();
        StartHere<AlfieControlEast>();

        var response = await target.GetResponse("look");

        response.Should().Contain("Through the cabin window you can see parallel rails running along the floor of a long tunnel, vanishing in the distance");
    }
    
    [Test]
    public async Task Look_WhileInStation_Inbound()
    {
        GameEngine<PlanetfallGame, PlanetfallContext> target = GetTarget();
        StartHere<AlfieControlWest>();

        var response = await target.GetResponse("look");

        response.Should().Contain("Through the cabin window you can see a featureless concrete wall");
    }
    
    [Test]
    public async Task Activate_WhileInStation_Inbound()
    {
        GameEngine<PlanetfallGame, PlanetfallContext> target = GetTarget();
        GetItem<ShuttleAccessCard>();
        StartHere<AlfieControlWest>();

        var response = await target.GetResponse("slide shuttle access card through slot");

        response.Should().Contain("A recorded voice says \"Use other control cabin. Control activation overridden.\"");
    }
    
    [Test]
    public async Task Activate_WhileInStation_Outbound()
    {
        GameEngine<PlanetfallGame, PlanetfallContext> target = GetTarget();
        GetItem<ShuttleAccessCard>();
        StartHere<AlfieControlEast>();

        var response = await target.GetResponse("slide shuttle access card through slot");

        response.Should().Contain("A recording of a deep male voice says \"Shuttle controls activated.\"");
        GetLocation<AlfieControlEast>().Activated.Should().BeTrue();
    }
}