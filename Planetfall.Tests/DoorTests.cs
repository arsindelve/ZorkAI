using FluentAssertions;
using GameEngine;
using Model.AIGeneration;
using Model.AIGeneration.Requests;
using Moq;
using Planetfall.Item.Feinstein;
using Planetfall.Item.Kalamontee;
using Planetfall.Item.Kalamontee.Admin;
using Planetfall.Location.Feinstein;
using Planetfall.Location.Kalamontee;

namespace Planetfall.Tests;

/// <summary>
/// The station's plain room-to-room doors — the brig cell door, the mess and kitchen doors, the
/// conference room door, and the escape-pod bulkhead. They all answer the same three questions:
/// does the door block you when closed, does "enter door" walk its direction when open, and does the
/// room description agree with the door's actual state. Merged from the former BrigCellDoorTests /
/// MessKitchenDoorTests / ConferenceRoomDoorTests / BulkheadForceVerbTests.
///
/// (The padlocked mess-hall door keeps its own file — that one is a key-and-padlock puzzle, not a
/// plain door.)
/// </summary>
public class DoorTests : EngineTestsBase
{
    [Test]
    public async Task CannotOpenDoor_ReturnsNoWayJose()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<Brig>();

        var response = await target.GetResponse("open door");
        response.Should().Contain("No way, Jose.");
    }

    [Test]
    public async Task CannotOpenCellDoor_ReturnsNoWayJose()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<Brig>();

        var response = await target.GetResponse("open cell door");
        response.Should().Contain("No way, Jose.");
    }

    [Test]
    public void CellDoor_ExistsInBrig()
    {
        var brig = Repository.GetLocation<Brig>();
        brig.HasItem<CellDoor>().Should().BeTrue();
    }

    [Test]
    public void CellDoor_IsNotOpen()
    {
        var door = Repository.GetItem<CellDoor>();
        door.IsOpen.Should().BeFalse();
    }

    [Test]
    public async Task CannotGoSouth_DoorLocked()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<Brig>();

        var response = await target.GetResponse("S");
        response.Should().Contain("The cell door is locked");
    }

    // issue #262 / #266: the mess door (Mess Corridor -> Storage West) and the kitchen door (Mess Hall
    // -> Kitchen) are plain doors, each declared as the GatingItem of its passage, so "enter door"
    // resolves the noun to it and walks its direction. The kitchen door's card/auto-close mechanic is
    // unaffected — we only walk through when the door is already open. (These doors are one-way gated by
    // the game's map: the return trips, Kitchen->MessHall and StorageWest->MessCorridor, are ungated.)

    [Test]
    public async Task EnterDoor_FromMessCorridor_Blocked_WhenMessDoorClosed()
    {
        var target = GetTarget();
        var room = Repository.GetLocation<MessCorridor>();
        room.Init();
        Repository.GetItem<MessDoor>().IsOpen = false;
        target.Context.CurrentLocation = room;

        var response = await target.GetResponse("enter door");

        response.Should().Contain("The door is closed.");
        target.Context.CurrentLocation.Should().BeOfType<MessCorridor>();
    }

    [Test]
    public async Task EnterDoor_FromMessCorridor_GoesToStorageWest_WhenMessDoorOpen()
    {
        var target = GetTarget();
        var room = Repository.GetLocation<MessCorridor>();
        room.Init();
        Repository.GetItem<MessDoor>().IsOpen = true;
        target.Context.CurrentLocation = room;

        await target.GetResponse("enter door");

        target.Context.CurrentLocation.Should().BeOfType<StorageWest>();
    }

    [Test]
    public async Task EnterDoor_FromMessHall_GoesToKitchen_WhenKitchenDoorOpen()
    {
        var target = GetTarget();
        var room = Repository.GetLocation<MessHall>();
        room.Init();
        Repository.GetItem<KitchenDoor>().IsOpen = true;
        target.Context.CurrentLocation = room;

        await target.GetResponse("enter door");

        target.Context.CurrentLocation.Should().BeOfType<Kitchen>();
    }

    [Test]
    public async Task Look_MessHall_DescribesDoorAsOpen_WhenKitchenDoorOpen()
    {
        // Issue #438: the room description hardcoded "A door to the south is closed" and never consulted
        // the kitchen door's state, contradicting "examine kitchen door" and the fact that you can walk
        // south while it is open.
        var target = GetTarget();
        var room = Repository.GetLocation<MessHall>();
        room.Init();
        Repository.GetItem<KitchenDoor>().IsOpen = true;
        target.Context.CurrentLocation = room;

        var response = await target.GetResponse("look");

        response.Should().Contain("A door to the south is open");
        response.Should().NotContain("A door to the south is closed");
    }

    [Test]
    public async Task Look_MessHall_DescribesDoorAsClosed_WhenKitchenDoorClosed()
    {
        // Issue #438: the closed-state description must still render when the door is shut.
        var target = GetTarget();
        var room = Repository.GetLocation<MessHall>();
        room.Init();
        Repository.GetItem<KitchenDoor>().IsOpen = false;
        target.Context.CurrentLocation = room;

        var response = await target.GetResponse("look");

        response.Should().Contain("A door to the south is closed");
    }

    // issue #262 / #266: the conference room door is a plain room-to-room door, declared as the
    // GatingItem of the passage on each side (ConferenceRoom -> RecArea, and back), so "enter door"
    // resolves the noun to it and walks its direction.

    [Test]
    public async Task EnterDoor_FromConferenceRoom_Blocked_WhenClosed()
    {
        var target = GetTarget();
        var room = Repository.GetLocation<ConferenceRoom>();
        room.Init(); // place the conference room door
        Repository.GetItem<ConferenceRoomDoor>().IsOpen = false;
        target.Context.CurrentLocation = room;

        var response = await target.GetResponse("enter door");

        response.Should().Contain("The door is closed.");
        target.Context.CurrentLocation.Should().BeOfType<ConferenceRoom>();
    }

    [Test]
    public async Task EnterDoor_FromConferenceRoom_GoesToRecArea_WhenOpen()
    {
        var target = GetTarget();
        var room = Repository.GetLocation<ConferenceRoom>();
        room.Init();
        Repository.GetItem<ConferenceRoomDoor>().IsOpen = true;
        target.Context.CurrentLocation = room;

        await target.GetResponse("enter door");

        target.Context.CurrentLocation.Should().BeOfType<RecArea>();
    }

    [Test]
    public async Task EnterDoor_FromRecArea_GoesToConferenceRoom_WhenOpen()
    {
        var target = GetTarget();
        var room = Repository.GetLocation<RecArea>();
        room.Init(); // place the conference room door on this side too
        Repository.GetItem<ConferenceRoomDoor>().IsOpen = true;
        target.Context.CurrentLocation = room;

        await target.GetResponse("enter door");

        target.Context.CurrentLocation.Should().BeOfType<ConferenceRoom>();
    }

    // Regression tests for issue #282: on Deck Nine (the opening room), "force" verbs aimed at the
    // escape-pod bulkhead (push / kick / shake / hit / knock / bang) returned a completely empty
    // response — a blank line — while benign verbs (examine, open, close) worked.
    //
    // Root cause: the BulkheadDoor is a single shared instance seeded into BOTH Deck Nine and the
    // Escape Pod (PlanetfallGame.Init loads the pod last, so the door's CurrentLocation ends up being
    // the pod even while the player stands on Deck Nine). A force verb has no matching processor, so it
    // produces a NoVerbMatch and the engine asks GetGeneratedNoMatchingVerbResponse to narrate it. That
    // method re-resolved the noun with Repository.GetItemInScope, which fails its accessibility check
    // for the door (its CurrentLocation points at the pod, not Deck Nine) and returned null — and the
    // method short-circuited to string.Empty, printing a blank line instead of reaching the narrator.

    private const string NoEffectFlavor = "You give the bulkhead a solid whack, but nothing happens.";

    /// <summary>
    /// Reproduce the exact move-1 prod state: the player stands on Deck Nine, but the shared
    /// BulkheadDoor singleton's CurrentLocation is the Escape Pod (because PlanetfallGame.Init loads
    /// the pod after Deck Nine). The test harness's GetTarget re-inits Deck Nine last, so we re-run
    /// the pod's init to reach the same end-state, and assert the precondition so this stays honest.
    /// </summary>
    private GameEngine<PlanetfallGame, PlanetfallContext> ArrangeDeckNineWithSharedBulkhead()
    {
        var target = GetTarget();

        Repository.GetLocation<EscapePod>().Init();
        Repository.GetItem<BulkheadDoor>().CurrentLocation.Should().BeOfType<EscapePod>(
            "issue #282 only bites when the shared door's CurrentLocation is the pod, not Deck Nine");

        target.Context.CurrentLocation = Repository.GetLocation<DeckNine>();

        // Narrator ON: any no-effect narration returns this flavor line so we can prove the force
        // verb reached the narrator rather than short-circuiting to a blank line.
        Mock.Get(target.GenerationClient)
            .Setup(x => x.GenerateNarration(It.IsAny<Request>(), It.IsAny<string>()))
            .ReturnsAsync(NoEffectFlavor);

        return target;
    }

    // push/kick/shake are the force verbs the TestParser resolves to a plain SimpleIntent (verb +
    // "bulkhead"), which is the routing that reaches GetGeneratedNoMatchingVerbResponse. The bug's
    // other reported verbs (hit/knock/bang) normalize to the same NoVerbMatch path in prod via the
    // AI parser, which the deterministic TestParser doesn't model — so they aren't enumerated here,
    // but the single shared-routing fix covers all of them.
    [TestCase("push the bulkhead")]
    [TestCase("kick the bulkhead")]
    [TestCase("shake the bulkhead")]
    public async Task ForceVerbOnBulkhead_AtDeckNine_NeverReturnsABlankLine(string input)
    {
        var target = ArrangeDeckNineWithSharedBulkhead();

        var response = await target.GetResponse(input);

        response.Should().NotBeNullOrWhiteSpace(
            "a force verb on the bulkhead must fall through to the narrator, not print a blank line (#282)");
        response.Should().Contain(NoEffectFlavor);
    }

    // The two sanity checks below confirm the fix doesn't disturb the verbs that already worked:
    // these are handled by dedicated processors and never depended on GetItemInScope.

    [Test]
    public async Task ExamineBulkhead_StillWorks()
    {
        var target = ArrangeDeckNineWithSharedBulkhead();

        var examine = await target.GetResponse("examine bulkhead");

        examine.Should().Contain("nothing special about the narrow emergency bulkhead");
    }

    [Test]
    public async Task OpenBulkhead_StillWorks()
    {
        var target = ArrangeDeckNineWithSharedBulkhead();

        var open = await target.GetResponse("open the bulkhead");

        open.Should().Contain("Why open the door to the emergency escape pod");
    }
}
