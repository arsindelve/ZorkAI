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
}
