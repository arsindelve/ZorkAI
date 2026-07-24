using FluentAssertions;
using GameEngine;
using Planetfall.Item.Kalamontee.Admin;
using Planetfall.Location.Kalamontee.Admin;

namespace Planetfall.Tests;

/// <summary>
/// Issue #398: examining an OPEN open/close container must list its contents instead of
/// reporting only that the drawer is "open" - which hid the access cards inside the desks.
/// </summary>
public class AdminOfficeTests : EngineTestsBase
{
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
