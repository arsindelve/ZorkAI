using FluentAssertions;
using Planetfall.Item.Kalamontee.Admin;
using Planetfall.Location.Kalamontee;
using Planetfall.Location.Kalamontee.Admin;
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

    /// <summary>
    ///     Issue #283: "look at &lt;single-word noun&gt;" was collapsing to the bare-room LOOK command,
    ///     re-describing the room instead of examining the named object. "look at" must route to the
    ///     same examine-with-noun path as "examine" / "inspect", including the single-word-noun case.
    /// </summary>
    [Test]
    public async Task LookAt_Light_AtComputerRoom_RoutesLikeExamine()
    {
        var target = GetTarget();
        StartHere<ComputerRoom>();

        var response = await target.GetResponse("look at light");

        response.Should().Contain("malfunction in the computer");
    }

    [Test]
    public async Task LookAt_Controls_AtHelicopter_RoutesLikeExamine()
    {
        var target = GetTarget();
        StartHere<Helicopter>();

        var response = await target.GetResponse("look at controls");

        response.Should().Contain("covered and locked");
    }

    /// <summary>
    ///     Issue #398: examining an OPEN open/close container must list its contents instead of
    ///     reporting only that the drawer is "open" - which hid the access cards inside the admin desks.
    ///     (Merged in from the former AdminOfficeTests.)
    /// </summary>
    [Test]
    public async Task ExamineSmallDesk_WhenOpen_ListsContents()
    {
        var target = GetTarget();
        StartHere<SmallOffice>();
        GetItem<SmallDesk>().IsOpen = true;

        var response = await target.GetResponse("examine desk");

        response.Should().Contain("kitchen access card");
        response.Should().NotContain("currently open");
    }

    [Test]
    public async Task ExamineSmallDesk_WhenClosed_ReportsClosed()
    {
        var target = GetTarget();
        StartHere<SmallOffice>();
        GetItem<SmallDesk>().IsOpen = false;

        var response = await target.GetResponse("examine desk");

        response.Should().Contain("closed");
        response.Should().NotContain("kitchen access card");
    }

    [Test]
    public async Task ExamineLargeDesk_WhenOpen_ListsContents()
    {
        var target = GetTarget();
        StartHere<LargeOffice>();
        GetItem<LargeDesk>().IsOpen = true;

        var response = await target.GetResponse("examine desk");

        response.Should().Contain("shuttle access card");
        response.Should().NotContain("currently open");
    }
}
