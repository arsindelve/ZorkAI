using FluentAssertions;
using Planetfall.Item.Feinstein;
using Planetfall.Item.Kalamontee.Admin;
using Planetfall.Item.Kalamontee.Mech;
using Planetfall.Location.Kalamontee.Admin;
using Planetfall.Location.Kalamontee.Mech;

namespace Planetfall.Tests;

public class AdminCorridorSouthTests : EngineTestsBase
{
    [Test]
    public async Task UseMagnetOnCrevice_RetrievesKey()
    {
        var target = GetTarget();
        StartHere<AdminCorridorSouth>();
        Take<Magnet>();

        var response = await target.GetResponse("use magnet on crevice");

        response.Should().Contain("a piece of metal leaps from the crevice");
        response.Should().Contain("steel key");
        Context.HasItem<Key>().Should().BeTrue();
        GetLocation<AdminCorridorSouth>().HasTakenTheKey.Should().BeTrue();
    }

    [Test]
    public async Task UseMagnetOnKey_RetrievesKey()
    {
        var target = GetTarget();
        StartHere<AdminCorridorSouth>();
        Take<Magnet>();

        var response = await target.GetResponse("use magnet on key");

        response.Should().Contain("a piece of metal leaps from the crevice");
        response.Should().Contain("steel key");
        Context.HasItem<Key>().Should().BeTrue();
    }

    [Test]
    public async Task UseMagnetOnFloor_RetrievesKey()
    {
        var target = GetTarget();
        StartHere<AdminCorridorSouth>();
        Take<Magnet>();

        var response = await target.GetResponse("use magnet on floor");

        response.Should().Contain("a piece of metal leaps from the crevice");
        response.Should().Contain("steel key");
        Context.HasItem<Key>().Should().BeTrue();
    }

    [Test]
    public async Task UseBarOnCrevice_RetrievesKey()
    {
        var target = GetTarget();
        StartHere<AdminCorridorSouth>();
        Take<Magnet>();

        var response = await target.GetResponse("use bar on crevice");

        response.Should().Contain("a piece of metal leaps from the crevice");
        response.Should().Contain("steel key");
        Context.HasItem<Key>().Should().BeTrue();
    }

    [Test]
    public async Task UseMagnetOnCrevice_WithoutMagnet_Fails()
    {
        var target = GetTarget();
        StartHere<AdminCorridorSouth>();

        await target.GetResponse("use magnet on crevice");

        Context.HasItem<Key>().Should().BeFalse();
        GetLocation<AdminCorridorSouth>().HasTakenTheKey.Should().BeFalse();
    }

    // Issue #436 — the multi-noun analog of the examine catch-all fixed in #291. While the magnet
    // is set down in this room (not held), the "you don't have the curved metal bar" hint must fire
    // ONLY for genuine magnet/key-fishing attempts, not for every two-noun command. A magnet-unrelated
    // command like "put brush in uniform" must fall through to normal handling.
    [Test]
    public async Task MagnetUnrelatedTwoNounCommand_WithMagnetOnFloorHere_DoesNotClaimMissingBar()
    {
        var target = GetTarget();
        StartHere<AdminCorridorSouth>();

        // Magnet is in this room but NOT in the player's inventory.
        GetLocation<AdminCorridorSouth>().ItemPlacedHere(GetItem<Magnet>());
        Context.HasItem<Magnet>().Should().BeFalse();
        GetItem<Magnet>().CurrentLocation.Should().Be(GetLocation<AdminCorridorSouth>());

        var response = await target.GetResponse("put brush in uniform");

        response.Should().NotContain("You don't have the curved metal bar");
    }

    // Issue #436 — the flip side: a real fishing attempt while the magnet is on the floor here must
    // still get the "you don't have the bar" hint (and must not silently retrieve the key).
    [Test]
    public async Task FishingAttempt_WithMagnetOnFloorHere_StillGivesBarHint()
    {
        var target = GetTarget();
        StartHere<AdminCorridorSouth>();

        GetLocation<AdminCorridorSouth>().ItemPlacedHere(GetItem<Magnet>());
        Context.HasItem<Magnet>().Should().BeFalse();

        var response = await target.GetResponse("get key with magnet");

        response.Should().Contain("You don't have the curved metal bar");
        Context.HasItem<Key>().Should().BeFalse();
        GetLocation<AdminCorridorSouth>().HasTakenTheKey.Should().BeFalse();
    }

