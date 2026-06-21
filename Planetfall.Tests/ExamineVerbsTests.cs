using FluentAssertions;
using Planetfall.Location.Kalamontee;
using Planetfall.Location.Kalamontee.Tower;
using Planetfall.Location.Lawanda;

namespace Planetfall.Tests;

/// <summary>
///     Issue #207: the scattered inline "examine / look at" verb arrays were centralized into
///     <see cref="Model.Verbs.ExamineVerbs" />. These tests pin the widening: a synonym like
///     "inspect" — which several of the old inline arrays did not include — must now route to the
///     same custom responses as "examine".
/// </summary>
public class ExamineVerbsTests : EngineTestsBase
{
    [Test]
    public async Task Inspect_Equipment_AtInfirmary_RoutesLikeExamine()
    {
        var target = GetTarget();
        StartHere<Infirmary>();

        var response = await target.GetResponse("inspect equipment");

        response.Should().Contain("so complicated");
    }

    [Test]
    public async Task Inspect_Light_AtComputerRoom_RoutesLikeExamine()
    {
        var target = GetTarget();
        StartHere<ComputerRoom>();

        var response = await target.GetResponse("inspect light");

        response.Should().Contain("malfunction in the computer");
    }

    [Test]
    public async Task Inspect_Controls_AtHelicopter_RoutesLikeExamine()
    {
        var target = GetTarget();
        StartHere<Helicopter>();

        var response = await target.GetResponse("inspect controls");

        response.Should().Contain("covered and locked");
    }

    [Test]
    public async Task Inspect_Games_AtRecArea_RoutesLikeExamine()
    {
        var target = GetTarget();
        StartHere<RecArea>();

        var response = await target.GetResponse("inspect games");

        response.Should().Contain("Double Fannucci");
    }
}
