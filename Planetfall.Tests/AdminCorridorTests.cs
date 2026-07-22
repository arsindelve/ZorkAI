using FluentAssertions;
using Planetfall.Item.Feinstein;
using Planetfall.Item.Kalamontee;
using Planetfall.Item.Kalamontee.Mech;
using Planetfall.Location.Kalamontee;
using Planetfall.Location.Kalamontee.Admin;
using Planetfall.Location.Kalamontee.Mech;

namespace Planetfall.Tests;

public class AdminCorridorTests : EngineTestsBase
{
    [Test]
    public async Task UseLadderOnRift_Extended_SpansRift()
    {
        var target = GetTarget();
        StartHere<AdminCorridor>();
        var ladder = GetItem<Ladder>();
        ladder.IsExtended = true;
        GetLocation<AdminCorridor>().ItemPlacedHere(ladder);

        var response = await target.GetResponse("use ladder on rift");

        response.Should().Contain("swings out across the rift");
        response.Should().Contain("spanning the precipice");
        ladder.IsAcrossRift.Should().BeTrue();
    }

    [Test]
    public async Task UseLadderOnChasm_Extended_SpansRift()
    {
        var target = GetTarget();
        StartHere<AdminCorridor>();
        var ladder = GetItem<Ladder>();
        ladder.IsExtended = true;
        GetLocation<AdminCorridor>().ItemPlacedHere(ladder);

        var response = await target.GetResponse("use ladder on chasm");

        response.Should().Contain("swings out across the rift");
        ladder.IsAcrossRift.Should().BeTrue();
    }

    [Test]
    public async Task UseLadderOnRift_NotExtended_FallsIntoRift()
    {
        var target = GetTarget();
        StartHere<AdminCorridor>();
        var ladder = GetItem<Ladder>();
        ladder.IsExtended = false;
        GetLocation<AdminCorridor>().ItemPlacedHere(ladder);

        var response = await target.GetResponse("use ladder on rift");

        response.Should().Contain("far too short");
        response.Should().Contain("plunges into the rift");
        response.Should().Contain("lost forever");
        ladder.CurrentLocation.Should().BeNull();
    }

    [Test]
    public async Task PlaceLadderAcrossRift_StillWorks()
    {
        var target = GetTarget();
        StartHere<AdminCorridor>();
        var ladder = GetItem<Ladder>();
        ladder.IsExtended = true;
        GetLocation<AdminCorridor>().ItemPlacedHere(ladder);

        var response = await target.GetResponse("place ladder across rift");

        response.Should().Contain("swings out across the rift");
        ladder.IsAcrossRift.Should().BeTrue();
    }

    [Test]
    public async Task CanGoNorth_AfterLadderSpansRift()
    {
        var target = GetTarget();
        StartHere<AdminCorridor>();
        var ladder = GetItem<Ladder>();
        ladder.IsExtended = true;
        ladder.IsAcrossRift = true;

        var response = await target.GetResponse("n");

        response.Should().Contain("swaying ladder");
        Context.CurrentLocation.Should().BeOfType<AdminCorridorNorth>();
    }

    [Test]
    public async Task CannotGoNorth_WithoutLadder()
    {
        var target = GetTarget();
        StartHere<AdminCorridor>();

        var response = await target.GetResponse("n");

        response.Should().Contain("too wide to jump");
        Context.CurrentLocation.Should().BeOfType<AdminCorridor>();
    }

    // Issue #369: "jump rift"/"jump into rift"/"jump over rift" were being misrouted to the
    // Direction.N movement failure ("too wide to jump across") instead of reaching the deliberate
    // death scene already written in RiftLocationBase.RespondToSimpleInteraction. A bare "jump" with
    // no rift noun must still fall through to normal (non-fatal) handling.
    [TestFixture]
    public class JumpIntoRift : AdminCorridorTests
    {
        [Test]
        public async Task JumpRift_TheBareNoun_DiesInTheRift()
        {
            var target = GetTarget();
            StartHere<AdminCorridor>();

            var response = await target.GetResponse("jump rift");

            response.Should().Contain("You have died");
            response.Should().Contain("sharp and nasty rocks");
        }

        [Test]
        public async Task JumpIntoRift_DiesInTheRift()
        {
            var target = GetTarget();
            StartHere<AdminCorridor>();

            var response = await target.GetResponse("jump into rift");

            response.Should().Contain("You have died");
        }

        [Test]
        public async Task JumpOverRift_DiesInTheRift()
        {
            var target = GetTarget();
            StartHere<AdminCorridor>();

            var response = await target.GetResponse("jump over rift");

            response.Should().Contain("You have died");
        }

        [Test]
        public async Task JumpIntoTheChasm_SynonymNounDiesInTheRift()
        {
            var target = GetTarget();
            StartHere<AdminCorridor>();

            var response = await target.GetResponse("jump into the chasm");

            response.Should().Contain("You have died");
        }

        [Test]
        public async Task JumpFromAdminCorridorNorth_DiesInTheRift()
        {
            var target = GetTarget();
            StartHere<AdminCorridorNorth>();

            var response = await target.GetResponse("jump into rift");

            response.Should().Contain("You have died");
        }

        [Test]
        public async Task BareJump_NoRiftNoun_DoesNotDie()
        {
            var target = GetTarget();
            StartHere<AdminCorridor>();

            var response = await target.GetResponse("jump");

            response.Should().NotContain("You have died");
        }
    }

