using FluentAssertions;
using GameEngine;
using Model.Interface;
using ZorkOne.Item;
using ZorkOne.Location;

namespace ZorkOne.Tests.Places;

/// <summary>
/// The ledges you can fall off: Canyon View, Deep Canyon and the Dome Room. All three share the
/// "jump here and die / climb down safely" shape, and the two canyon rooms share the dam-and-
/// reservoir water state that colors their descriptions. Merged from the former CanyonViewTests /
/// DeepCanyonTests / DomeTests.
/// </summary>
[TestFixture]
public class HighPlacesTests : EngineTestsBase
{
    [Test]
    public async Task CanyonView_Jump_Fatal()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<CanyonView>();

        // Act
        var response = await target.GetResponse("jump");

        // Assert
        response.Should().Contain("lousy place to jump");
        response.Should().Contain("died");
        target.Context.DeathCounter.Should().Be(1);
    }

    [Test]
    public async Task CanyonView_Leap_Fatal()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<CanyonView>();

        // Act
        var response = await target.GetResponse("leap");

        // Assert
        response.Should().Contain("lousy place to jump");
        response.Should().Contain("died");
        target.Context.DeathCounter.Should().Be(1);
    }

    [Test]
    public async Task CanyonView_ClimbDown_NotFatal()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<CanyonView>();

        // Act - climbing down is the legitimate, safe descent into the canyon
        var response = await target.GetResponse("climb down");

        // Assert
        response.Should().NotContain("died");
        target.Context.DeathCounter.Should().Be(0);
        target.Context.CurrentLocation.Should().BeOfType<RockyLedge>();
    }

    // The Deep Canyon water-sound line varies with two pieces of world state in the original
    // (zork1/1actions.zil DEEP-CANYON-F, lines 1730-1745): GATES-OPEN and LOW-TIDE.
    // GATES-OPEN <-> Dam.SluiceGatesOpen, LOW-TIDE <-> ReservoirSouth.IsDrained.

    [Test]
    public async Task DeepCanyon_GatesOpen_NotDrained_LoudRoaringSound()
    {
        var target = GetLitTargetAt<DeepCanyon>();
        Repository.GetLocation<Dam>().SluiceGatesOpen = true;
        Repository.GetLocation<ReservoirSouth>().IsDrained = false;

        var response = await target.GetResponse("look");

        response.Should().Contain("loud roaring sound");
        response.Should().Contain("rushing water");
    }

    [Test]
    public async Task DeepCanyon_GatesClosed_Drained_NoWaterSound()
    {
        var target = GetLitTargetAt<DeepCanyon>();
        Repository.GetLocation<Dam>().SluiceGatesOpen = false;
        Repository.GetLocation<ReservoirSouth>().IsDrained = true;

        var response = await target.GetResponse("look");

        // Both water variants start with "You can hear ..." — asserting on "hear" is more robust
        // than a phrase match if the water copy ever changes but still mentions water.
        response.Should().NotContain("hear");
    }

    [Test]
    public async Task DeepCanyon_GatesOpen_Drained_FlowingWater()
    {
        var target = GetLitTargetAt<DeepCanyon>();
        Repository.GetLocation<Dam>().SluiceGatesOpen = true;
        Repository.GetLocation<ReservoirSouth>().IsDrained = true;

        var response = await target.GetResponse("look");

        response.Should().Contain("sound of flowing water");
    }

    [Test]
    public async Task DeepCanyon_DefaultState_FlowingWater()
    {
        // Game start: gates closed, reservoir full (not drained). This is the "other" bucket,
        // so the original (and current) behavior prints the flowing-water sentence.
        var target = GetLitTargetAt<DeepCanyon>();
        Repository.GetLocation<Dam>().SluiceGatesOpen = false;
        Repository.GetLocation<ReservoirSouth>().IsDrained = false;

        var response = await target.GetResponse("look");

        response.Should().Contain("sound of flowing water");
    }

    [Test]
    public async Task DomeRoom_TieRopeToRailing_DoNotHaveTheRope()
    {
        var target = GetLitTargetAt<DomeRoom>();
        ((ICanContainItems)target.Context.CurrentLocation).ItemPlacedHere(Repository.GetItem<Rope>());

        // Act
        var response = await target.GetResponse("tie rope to railing");

        // Assert
        response.Should().Contain("don't have the rope");
        Repository.GetItem<Rope>().TiedToRailing.Should().BeFalse();
    }

    [Test]
    public async Task DomeRoom_TieRopeToRailing_Success()
    {
        var target = GetLitTargetAt<DomeRoom>();
        target.Context.Take(Repository.GetItem<Rope>());

        // Act
        var response = await target.GetResponse("tie rope to railing");

        // Assert
        response.Should().Contain("drops over the side");
        Repository.GetItem<Rope>().TiedToRailing.Should().BeTrue();
    }

    [Test]
    public async Task DomeRoom_TakingTheRopeUntiesIt()
    {
        var target = GetLitTargetAt<DomeRoom>();
        target.Context.Take(Repository.GetItem<Rope>());

        // Act
        await target.GetResponse("tie rope to railing");
        target.Context.HasItem<Rope>().Should().BeFalse();
        var response = await target.GetResponse("take rope");

        // Assert
        response.Should().Contain("Taken");
        Repository.GetItem<Rope>().TiedToRailing.Should().BeFalse();
        target.Context.HasItem<Rope>().Should().BeTrue();
    }

    [Test]
    public async Task DomeRoom_Jump_Fatal()
    {
        var target = GetLitTargetAt<DomeRoom>();
        target.Context.Take(Repository.GetItem<Rope>());

        // Act
        var response = await target.GetResponse("jump");

        // Assert
        response.Should().Contain("died");
        target.Context.DeathCounter.Should().Be(1);
    }

    [Test]
    public async Task DomeRoom_Leap_Fatal()
    {
        // "leap" is a synonym for "jump" (Verbs.JumpVerbs) and should be just as fatal here.
        var target = GetLitTargetAt<DomeRoom>();
        target.Context.Take(Repository.GetItem<Rope>());

        // Act
        var response = await target.GetResponse("leap");

        // Assert
        response.Should().Contain("died");
        target.Context.DeathCounter.Should().Be(1);
    }
}
