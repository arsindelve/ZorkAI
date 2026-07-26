using FluentAssertions;
using GameEngine;
using ZorkOne.Item;
using ZorkOne.Location;

namespace ZorkOne.Tests.Places;

/// <summary>
/// Everything that touches the reservoir/stream/river water: the two reservoir shores and their
/// drown barrier, the optional stream branch, and the river's local-global water object at Shore and
/// Sandy Beach. Merged from the former ReservoirTests / StreamTests / ShoreTests, which all shared
/// the same lit-lantern setup and the same ReservoirSouth water-state flags.
/// </summary>
[TestFixture]
public class WaterTests : EngineTestsBase
{
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

    // Issue #87: the two reservoir shores must be symmetric. In the original game both shores only
    // let you step onto the middle RESERVOIR room when the water is low:
    //   zork1/1dungeon.zil:1769  RESERVOIR-SOUTH  (NORTH TO RESERVOIR IF LOW-TIDE ELSE "You would drown.")
    //   zork1/1dungeon.zil:1796  RESERVOIR-NORTH  (SOUTH TO RESERVOIR IF LOW-TIDE ELSE "You would drown.")
    // The shores never kill you; they gate the crossing with the "You would drown." barrier. Only the
    // middle RESERVOIR room drowns you (when the fill daemon fires while you stand there). The bug was
    // that ReservoirNorth's south exit was unconditional, so a full reservoir let you walk in and die.

    [Test]
    public async Task ReservoirNorth_GoSouth_WhenFull_IsBlockedAndDoesNotDrown()
    {
        var target = GetLitTargetAt<ReservoirNorth>();
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
        var target = GetLitTargetAt<ReservoirNorth>();
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
        var target = GetLitTargetAt<ReservoirSouth>();
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
        var target = GetLitTargetAt<ReservoirSouth>();
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
        var target = GetLitTargetAt<ReservoirNorth>();
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
        var target = GetLitTargetAt<ReservoirNorth>();
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
        var target = GetLitTargetAt<ReservoirNorth>();
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
        var target = GetLitTargetAt<ReservoirNorth>();
        var south = Repository.GetLocation<ReservoirSouth>();
        south.IsDraining = true;
        south.IsFull = south.IsDrained = south.IsFilling = false;

        var response = await target.GetResponse("look");

        response.Should().Contain("dropping");
    }

    // Issue #210: the optional stream branch off the Reservoir was missing — Stream View and the
    // water room IN-STREAM (displayed "Stream"), plus the four exits that reach them. Verified against
    // zork1/1dungeon.zil: STREAM-VIEW :1805, IN-STREAM :1819, RESERVOIR-SOUTH WEST :1775,
    // RESERVOIR UP/WEST :1788-1789.

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

    // Issue #337: Shore and Sandy Beach both expose the river in their room descriptions, but neither
    // location wired up the original ZIL "local-global" water/river objects
    // (zork1/1dungeon.zil: both SHORE and SANDY-BEACH declare `(GLOBAL GLOBAL-WATER RIVER)`), so
    // swim/drink/take fell through to the engine's generic no-op responses instead of the original
    // handlers.

    [TestCase("swim")]
    [TestCase("swim in river")]
    [TestCase("swim in water")]
    [TestCase("bathe")]
    public async Task Swim_AtShore_GivesTheOriginalRefusal(string input)
    {
        var target = GetTarget();
        StartHere<Shore>();

        var response = await target.GetResponse(input);

        response.Should().Contain("Swimming isn't usually allowed in the dungeon.");
    }

    [Test]
    public async Task DrinkWater_AtShore_QuenchesThirst()
    {
        var target = GetTarget();
        StartHere<Shore>();

        var response = await target.GetResponse("drink water");

        response.Should().Contain("Thank you very much. I was rather thirsty (from all this talking, probably).");
    }

    [Test]
    public async Task TakeWater_AtShore_SlipsThroughFingers()
    {
        var target = GetTarget();
        StartHere<Shore>();

        var response = await target.GetResponse("take water");

        response.Should().Contain("The water slips through your fingers.");
    }

    [Test]
    public async Task ExamineRiver_AtShore_DescribesIt()
    {
        var target = GetTarget();
        StartHere<Shore>();

        var response = await target.GetResponse("examine river");

        response.Should().Contain("The river is wide, swift, and cold.");
    }

    [Test]
    public async Task Swim_AtSandyBeach_GivesTheOriginalRefusal_EvenInTheDark()
    {
        // Sandy Beach is a DarkLocation, but RespondToSpecificLocationInteraction (like the
        // existing "launch" handling here) runs before any darkness gate, so this must work
        // without a light source too.
        var target = GetTarget();
        StartHere<SandyBeach>();

        var response = await target.GetResponse("swim in river");

        response.Should().Contain("Swimming isn't usually allowed in the dungeon.");
    }

    [Test]
    public async Task DrinkWater_AtSandyBeach_QuenchesThirst()
    {
        var target = GetTarget();
        StartHere<SandyBeach>();
        Take<Lantern>();
        GetItem<Lantern>().IsOn = true;

        var response = await target.GetResponse("drink water");

        response.Should().Contain("Thank you very much. I was rather thirsty (from all this talking, probably).");
    }
}