    // Issue #297, Divergence A: once the ladder has plunged into the rift it is out of scope
    // entirely (not carried, not in any room, not spanning). The handler used to match on the
    // command text alone and still narrate destroying a ladder that no longer exists.
    [Test]
    public async Task PlaceLadderAcrossRift_LadderAlreadyLostFromGame_DoesNotNarrateLoss()
    {
        var target = GetTarget();
        StartHere<AdminCorridor>();
        var ladder = GetItem<Ladder>();
        ladder.IsExtended = false;
        ladder.IsAcrossRift = false;
        ladder.CurrentLocation = null;

        var response = await target.GetResponse("place ladder across rift");

        response.Should().NotContain("lost forever");
        response.Should().NotContain("plunges into the rift");
        // The command had no effect: the ladder is still gone and certainly didn't span the rift.
        ladder.CurrentLocation.Should().BeNull();
        ladder.IsAcrossRift.Should().BeFalse();
    }

    [Test]
    public async Task PutLadderAcrossRift_LadderAlreadyLostFromGame_DoesNotNarrateLoss()
    {
        var target = GetTarget();
        StartHere<AdminCorridor>();
        var ladder = GetItem<Ladder>();
        ladder.IsExtended = false;
        ladder.IsAcrossRift = false;
        ladder.CurrentLocation = null;

        var response = await target.GetResponse("put ladder across rift");

        response.Should().NotContain("lost forever");
        response.Should().NotContain("plunges into the rift");
        // The command had no effect: the ladder is still gone and certainly didn't span the rift.
        ladder.CurrentLocation.Should().BeNull();
        ladder.IsAcrossRift.Should().BeFalse();
    }

    // Issue #297 follow-up: the ladder must actually be in scope (here or carried) to bridge the
    // rift. Left in another room, the handler must not silently span the rift with an absent ladder.
    [Test]
    public async Task PlaceLadderAcrossRift_LadderInAnotherRoom_DoesNotSpanRift()
    {
        var target = GetTarget();
        StartHere<AdminCorridor>();
        var ladder = GetItem<Ladder>();
        ladder.IsExtended = true;
        ladder.IsAcrossRift = false;
        // The (extended) ladder is sitting in a different room — not at the rift, not carried.
        GetLocation<StorageWest>().ItemPlacedHere(ladder);

        var response = await target.GetResponse("place ladder across rift");

        response.Should().NotContain("swings out across the rift");
        response.Should().NotContain("spanning the precipice");
        ladder.IsAcrossRift.Should().BeFalse();
        // The ladder stays put; it was never moved to the rift.
        ladder.CurrentLocation.Should().Be(GetLocation<StorageWest>());
    }

    // Issue #297, Divergence B: placing the ladder when it already spans the rift used to
    // re-narrate success and add the same instance to AdminCorridorNorth.Items a second time.
    // The original prints "The ladder already spans the rift." and does nothing.
    [Test]
    public async Task PlaceLadderAcrossRift_AlreadySpanning_SaysAlreadySpansAndDoesNotDuplicate()
    {
        var target = GetTarget();
        StartHere<AdminCorridor>();
        var ladder = GetItem<Ladder>();
        ladder.IsExtended = true;
        ladder.IsAcrossRift = true;
        // The real spanning state: present in the corridor and mirrored into the north room.
        GetLocation<AdminCorridor>().ItemPlacedHere(ladder);
        GetLocation<AdminCorridorNorth>().Items.Add(ladder);

        var response = await target.GetResponse("place ladder across rift");

        response.Should().Contain("already spans the rift");
        GetLocation<AdminCorridorNorth>().Items.Count(i => i is Ladder).Should().Be(1);
    }

    // Issue #429: the generic "throw <thing> into rift" handler in RiftLocationBase resolved the
    // thrown object with a GLOBAL Repository.GetItem lookup that ignores scope/state. Two divergences
    // followed, both reproduced on prod. These mirror the sibling ladder guards (#297) above.
    [TestFixture]
    public class ThrowIntoRift : AdminCorridorTests
    {
        // Divergence A: throwing an item that lives in a completely different part of the complex
        // still "succeeded" — the global lookup found the singleton regardless of location and
        // deleted it (CurrentLocation = null). Because critical-path items (e.g. the laser) can be
        // destroyed this way without the player ever touching them, this is a softlock vector.
        [Test]
        public async Task ThrowIntoRift_ItemInAnotherRoom_IsNotDestroyedAndNoLossNarrated()
        {
            var target = GetTarget();
            StartHere<AdminCorridorNorth>();
            var laser = GetItem<Laser>();
            // The laser sits in the Mech area — not carried, not here at the rift.
            GetLocation<ToolRoom>().ItemPlacedHere(laser);

            var response = await target.GetResponse("throw laser into rift");

            response.Should().NotContain("sails gracefully into the rift");
            // The out-of-scope laser must not be deleted from the game.
            laser.CurrentLocation.Should().Be(GetLocation<ToolRoom>());
        }

        // Divergence B: once a thrown item's CurrentLocation is null, the command still matched and
        // the global lookup still returned the singleton, so the loss was narrated a second time.
        [Test]
        public async Task ThrowIntoRift_ReThrowingAlreadyLostItem_DoesNotReNarrateLoss()
        {
            var target = GetTarget();
            StartHere<AdminCorridorNorth>();
            var diary = Take<Diary>();

            // First throw: the held diary really does sail into the rift and leaves play.
            var first = await target.GetResponse("throw diary into rift");
            first.Should().Contain("sails gracefully into the rift");
            diary.CurrentLocation.Should().BeNull();
            Context.Items.Should().NotContain(diary);

            // Second throw of the now-gone diary must not re-narrate success.
            var second = await target.GetResponse("throw diary into rift");
            second.Should().NotContain("sails gracefully into the rift");
        }
    }
}
