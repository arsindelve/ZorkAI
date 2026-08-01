using FluentAssertions;
using GameEngine;
using Model;
using ZorkOne.Item;
using ZorkOne.Location;
using ZorkOne.Location.CoalMineLocation;
using ZorkOne.Location.MazeLocation;

namespace ZorkOne.Tests.Places;

/// <summary>
/// Getting *through* the underground passages: the squeeze-limited timber/drafty crawl, the maze
/// one-way drops, the Cellar ramp, and the passage room naming. Merged from the former
/// TimberRoomTests / MazeTests / CellarTests / NorthSouthPassageTests, each of which held between
/// one and six tests on the same theme.
/// </summary>
[TestFixture]
public class UndergroundPassagesTests : EngineTestsBase
{
    [SetUp]
    public void SetUp()
    {
        Repository.Reset();
    }

    [TearDown]
    public void TearDown()
    {
        Repository.Reset();
    }

    [Test]
    public void NorthSouthPassage_NameHasNoTrailingWhitespace()
    {
        var location = Repository.GetLocation<NorthSouthPassage>();

        location.Name.Should().Be("North-South Passage");
        location.Name.Should().Be(location.Name.TrimEnd());
    }

    // Issue #473 (same lifecycle-override bug as the Planetfall connector rooms, found via the
    // repo-wide grep the issue suggested): MazeEleven, entered from MazeNine, returned the
    // "You won't be able to get back up..." transition text directly instead of prepending it to
    // base.BeforeEnterLocation(...). base is the only place VisitCount is incremented (and
    // OnFirstTimeEnterLocation fires); in Brief mode LookProcessor shows the full room description
    // only when VisitCount == 1, so a lit first entry via this path dropped the maze description.
    [Test]
    public async Task EnterMazeElevenFromMazeNine_FirstVisit_ShowsMazeDescriptionAndIncrementsVisitCount()
    {
        // The maze is dark; carry a lit lantern so the room description (not the darkness text) shows.
        var target = GetLitTargetAt<MazeNine>();
        target.Context.Verbosity = Verbosity.Brief;

        var response = await target.GetResponse("down");

        target.Context.CurrentLocation.Should().BeOfType<MazeEleven>();
        // The transition warning must accompany, not replace, the first-visit maze description.
        response.Should().Contain("You won't be able to get back up to the tunnel");
        response.Should().Contain("twisty little passages, all alike");
        Repository.GetLocation<MazeEleven>().VisitCount.Should().Be(1);
    }

    [Test]
    public async Task TimberRoom_WestExit_WithItems_ReturnsNarrowPassageMessage()
    {
        var target = GetTarget();
        Repository.GetLocation<TimberRoom>().IsNoLongerDark = true;
        target.Context.CurrentLocation = Repository.GetLocation<TimberRoom>();
        target.Context.Take(Repository.GetItem<Lantern>());

        var response = await target.GetResponse("W");
        Console.WriteLine(response);

        response.Should().NotBeNullOrEmpty();
        response.Should().Contain("squeeze");
    }

    [Test]
    public async Task TimberRoom_WestExit_WithNoItems_EntersDraftyRoom()
    {
        var target = GetTarget();
        Repository.GetLocation<TimberRoom>().IsNoLongerDark = true;
        Repository.GetLocation<DraftyRoom>().IsNoLongerDark = true;
        target.Context.CurrentLocation = Repository.GetLocation<TimberRoom>();

        var response = await target.GetResponse("W");
        Console.WriteLine(response);

        target.Context.CurrentLocation.Should().BeOfType<DraftyRoom>();
    }

    [Test]
    public async Task DraftyRoom_EastExit_WithItems_ReturnsNarrowPassageMessage()
    {
        var target = GetTarget();
        Repository.GetLocation<DraftyRoom>().IsNoLongerDark = true;
        target.Context.CurrentLocation = Repository.GetLocation<DraftyRoom>();
        target.Context.Take(Repository.GetItem<Lantern>());

        var response = await target.GetResponse("E");
        Console.WriteLine(response);

        response.Should().NotBeNullOrEmpty();
        response.Should().Contain("squeeze");
    }

    [Test]
    public async Task DraftyRoom_EastExit_WithNoItems_EntersTimberRoom()
    {
        var target = GetTarget();
        Repository.GetLocation<DraftyRoom>().IsNoLongerDark = true;
        Repository.GetLocation<TimberRoom>().IsNoLongerDark = true;
        target.Context.CurrentLocation = Repository.GetLocation<DraftyRoom>();

        var response = await target.GetResponse("E");
        Console.WriteLine(response);

        target.Context.CurrentLocation.Should().BeOfType<TimberRoom>();
    }

    // Issue #386: in the Cellar the ramp is the (unclimbable) West exit, but the parser maps the verb
    // "climb" (and "go up") to Direction.Up - the trap door - so a ramp command wrongly returned "The
    // trap door is closed." instead of the ramp's own failure message. The original SLIDE-FUNCTION
    // (zork1/1actions.zil:3086) routes any climb of the slide/ramp/chute in the Cellar to the West exit.

    [Test]
    public async Task ClimbRamp_InCellar_GivesRampMessage_NotTrapDoor()
    {
        var target = GetLitTargetAt<Cellar>();

        var response = await target.GetResponse("climb ramp");

        response.Should().Contain("ramp");
        response.Should().NotContain("trap door");
        target.Context.CurrentLocation.Should().BeOfType<Cellar>();
    }

    [Test]
    public async Task GoUpRamp_InCellar_GivesRampMessage_NotTrapDoor()
    {
        var target = GetLitTargetAt<Cellar>();

        var response = await target.GetResponse("go up ramp");

        response.Should().Contain("ramp");
        response.Should().NotContain("trap door");
        target.Context.CurrentLocation.Should().BeOfType<Cellar>();
    }

    [Test]
    public async Task ClimbUpRamp_InCellar_GivesRampMessage_NotTrapDoor()
    {
        var target = GetLitTargetAt<Cellar>();

        var response = await target.GetResponse("climb up ramp");

        response.Should().Contain("ramp");
        response.Should().NotContain("trap door");
    }

    [Test]
    public async Task ClimbSlide_InCellar_GivesRampMessage_NotTrapDoor()
    {
        // The ZIL slide object's synonyms are chute/ramp/slide (zork1/1dungeon.zil:265), so a climb
        // of the "slide" must reroute to West just like "ramp" does.
        var target = GetLitTargetAt<Cellar>();

        var response = await target.GetResponse("climb slide");

        response.Should().Contain("ramp");
        response.Should().NotContain("trap door");
        target.Context.CurrentLocation.Should().BeOfType<Cellar>();
    }

    [Test]
    public async Task AscendMetalRamp_InCellar_GivesRampMessage_NotTrapDoor()
    {
        // Exercises the "ascend" verb and a ZIL adjective ("metal") - substring matching should
        // handle "ascend the steep metal ramp" without enumerating every phrasing.
        var target = GetLitTargetAt<Cellar>();

        var response = await target.GetResponse("ascend the metal ramp");

        response.Should().Contain("ramp");
        response.Should().NotContain("trap door");
        target.Context.CurrentLocation.Should().BeOfType<Cellar>();
    }

    [Test]
    public async Task PlainClimbUp_InCellar_StillReferencesTrapDoor()
    {
        // Regression guard: only ramp/slide/chute commands reroute to West. A bare "climb"/"climb up"
        // with no ramp object still means Up (the trap door), exactly as before the fix.
        var target = GetLitTargetAt<Cellar>();

        var response = await target.GetResponse("climb up");

        response.Should().Contain("trap door");
    }
}
