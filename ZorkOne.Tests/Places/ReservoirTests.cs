using FluentAssertions;
using GameEngine;
using Model.Location;
using ZorkOne.Item;
using ZorkOne.Location;

namespace ZorkOne.Tests.Places;

[TestFixture]
public class ReservoirTests : EngineTestsBase
{
    // Issue #87: the two reservoir shores must be symmetric. In the original game both shores only
    // let you step onto the middle RESERVOIR room when the water is low:
    //   zork1/1dungeon.zil:1769  RESERVOIR-SOUTH  (NORTH TO RESERVOIR IF LOW-TIDE ELSE "You would drown.")
    //   zork1/1dungeon.zil:1796  RESERVOIR-NORTH  (SOUTH TO RESERVOIR IF LOW-TIDE ELSE "You would drown.")
    // The shores never kill you; they gate the crossing with the "You would drown." barrier. Only the
    // middle RESERVOIR room drowns you (when the fill daemon fires while you stand there). The bug was
    // that ReservoirNorth's south exit was unconditional, so a full reservoir let you walk in and die.

    private GameEngine<ZorkI, ZorkIContext> GetLitTarget<TLocation>() where TLocation : class, ILocation, new()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<TLocation>();
        target.Context.Take(Repository.GetItem<Lantern>());
        Repository.GetItem<Lantern>().IsOn = true;
        return target;
    }

    [Test]
    public async Task ReservoirNorth_GoSouth_WhenFull_IsBlockedAndDoesNotDrown()
    {
        var target = GetLitTarget<ReservoirNorth>();
        var south = Repository.GetLocation<ReservoirSouth>();
        south.IsFull = true;
        south.IsDrained = south.IsFilling = south.IsDraining = false;

        var response = await target.GetResponse("go south");

        response.Should().Contain("You would drown.");
        target.Context.CurrentLocation.Should().Be(Repository.GetLocation<ReservoirNorth>());
        target.Context.DeathCounter.Should().Be(0);
    }

    [Test]
    public async Task ReservoirNorth_GoSouth_WhenDrained_Succeeds()
    {
        var target = GetLitTarget<ReservoirNorth>();
        var south = Repository.GetLocation<ReservoirSouth>();
        south.IsDrained = true;
        south.IsFull = south.IsFilling = south.IsDraining = false;

        await target.GetResponse("go south");

        target.Context.CurrentLocation.Should().Be(Repository.GetLocation<Reservoir>());
        target.Context.DeathCounter.Should().Be(0);
    }

    [Test]
    public async Task ReservoirSouth_GoNorth_WhenFull_IsBlocked()
    {
        var target = GetLitTarget<ReservoirSouth>();
        var south = Repository.GetLocation<ReservoirSouth>();
        south.IsFull = true;
        south.IsDrained = south.IsFilling = south.IsDraining = false;

        var response = await target.GetResponse("go north");

        response.Should().Contain("You would drown.");
        target.Context.CurrentLocation.Should().Be(Repository.GetLocation<ReservoirSouth>());
        target.Context.DeathCounter.Should().Be(0);
    }

    [Test]
    public async Task ReservoirSouth_GoNorth_WhenDrained_Succeeds()
    {
        var target = GetLitTarget<ReservoirSouth>();
        var south = Repository.GetLocation<ReservoirSouth>();
        south.IsDrained = true;
        south.IsFull = south.IsFilling = south.IsDraining = false;

        await target.GetResponse("go north");

        target.Context.CurrentLocation.Should().Be(Repository.GetLocation<Reservoir>());
        target.Context.DeathCounter.Should().Be(0);
    }

    // Issue #87 follow-up: Reservoir North's room description must track the water state, just as the
    // south shore's does. It is reachable in every fill state (cross while drained, close the gates and
    // it fills around you, etc.), so a static "the water level is lowered / a wide stream" description
    // contradicts the "You would drown." barrier whenever the reservoir is actually full or filling.

    [Test]
    public async Task ReservoirNorth_Look_WhenFull_DescribesUncrossableWater()
    {
        var target = GetLitTarget<ReservoirNorth>();
        var south = Repository.GetLocation<ReservoirSouth>();
        south.IsFull = true;
        south.IsDrained = south.IsFilling = south.IsDraining = false;

        var response = await target.GetResponse("look");

        response.Should().NotContain("water level lowered");
        response.Should().NotContain("wide stream");
        response.Should().Contain("too deep");
    }

    [Test]
    public async Task ReservoirNorth_Look_WhenDrained_DescribesLowWater()
    {
        var target = GetLitTarget<ReservoirNorth>();
        var south = Repository.GetLocation<ReservoirSouth>();
        south.IsDrained = true;
        south.IsFull = south.IsFilling = south.IsDraining = false;

        var response = await target.GetResponse("look");

        response.Should().Contain("with the water level lowered");
        response.Should().Contain("wide stream");
    }

    [Test]
    public async Task ReservoirNorth_Look_WhenFilling_WarnsWaterRising()
    {
        var target = GetLitTarget<ReservoirNorth>();
        var south = Repository.GetLocation<ReservoirSouth>();
        // Issue #233 invariant: a refill stays low-tide (IsDrained) until the reservoir is full again,
        // so a realistic "filling" state has BOTH IsFilling and IsDrained set. The rising-water text must
        // win and the drained "wide stream" line must be suppressed.
        south.IsFilling = true;
        south.IsDrained = true;
        south.IsFull = south.IsDraining = false;

        var response = await target.GetResponse("look");

        response.Should().Contain("rising");
        response.Should().Contain("impossible to cross");
        response.Should().NotContain("wide stream");
    }

    [Test]
    public async Task ReservoirNorth_Look_WhenDraining_NotesWaterDropping()
    {
        var target = GetLitTarget<ReservoirNorth>();
        var south = Repository.GetLocation<ReservoirSouth>();
        south.IsDraining = true;
        south.IsFull = south.IsDrained = south.IsFilling = false;

        var response = await target.GetResponse("look");

        response.Should().Contain("dropping");
    }
}
