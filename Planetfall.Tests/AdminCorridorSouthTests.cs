using FluentAssertions;
using Planetfall.Item.Feinstein;
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
}
