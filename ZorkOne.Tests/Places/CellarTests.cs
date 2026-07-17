using FluentAssertions;
using GameEngine;
using UnitTests;
using ZorkOne.Item;
using ZorkOne.Location;

namespace ZorkOne.Tests.Places;

/// <summary>
/// Issue #386: in the Cellar the ramp is the (unclimbable) West exit, but the parser maps the verb
/// "climb" (and "go up") to Direction.Up - the trap door - so a ramp command wrongly returned "The
/// trap door is closed." instead of the ramp's own failure message. The original SLIDE-FUNCTION
/// (zork1/1actions.zil:3086) routes any climb of the slide/ramp/chute in the Cellar to the West exit.
/// </summary>
public class CellarTests : EngineTestsBase
{
    private GameEngine.GameEngine<ZorkI, ZorkIContext> ArriveInLitCellar()
    {
        var target = GetTarget();
        var lamp = Repository.GetItem<Lantern>();
        lamp.IsOn = true;
        target.Context.Take(lamp);
        target.Context.CurrentLocation = Repository.GetLocation<Cellar>();
        return target;
    }

    [Test]
    public async Task ClimbRamp_InCellar_GivesRampMessage_NotTrapDoor()
    {
        var target = ArriveInLitCellar();

        var response = await target.GetResponse("climb ramp");

        response.Should().Contain("ramp");
        response.Should().NotContain("trap door");
        target.Context.CurrentLocation.Should().BeOfType<Cellar>();
    }

    [Test]
    public async Task GoUpRamp_InCellar_GivesRampMessage_NotTrapDoor()
    {
        var target = ArriveInLitCellar();

        var response = await target.GetResponse("go up ramp");

        response.Should().Contain("ramp");
        response.Should().NotContain("trap door");
        target.Context.CurrentLocation.Should().BeOfType<Cellar>();
    }

    [Test]
    public async Task ClimbUpRamp_InCellar_GivesRampMessage_NotTrapDoor()
    {
        var target = ArriveInLitCellar();

        var response = await target.GetResponse("climb up ramp");

        response.Should().Contain("ramp");
        response.Should().NotContain("trap door");
    }

    [Test]
    public async Task PlainClimbUp_InCellar_StillReferencesTrapDoor()
    {
        // Regression guard: only ramp/slide/chute commands reroute to West. A bare "climb"/"climb up"
        // with no ramp object still means Up (the trap door), exactly as before the fix.
        var target = ArriveInLitCellar();

        var response = await target.GetResponse("climb up");

        response.Should().Contain("trap door");
    }
}
