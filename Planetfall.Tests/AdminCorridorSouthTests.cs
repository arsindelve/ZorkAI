using FluentAssertions;
using Planetfall.Item.Kalamontee.Admin;
using Planetfall.Item.Kalamontee.Mech;
using Planetfall.Location.Kalamontee.Admin;

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
}
