using FluentAssertions;
using GameEngine;
using Model.Location;
using ZorkOne.Item;
using ZorkOne.Location;

namespace ZorkOne.Tests.Places;

[TestFixture]
public class StreamTests : EngineTestsBase
{
    // Issue #210: the optional stream branch off the Reservoir was missing — Stream View and the
    // water room IN-STREAM (displayed "Stream"), plus the four exits that reach them. Verified against
    // zork1/1dungeon.zil: STREAM-VIEW :1805, IN-STREAM :1819, RESERVOIR-SOUTH WEST :1775,
    // RESERVOIR UP/WEST :1788-1789.

    private GameEngine<ZorkI, ZorkIContext> GetLitTargetAt<T>() where T : class, ILocation, new()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<T>();
        target.Context.Take(Repository.GetItem<Lantern>());
        Repository.GetItem<Lantern>().IsOn = true;
        return target;
    }

    // Put the reservoir into its drained, survivable state. MUST be called AFTER GetLitTargetAt — that
    // helper runs GetTarget(), which calls Repository.Reset() and rebuilds every location singleton
    // fresh (ReservoirSouth.Init() sets IsFull = true). Calling it earlier would be silently undone by
    // that reset, leaving the reservoir full so the player drowns the moment they enter it.
    private static void DrainReservoir()
    {
        var south = Repository.GetLocation<ReservoirSouth>();
        south.IsDrained = true;
        south.IsFull = false;
        south.IsFilling = false;
        south.IsDraining = false;
    }

    [Test]
    public async Task ReservoirSouth_West_ReachesStreamView()
    {
        // The room's own description promises "a path along the stream to the east or west"; before the
        // fix, WEST was a dead pointer.
        var target = GetLitTargetAt<ReservoirSouth>();

        var response = await target.GetResponse("west");

        response.Should().Contain("Stream View");
        response.Should().Contain("gently flowing stream");
    }

    [Test]
    public async Task StreamView_West_TooSmallToEnter()
    {
        var target = GetLitTargetAt<StreamView>();

        var response = await target.GetResponse("west");

        response.Should().Contain("too small for you to enter");
    }

    [Test]
    public async Task StreamView_East_ReturnsToReservoirSouth()
    {
        var target = GetLitTargetAt<StreamView>();

        var response = await target.GetResponse("east");

        response.Should().Contain("Reservoir South");
    }

    [Test]
    public async Task Reservoir_Up_ReachesStream_WhenDrained()
    {
        var target = GetLitTargetAt<Reservoir>();
        DrainReservoir();

        var response = await target.GetResponse("up");

        response.Should().Contain("narrow beach");
    }

    [Test]
    public async Task Reservoir_West_ReachesStream_WhenDrained()
    {
        var target = GetLitTargetAt<Reservoir>();
        DrainReservoir();

        var response = await target.GetResponse("west");

        response.Should().Contain("narrow beach");
    }

    [Test]
    public async Task Stream_Up_ChannelTooNarrow()
    {
        var target = GetLitTargetAt<InStream>();

        var response = await target.GetResponse("up");

        response.Should().Contain("channel is too narrow");
    }

    [Test]
    public async Task Stream_West_ChannelTooNarrow()
    {
        var target = GetLitTargetAt<InStream>();

        var response = await target.GetResponse("west");

        response.Should().Contain("channel is too narrow");
    }

    [Test]
    public async Task Stream_Down_ReachesReservoir()
    {
        var target = GetLitTargetAt<InStream>();
        DrainReservoir();

        var response = await target.GetResponse("down");

        response.Should().Contain("mud");
        response.Should().NotContain("rising river"); // a drained reservoir is survivable — no drowning
    }

    [Test]
    public async Task Stream_East_ReachesReservoir()
    {
        var target = GetLitTargetAt<InStream>();
        DrainReservoir();

        var response = await target.GetResponse("east");

        response.Should().Contain("mud");
        response.Should().NotContain("rising river"); // a drained reservoir is survivable — no drowning
    }

    [Test]
    public async Task Stream_Down_IntoFullReservoir_Drowns()
    {
        // InStream's DOWN/EAST exits carry no "you would drown" pre-check (unlike ReservoirSouth's
        // North). The kill is enforced one layer down by Reservoir.Act() when the reservoir is full, so
        // re-entering a full reservoir from the stream is fatal. This is the load-bearing safety net
        // that lets InStream itself carry no drowning logic (issue #210). Set the full state AFTER
        // GetLitTargetAt so it survives the Reset() inside GetTarget().
        var target = GetLitTargetAt<InStream>();
        var south = Repository.GetLocation<ReservoirSouth>();
        south.IsFull = true;
        south.IsDrained = false;
        south.IsFilling = false;
        south.IsDraining = false;

        var response = await target.GetResponse("down");

        response.Should().Contain("rising river");
    }

    [Test]
    public async Task Stream_Land_ReachesStreamView()
    {
        // IN-STREAM's LAND exit. Direction has no "Land" member, so InStream handles the original LAND
        // command (and synonyms) and routes to the beach at Stream View.
        var target = GetLitTargetAt<InStream>();

        var response = await target.GetResponse("land");

        response.Should().Contain("Stream View");
    }
}
