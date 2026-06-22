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

    // Put the reservoir into its drained state — the only state in which the Reservoir room (and so the
    // stream above it) is reachable in normal play, and the only state in which standing in the
    // Reservoir is survivable. A full/filling reservoir drowns the player via Reservoir.Act(), which
    // fires during end-of-turn processing the moment we move into the Reservoir.
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
        DrainReservoir();
        var target = GetLitTargetAt<Reservoir>();

        var response = await target.GetResponse("up");

        response.Should().Contain("narrow beach");
    }

    [Test]
    public async Task Reservoir_West_ReachesStream_WhenDrained()
    {
        DrainReservoir();
        var target = GetLitTargetAt<Reservoir>();

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
        DrainReservoir();
        var target = GetLitTargetAt<InStream>();

        var response = await target.GetResponse("down");

        response.Should().Contain("mud");
    }

    [Test]
    public async Task Stream_East_ReachesReservoir()
    {
        DrainReservoir();
        var target = GetLitTargetAt<InStream>();

        var response = await target.GetResponse("east");

        response.Should().Contain("mud");
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