    [Test]
    public async Task UseMagnetOnCrevice_AlreadyHasKey_NothingHappens()
    {
        var target = GetTarget();
        StartHere<AdminCorridorSouth>();
        Take<Magnet>();
        GetLocation<AdminCorridorSouth>().HasTakenTheKey = true;

        var response = await target.GetResponse("use magnet on crevice");

        response.Should().Contain("Nothing interesting happens");
    }

    [Test]
    public async Task PutMagnetOnCrevice_StillWorks()
    {
        var target = GetTarget();
        StartHere<AdminCorridorSouth>();
        Take<Magnet>();

        var response = await target.GetResponse("put magnet on crevice");

        response.Should().Contain("a piece of metal leaps from the crevice");
        response.Should().Contain("steel key");
        Context.HasItem<Key>().Should().BeTrue();
    }

    // Issue #298 — Framing A: the magnet as the tool, the key as the object retrieved "with" it.
    // This is the original game's canonical solve (compone.zil KEY-F: TAKE/ZATTRACT the key,
    // PRSI = MAGNET). The port never wired it up, so it fell through to the AI narrator, which
    // falsely insisted the steel key was "non-magnetic".
    [Test]
    public async Task GetKeyWithMagnet_RetrievesKey()
    {
        var target = GetTarget();
        StartHere<AdminCorridorSouth>();
        Take<Magnet>();

        var response = await target.GetResponse("get key with magnet");

        response.Should().Contain("a piece of metal leaps from the crevice");
        response.Should().Contain("steel key");
        Context.HasItem<Key>().Should().BeTrue();
        GetLocation<AdminCorridorSouth>().HasTakenTheKey.Should().BeTrue();
    }

    [Test]
    public async Task SnatchKeyWithMagnet_RetrievesKey()
    {
        // Review follow-up to issue #406: the retrieval framing's verb list is now built on
        // Verbs.TakeVerbs instead of a partial hand-rolled copy that omitted "snatch", "acquire",
        // "hold", and "carry" - those phrasings fell through to the AI narrator, which improvised
        // a refusal contradicting the authored success text.
        var target = GetTarget();
        StartHere<AdminCorridorSouth>();
        Take<Magnet>();

        var response = await target.GetResponse("snatch key with magnet");

        response.Should().Contain("a piece of metal leaps from the crevice");
        response.Should().Contain("steel key");
        Context.HasItem<Key>().Should().BeTrue();
        GetLocation<AdminCorridorSouth>().HasTakenTheKey.Should().BeTrue();
    }

    [Test]
    public async Task TakeKeyWithMagnet_RetrievesKey()
    {
        var target = GetTarget();
        StartHere<AdminCorridorSouth>();
        Take<Magnet>();

        var response = await target.GetResponse("take key with magnet");

        response.Should().Contain("a piece of metal leaps from the crevice");
        response.Should().Contain("steel key");
        Context.HasItem<Key>().Should().BeTrue();
    }

    [Test]
    public async Task AttractKeyWithMagnet_RetrievesKey()
    {
        var target = GetTarget();
        StartHere<AdminCorridorSouth>();
        Take<Magnet>();

        var response = await target.GetResponse("attract key with magnet");

        response.Should().Contain("a piece of metal leaps from the crevice");
        response.Should().Contain("steel key");
        Context.HasItem<Key>().Should().BeTrue();
    }

    [Test]
    public async Task FishKeyOutWithMagnet_RetrievesKey()
    {
        var target = GetTarget();
        StartHere<AdminCorridorSouth>();
        Take<Magnet>();

        var response = await target.GetResponse("fish key out with magnet");

        response.Should().Contain("a piece of metal leaps from the crevice");
        response.Should().Contain("steel key");
        Context.HasItem<Key>().Should().BeTrue();
    }

    // Issue #298 — Framing B: the magnet as the subject, lowered INTO the crack. The original only
    // whitelisted on/over/beside/next to; the natural "in/into/lower/drop" phrasings all failed.
    [Test]
    public async Task PutMagnetInCrevice_RetrievesKey()
    {
        var target = GetTarget();
        StartHere<AdminCorridorSouth>();
        Take<Magnet>();

        var response = await target.GetResponse("put magnet in crevice");

        response.Should().Contain("a piece of metal leaps from the crevice");
        response.Should().Contain("steel key");
        Context.HasItem<Key>().Should().BeTrue();
    }

