using FluentAssertions;
using Planetfall.Item.Feinstein;
using Planetfall.Location.Feinstein;

namespace Planetfall.Tests;

/// <summary>
/// Regression tests for #264. Addressing a talkable NPC (Floyd / Blather / the Ambassador) by name
/// while they are NOT present must tell the player the character isn't here instead of letting the
/// command leak into normal player parsing — which would silently move the player, drop their items,
/// or let the narrator hallucinate the absent NPC acting.
///
/// In production the narrator phrases the absence dynamically. The test engine stubs generation, so
/// these tests observe the deterministic static fallback ("X isn't here.") — which is exactly what
/// matters for this bug class: a non-leaking, side-effect-free response. The critical assertions are
/// that the player did not move and did not drop anything.
/// </summary>
public class AbsentTalkableNpcTests : EngineTestsBase
{
    [Test]
    public async Task AddressingAbsentFloyd_GoUp_SaysNotHere_AndDoesNotMove()
    {
        var target = GetTarget();
        StartHere<DeckNine>();

        var response = await target.GetResponse("floyd, go up");

        response.Should().Contain("Floyd isn't here.");
        Context.CurrentLocation.Should().BeOfType<DeckNine>();
    }

    [Test]
    public async Task AddressingAbsentFloyd_DropDiary_SaysNotHere_AndKeepsItem()
    {
        var target = GetTarget();
        StartHere<DeckNine>();
        Take<Diary>();

        var response = await target.GetResponse("floyd, drop diary");

        response.Should().Contain("Floyd isn't here.");
        Context.HasItem<Diary>().Should().BeTrue();
    }

    [Test]
    public async Task AddressingAbsentFloyd_Sing_SaysNotHere_AndDoesNotHallucinate()
    {
        var target = GetTarget();
        StartHere<DeckNine>();

        var response = await target.GetResponse("floyd, sing");

        response.Should().Contain("Floyd isn't here.");
        response.Should().NotContain("tune");
    }

    [Test]
    public async Task AddressingAbsentBlather_TakeBrush_SaysNotHere()
    {
        var target = GetTarget();
        StartHere<DeckNine>();

        var response = await target.GetResponse("blather, take brush");

        response.Should().Contain("Blather isn't here.");
    }

    [Test]
    public async Task AddressingAbsentBlather_GoUp_SaysNotHere_AndDoesNotMove()
    {
        var target = GetTarget();
        StartHere<DeckNine>();

        var response = await target.GetResponse("blather, go up");

        response.Should().Contain("Blather isn't here.");
        Context.CurrentLocation.Should().BeOfType<DeckNine>();
    }

    [Test]
    public async Task AddressingAbsentBlatherByFullName_SaysNotHere()
    {
        var target = GetTarget();
        StartHere<DeckNine>();

        var response = await target.GetResponse("ensign blather, go up");

        response.Should().Contain("Blather isn't here.");
        Context.CurrentLocation.Should().BeOfType<DeckNine>();
    }

    [Test]
    public async Task AddressingAbsentAmbassador_GoUp_SaysNotHere_AndDoesNotMove()
    {
        var target = GetTarget();
        StartHere<DeckNine>();

        var response = await target.GetResponse("ambassador, go up");

        response.Should().Contain("The ambassador isn't here.");
        Context.CurrentLocation.Should().BeOfType<DeckNine>();
    }

    [Test]
    public async Task AddressingAbsentAmbassador_TakeBrush_SaysNotHere()
    {
        var target = GetTarget();
        StartHere<DeckNine>();

        var response = await target.GetResponse("ambassador, take brush");

        response.Should().Contain("The ambassador isn't here.");
    }

    [Test]
    public async Task AddressingAbsentFloydWithoutComma_GoUp_SaysNotHere_AndDoesNotMove()
    {
        var target = GetTarget();
        StartHere<DeckNine>();

        var response = await target.GetResponse("floyd go up");

        response.Should().Contain("Floyd isn't here.");
        Context.CurrentLocation.Should().BeOfType<DeckNine>();
    }

    [Test]
    public async Task AddressingAbsentFloydWithoutComma_DropDiary_SaysNotHere_AndKeepsItem()
    {
        var target = GetTarget();
        StartHere<DeckNine>();
        Take<Diary>();

        var response = await target.GetResponse("floyd drop diary");

        response.Should().Contain("Floyd isn't here.");
        Context.HasItem<Diary>().Should().BeTrue();
    }

    [Test]
    public async Task AddressingAbsentBlatherByNameWithoutComma_SaysNotHere()
    {
        var target = GetTarget();
        StartHere<DeckNine>();

        var response = await target.GetResponse("blather go up");

        response.Should().Contain("Blather isn't here.");
        Context.CurrentLocation.Should().BeOfType<DeckNine>();
    }

    // "robot" is a generic synonym for Floyd; addressing "the robot" must not be attributed to the
    // absent Floyd (other robots exist elsewhere in the game).
    [Test]
    public async Task TellingTheRobot_WhileFloydAbsent_IsNotAttributedToFloyd()
    {
        var target = GetTarget();
        StartHere<DeckNine>();

        var response = await target.GetResponse("tell the robot to go up");

        response.Should().NotContain("Floyd isn't here");
    }

    [Test]
    public async Task TellingAbsentFloydToDoSomething_SaysNotHere()
    {
        var target = GetTarget();
        StartHere<DeckNine>();

        var response = await target.GetResponse("tell floyd to go up");

        response.Should().Contain("Floyd isn't here.");
        Context.CurrentLocation.Should().BeOfType<DeckNine>();
    }

    [Test]
    public async Task AskingAbsentAmbassador_SaysNotHere()
    {
        var target = GetTarget();
        StartHere<DeckNine>();

        var response = await target.GetResponse("ask the ambassador about the planet");

        response.Should().Contain("The ambassador isn't here.");
    }

    [Test]
    public async Task TalkingToAbsentBlather_SaysNotHere()
    {
        var target = GetTarget();
        StartHere<DeckNine>();

        var response = await target.GetResponse("talk to blather");

        response.Should().Contain("Blather isn't here.");
    }

    // No-hijack guard: a real command that merely *contains* the NPC's name but is not a direct
    // address must fall through to normal parsing, not the absent-NPC guard.
    [Test]
    public async Task ExaminingAbsentFloyd_IsNotInterceptedByGuard()
    {
        var target = GetTarget();
        StartHere<DeckNine>();

        var response = await target.GetResponse("examine floyd");

        response.Should().NotContain("isn't here");
    }

    [Test]
    public async Task AskingAboutAbsentAmbassador_IsNotInterceptedByGuard()
    {
        var target = GetTarget();
        StartHere<DeckNine>();

        // The name does not immediately follow the address verb, so this is not direct address.
        var response = await target.GetResponse("ask about the ambassador");

        response.Should().NotContain("isn't here");
    }

    [Test]
    public async Task AttackingAbsentFloyd_IsNotInterceptedByGuard()
    {
        var target = GetTarget();
        StartHere<DeckNine>();

        var response = await target.GetResponse("attack floyd");

        response.Should().NotContain("isn't here");
    }
}
