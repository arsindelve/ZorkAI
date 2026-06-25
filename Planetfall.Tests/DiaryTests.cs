using FluentAssertions;
using Planetfall.Item.Feinstein;
using Planetfall.Location.Feinstein;

namespace Planetfall.Tests;

public class DiaryTests : EngineTestsBase
{
    /// <summary>
    /// Reads the diary and advances to the requested entry by pressing the button.
    /// Returns the response text shown for that entry.
    /// </summary>
    private async Task<string> AdvanceToEntry(GameEngine.GameEngine<PlanetfallGame, PlanetfallContext> target,
        int entryIndex)
    {
        var response = await target.GetResponse("read diary");
        for (var i = 0; i < entryIndex; i++)
            response = await target.GetResponse("push button");
        return response!;
    }

    [Test]
    public async Task SeptemFive_HasNoGarbledText()
    {
        var target = GetTarget();
        StartHere<DeckNine>();
        Take<Diary>();

        // Index 13 = the Septem 5 entry.
        var response = await AdvanceToEntry(target, 13);

        response.Should().Contain("when I was cleaning");
        response.Should().NotContain("when I was when I was");
        response.Should().Contain("and assigned");
        response.Should().NotContain("andassigned");
        response.Should().Contain("even abandon");
        response.Should().NotContain("evenabandon");
    }

    [Test]
    public async Task EndOfDiary_DoesNotContradictItself()
    {
        var target = GetTarget();
        StartHere<DeckNine>();
        Take<Diary>();

        // Index 14 = the final "END OF DIARY" entry.
        var response = await AdvanceToEntry(target, 14);

        response.Should().Contain("the little button flickers off");
        response.Should().NotContain("the little button flashes");
    }

    [Test]
    public async Task Examine_BeforeReading_ButtonIsFlashing()
    {
        var target = GetTarget();
        StartHere<DeckNine>();
        Take<Diary>();

        var response = await target.GetResponse("examine diary");

        response.Should().Contain("which is flashing");
    }

    [Test]
    public async Task Examine_AfterEndOfDiary_ButtonIsNotFlashing()
    {
        var target = GetTarget();
        StartHere<DeckNine>();
        Take<Diary>();

        // Index 14 = the final "END OF DIARY" entry, whose text says the button flickers off.
        await AdvanceToEntry(target, 14);

        var response = await target.GetResponse("examine diary");

        // The button just flickered off -- examining it must not claim it is still flashing.
        response.Should().NotContain("which is flashing");
    }

    [Test]
    public async Task AfterEndOfDiary_PressingButtonResetsDiary()
    {
        var target = GetTarget();
        StartHere<DeckNine>();
        Take<Diary>();

        await AdvanceToEntry(target, 14);

        // One more press after the final entry resets the diary; nothing visibly happens.
        var response = await target.GetResponse("push button");

        response.Should().Contain("Nothing happens");
    }
}