    [Test]
    public async Task PutMagnetInHole_RetrievesKey()
    {
        var target = GetTarget();
        StartHere<AdminCorridorSouth>();
        Take<Magnet>();

        var response = await target.GetResponse("put magnet in hole");

        response.Should().Contain("a piece of metal leaps from the crevice");
        response.Should().Contain("steel key");
        Context.HasItem<Key>().Should().BeTrue();
    }

    [Test]
    public async Task LowerMagnetIntoCrevice_RetrievesKey()
    {
        var target = GetTarget();
        StartHere<AdminCorridorSouth>();
        Take<Magnet>();

        var response = await target.GetResponse("lower magnet into crevice");

        response.Should().Contain("a piece of metal leaps from the crevice");
        response.Should().Contain("steel key");
        Context.HasItem<Key>().Should().BeTrue();
    }

    // Issue #298 — a multi-noun "drop magnet in crevice" must solve the puzzle, not silently dump
    // the magnet on the floor and ignore "in crevice".
    [Test]
    public async Task DropMagnetInCrevice_RetrievesKey()
    {
        var target = GetTarget();
        StartHere<AdminCorridorSouth>();
        Take<Magnet>();

        var response = await target.GetResponse("drop magnet in crevice");

        response.Should().Contain("a piece of metal leaps from the crevice");
        response.Should().Contain("steel key");
        Context.HasItem<Key>().Should().BeTrue();
    }

    // "drop" only solves when the magnet goes INTO the crack. Dropping it on the floor is a plain
    // drop, not a solve — otherwise the handler would swallow the bar and (once the key is taken)
    // answer "Nothing interesting happens" instead of letting the magnet hit the floor.
    [Test]
    public async Task DropMagnetOnFloor_DoesNotSolvePuzzle()
    {
        var target = GetTarget();
        StartHere<AdminCorridorSouth>();
        Take<Magnet>();

        var response = await target.GetResponse("drop magnet on floor");

        response.Should().NotContain("steel key");
        Context.HasItem<Key>().Should().BeFalse();
        GetLocation<AdminCorridorSouth>().HasTakenTheKey.Should().BeFalse();
    }

    // Framing A matches the key's own nouns, not the crack/floor synonyms, so a nonsense
    // "lift floor with magnet" no longer counts as retrieving the key.
    [Test]
    public async Task LiftFloorWithMagnet_DoesNotSolvePuzzle()
    {
        var target = GetTarget();
        StartHere<AdminCorridorSouth>();
        Take<Magnet>();

        var response = await target.GetResponse("lift floor with magnet");

        response.Should().NotContain("steel key");
        Context.HasItem<Key>().Should().BeFalse();
        GetLocation<AdminCorridorSouth>().HasTakenTheKey.Should().BeFalse();
    }

    // Framing A still accepts the key's synonyms ("shiny object"), not only the literal word "key".
    [Test]
    public async Task GetShinyObjectWithMagnet_RetrievesKey()
    {
        var target = GetTarget();
        StartHere<AdminCorridorSouth>();
        Take<Magnet>();

        var response = await target.GetResponse("get shiny object with magnet");

        response.Should().Contain("a piece of metal leaps from the crevice");
        response.Should().Contain("steel key");
        Context.HasItem<Key>().Should().BeTrue();
    }

    // "hole" is one of the crevice's ZIL synonyms, so examining it reveals the key just like
    // "examine crevice" / "examine crack" do.
    [Test]
    public async Task ExamineHole_RevealsKey()
    {
        var target = GetTarget();
        StartHere<AdminCorridorSouth>();

        var response = await target.GetResponse("examine hole");

        response.Should().Contain("steel key");
        GetLocation<AdminCorridorSouth>().HasSeenTheLight.Should().BeTrue();
    }

    // ---- Issue #298 follow-up: broaden the accepted vocabulary across all six phrasing groups ----

