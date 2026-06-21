using FluentAssertions;
using GameEngine;
using ZorkOne.Item;
using ZorkOne.Location;

namespace ZorkOne.Tests.Places;

[TestFixture]
public class DeepCanyonTests : EngineTestsBase
{
    // The Deep Canyon water-sound line varies with two pieces of world state in the original
    // (zork1/1actions.zil DEEP-CANYON-F, lines 1730-1745): GATES-OPEN and LOW-TIDE.
    // GATES-OPEN <-> Dam.SluiceGatesOpen, LOW-TIDE <-> ReservoirSouth.IsDrained.

    private GameEngine<ZorkI, ZorkIContext> GetLitTargetInDeepCanyon()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<DeepCanyon>();
        target.Context.Take(Repository.GetItem<Lantern>());
        Repository.GetItem<Lantern>().IsOn = true;
        return target;
    }

    [Test]
    public async Task GatesOpen_NotDrained_LoudRoaringSound()
    {
        var target = GetLitTargetInDeepCanyon();
        Repository.GetLocation<Dam>().SluiceGatesOpen = true;
        Repository.GetLocation<ReservoirSouth>().IsDrained = false;

        var response = await target.GetResponse("look");

        response.Should().Contain("loud roaring sound");
        response.Should().Contain("rushing water");
    }

    [Test]
    public async Task GatesClosed_Drained_NoWaterSound()
    {
        var target = GetLitTargetInDeepCanyon();
        Repository.GetLocation<Dam>().SluiceGatesOpen = false;
        Repository.GetLocation<ReservoirSouth>().IsDrained = true;

        var response = await target.GetResponse("look");

        // Both water variants start with "You can hear ..." — asserting on "hear" is more robust
        // than a phrase match if the water copy ever changes but still mentions water.
        response.Should().NotContain("hear");
    }

    [Test]
    public async Task GatesOpen_Drained_FlowingWater()
    {
        var target = GetLitTargetInDeepCanyon();
        Repository.GetLocation<Dam>().SluiceGatesOpen = true;
        Repository.GetLocation<ReservoirSouth>().IsDrained = true;

        var response = await target.GetResponse("look");

        response.Should().Contain("sound of flowing water");
    }

    [Test]
    public async Task DefaultState_FlowingWater()
    {
        // Game start: gates closed, reservoir full (not drained). This is the "other" bucket,
        // so the original (and current) behavior prints the flowing-water sentence.
        var target = GetLitTargetInDeepCanyon();
        Repository.GetLocation<Dam>().SluiceGatesOpen = false;
        Repository.GetLocation<ReservoirSouth>().IsDrained = false;

        var response = await target.GetResponse("look");

        response.Should().Contain("sound of flowing water");
    }
}
