using FluentAssertions;
using Planetfall.Location.Lawanda.LabOffice;

namespace Planetfall.Tests;

public class BioLabEscapeTests : EngineTestsBase
{
    [Test]
    public async Task LabDesk_OpenDesk_ShowsContents()
    {
        var target = GetTarget();
        StartHere<LabOffice>();

        var response = await target.GetResponse("open desk");

        response.Should().Contain("Opening the desk reveals a gas mask");
    }

    [Test]
    public async Task LabDesk_OpenDesk_WhenEmpty_ShowsOpened()
    {
        var target = GetTarget();
        StartHere<LabOffice>();

        await target.GetResponse("open desk");
        await target.GetResponse("take mask");
        await target.GetResponse("close desk");
        var response = await target.GetResponse("open desk");

        response.Should().Be("Opened. \n");
    }
}