    // Group 1 (more "retrieve the key WITH the magnet" verbs) + Group 2 (to/toward/towards).
    [TestCase("scoop key out with magnet")]
    [TestCase("catch key with magnet")]
    [TestCase("collect key with magnet")]
    [TestCase("recover key with magnet")]
    [TestCase("yank key out with magnet")]
    [TestCase("drag key out with magnet")]
    [TestCase("reel key in with magnet")]
    [TestCase("pluck key with magnet")]
    [TestCase("nab key with magnet")]
    [TestCase("haul key up with magnet")]
    [TestCase("obtain key with magnet")]
    [TestCase("draw key out with magnet")]
    [TestCase("pull key to magnet")]
    [TestCase("draw key toward magnet")]
    [TestCase("pull key towards magnet")]
    public async Task FramingA_VariedVerbsAndPrepositions_RetrievesKey(string command)
    {
        var target = GetTarget();
        StartHere<AdminCorridorSouth>();
        Take<Magnet>();

        var response = await target.GetResponse(command);

        response.Should().Contain("a piece of metal leaps from the crevice");
        response.Should().Contain("steel key");
        Context.HasItem<Key>().Should().BeTrue();
    }

    // Group 3 — more "place/lower the magnet at the crack" verbs.
    [TestCase("set magnet on crevice")]
    [TestCase("lay magnet over crack")]
    [TestCase("rest magnet on crevice")]
    [TestCase("slide magnet into crack")]
    [TestCase("slip magnet into crevice")]
    [TestCase("feed magnet into crevice")]
    [TestCase("thread magnet into crack")]
    [TestCase("hang magnet in crevice")]
    [TestCase("push magnet into crack")]
    [TestCase("poke magnet into crack")]
    [TestCase("move magnet to crevice")]
    [TestCase("wave magnet over crevice")]
    [TestCase("swing magnet over crevice")]
    public async Task FramingB_VariedVerbs_RetrievesKey(string command)
    {
        var target = GetTarget();
        StartHere<AdminCorridorSouth>();
        Take<Magnet>();

        var response = await target.GetResponse(command);

        response.Should().Contain("a piece of metal leaps from the crevice");
        response.Should().Contain("steel key");
        Context.HasItem<Key>().Should().BeTrue();
    }

    // Group 4 — more prepositions for lowering the magnet toward the crack.
    [TestCase("hold magnet near crevice")]
    [TestCase("put magnet by crevice")]
    [TestCase("hold magnet against crack")]
    [TestCase("put magnet inside crevice")]
    [TestCase("lower magnet to crevice")]
    [TestCase("lower magnet toward crevice")]
    [TestCase("lower magnet towards crack")]
    public async Task FramingB_VariedPrepositions_RetrievesKey(string command)
    {
        var target = GetTarget();
        StartHere<AdminCorridorSouth>();
        Take<Magnet>();

        var response = await target.GetResponse(command);

        response.Should().Contain("a piece of metal leaps from the crevice");
        response.Should().Contain("steel key");
        Context.HasItem<Key>().Should().BeTrue();
    }

    // Group 5 — the magnetic-attraction framing: aim/point/touch the magnet at or to the key.
    [TestCase("aim magnet at key")]
    [TestCase("point magnet at crevice")]
    [TestCase("aim magnet at crack")]
    [TestCase("touch magnet to key")]
    [TestCase("connect magnet to key")]
    [TestCase("attach magnet to key")]
    [TestCase("point magnet toward crevice")]
    public async Task AimTouchFraming_RetrievesKey(string command)
    {
        var target = GetTarget();
        StartHere<AdminCorridorSouth>();
        Take<Magnet>();

        var response = await target.GetResponse(command);

        response.Should().Contain("a piece of metal leaps from the crevice");
        response.Should().Contain("steel key");
        Context.HasItem<Key>().Should().BeTrue();
    }

    // Group 6 — broaden "use" beyond "on", and add "apply".
    [TestCase("use magnet in crevice")]
    [TestCase("use bar in crack")]
    [TestCase("use magnet into crevice")]
    [TestCase("apply magnet to key")]
    [TestCase("apply magnet to crevice")]
    [TestCase("apply magnet on crevice")]
    [TestCase("use magnet against crack")]
    [TestCase("use magnet near crevice")]
    public async Task UseOrApplyMagnet_RetrievesKey(string command)
    {
        var target = GetTarget();
        StartHere<AdminCorridorSouth>();
        Take<Magnet>();

        var response = await target.GetResponse(command);

        response.Should().Contain("a piece of metal leaps from the crevice");
        response.Should().Contain("steel key");
        Context.HasItem<Key>().Should().BeTrue();
    }

