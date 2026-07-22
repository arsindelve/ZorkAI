using FluentAssertions;
using Planetfall.Item.Lawanda;
using Planetfall.Location.Lawanda;

namespace Planetfall.Tests;

public class MuralTests : EngineTestsBase
{
    [Test]
    public async Task ExamineMural_ShouldReturnDescription()
    {
        var target = GetTarget();
        StartHere<ProjConOffice>();

        var response = await target.GetResponse("examine mural");

        response.Should().Contain("gaudy work of orange and purple abstract shapes");
        response.Should().Contain("Burstini Bonz");
        response.Should().Contain("ripple now and then");
    }

    [Test]
    public async Task TakeMural_ShouldIndicateFirmlyAttached()
    {
        var target = GetTarget();
        StartHere<ProjConOffice>();

        var response = await target.GetResponse("take mural");

        response.Should().Contain("firmly attached to the wall");
    }

    [Test]
    public async Task PunchMural_ShouldReturnCivilResponse()
    {
        var target = GetTarget();
        StartHere<ProjConOffice>();

        var response = await target.GetResponse("punch mural");

        response.Should().Contain("My sentiments also, but let's be civil");
    }

    [Test]
    public async Task ProjConOffice_ShouldMentionMuralInDescription()
    {
        var target = GetTarget();
        StartHere<ProjConOffice>();

        var response = await target.GetResponse("look");

        response.Should().Contain("garish mural");
    }

    [Test]
    public async Task ProjConOffice_DescriptionShouldNotStartWithDoubledLetter()
    {
        // Regression for #435: the pre-announcement description had a typo ("TThis office...").
        var target = GetTarget();
        StartHere<ProjConOffice>();

        var response = await target.GetResponse("look");

        // \b anchors "This" to a word boundary, so the bug ("TThis office...") fails to match while the
        // correct text matches. This makes the positive assertion catch the regression on its own rather
        // than resting solely on the NotContain guard below (a plain Contain would pass on "TThis" too).
        response.Should().MatchRegex(@"\bThis office looks like a headquarters");
        response.Should().NotContain("TThis");
    }

    [Test]
    public void ProjConOffice_MuralShouldBePresent()
    {
        GetTarget();
        var location = StartHere<ProjConOffice>();
        var mural = GetItem<Mural>();
        mural.CurrentLocation.Should().Be(location);
        location.Items.Should().Contain(mural);
    }
}
