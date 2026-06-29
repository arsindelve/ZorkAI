using FluentAssertions;
using GameEngine;
using ZorkOne.Location;

namespace ZorkOne.Tests.Places;

[TestFixture]
public class AragainFallsTests : EngineTestsBase
{
    [Test]
    public async Task LookUnderRainbow_DescribesRiver()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<AragainFalls>();

        var response = await target.GetResponse("look under rainbow");

        response.Should().Contain("The Frigid River flows under the rainbow.");
    }

    [Test]
    public async Task ThroughRainbow_WhenRainbowIsNotSolid_Refuses()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<AragainFalls>();

        var response = await target.GetResponse("through rainbow");

        response.Should().Contain("Can you walk on water vapor?");
    }

    [Test]
    public async Task GoThroughRainbow_WhenRainbowIsNotSolid_Refuses()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<AragainFalls>();

        var response = await target.GetResponse("go through rainbow");

        response.Should().Contain("Can you walk on water vapor?");
    }

    [Test]
    public async Task CrossRainbow_WhenRainbowIsNotSolid_Refuses()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<AragainFalls>();

        var response = await target.GetResponse("cross rainbow");

        response.Should().Contain("Can you walk on water vapor?");
    }
}