    // Key is hidden in the crevice under dust; examining it pre-discovery must not reveal it.
    // base.RespondToSimpleInteraction finds Key in room Items, so the location must intercept
    // the noun before falling through to base.
    [Test]
    public async Task ExamineKey_BeforeDiscovery_DoesNotRevealKey()
    {
        var target = GetTarget();
        StartHere<AdminCorridorSouth>();

        var response = await target.GetResponse("examine key");

        response.Should().NotContain("steel key");
        response.Should().NotContain("nothing special");
    }

    [Test]
    public async Task ExamineSteelKey_BeforeDiscovery_DoesNotRevealKey()
    {
        var target = GetTarget();
        StartHere<AdminCorridorSouth>();

        var response = await target.GetResponse("examine steel key");

        response.Should().NotContain("steel key");
        response.Should().NotContain("nothing special");
    }

    // Room description must not mention the shiny object once the key is no longer in the crevice.
    // GetContextBasedDescription must gate on !HasTakenTheKey, not just HasSeenTheLight.
    [Test]
    public async Task Look_AfterKeyTaken_DoesNotShowShinyObjectHint()
    {
        var target = GetTarget();
        StartHere<AdminCorridorSouth>();
        GetLocation<AdminCorridorSouth>().HasSeenTheLight = true;
        GetLocation<AdminCorridorSouth>().HasTakenTheKey = true;

        var response = await target.GetResponse("look");

        response.Should().NotContain("shiny object");
    }

    [Test]
    public async Task ExamineChronometer_InAdminCorridorSouth_ReturnsChronometerDescription()
    {
        var target = GetTarget();
        StartHere<AdminCorridorSouth>();
        Take<Chronometer>();

        var response = await target.GetResponse("examine chronometer");

        response.Should().Contain("wrist chronometer");
        response.Should().NotContain("crevice");
    }

    // Issue #291 secondary: examining the crevice after key is taken should not mention the key
    [Test]
    public async Task ExamineCrevice_AfterKeyTaken_DoesNotMentionKey()
    {
        var target = GetTarget();
        StartHere<AdminCorridorSouth>();
        GetLocation<AdminCorridorSouth>().HasTakenTheKey = true;

        var response = await target.GetResponse("examine crevice");

        response.Should().NotContain("steel key");
    }

    // Issue #211 - I-MAGNET (compone.zil:1595-1612): carrying the magnet scrambles carried access
    // cards' magnetic stripes, one per turn (in acquisition order, not the original's fixed type
    // priority - see Magnet.Act for why).
    [Test]
    public async Task TakeMagnet_WhileHoldingAccessCard_ScramblesTheCard()
    {
        var target = GetTarget();
        StartHere<ToolRoom>();
        Take<KitchenAccessCard>();

        await target.GetResponse("take bar");

        GetItem<KitchenAccessCard>().Scrambled.Should().BeTrue();
    }

    [Test]
    public async Task NotCarryingTheMagnet_DoesNotScrambleAccessCard()
    {
        var target = GetTarget();
        StartHere<ToolRoom>();
        Take<KitchenAccessCard>();

        // The magnet is left behind in the tool room - never taken.
        await target.GetResponse("look");

        GetItem<KitchenAccessCard>().Scrambled.Should().BeFalse();
    }

    [Test]
    public async Task HoldingMagnet_ScramblesOneCardPerTurn_InAcquisitionOrder()
    {
        var target = GetTarget();
        StartHere<ToolRoom>();
        Take<ShuttleAccessCard>();
        Take<KitchenAccessCard>();

        await target.GetResponse("take bar");

        // Scrambles whichever unscrambled AccessCard comes first among the player's held items -
        // Shuttle was picked up first, so it's scrambled first.
        GetItem<ShuttleAccessCard>().Scrambled.Should().BeTrue();
        GetItem<KitchenAccessCard>().Scrambled.Should().BeFalse();

        await target.GetResponse("look");

        GetItem<KitchenAccessCard>().Scrambled.Should().BeTrue();
    }

    [Test]
    public async Task DroppingTheMagnet_StopsFurtherScrambling()
    {
        var target = GetTarget();
        StartHere<ToolRoom>();
        Take<ShuttleAccessCard>();
        Take<KitchenAccessCard>();

        await target.GetResponse("take bar");
        GetItem<ShuttleAccessCard>().Scrambled.Should().BeTrue();

        await target.GetResponse("drop bar");
        await target.GetResponse("look");
        await target.GetResponse("look");

        GetItem<KitchenAccessCard>().Scrambled.Should().BeFalse();
    }
}
