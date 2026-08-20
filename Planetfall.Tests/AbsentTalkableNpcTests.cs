using FluentAssertions;
using Model.AIGeneration.Requests;
using Moq;
using Planetfall.Item.Feinstein;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
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
    /// <summary>
    /// Puts Floyd in the state these tests are actually about: met, then left behind or wandered
    /// off. Naming him BEFORE the player has woken him is claimed by the fourth-wall intercept
    /// (issue #552), which is a different answer to a different situation - the player can't have
    /// been separated from a companion they have never met.
    /// </summary>
    private void MeetFloyd()
    {
        GetItem<Floyd>().HasEverBeenOn = true;
    }

    [Test]
    public async Task AddressingAbsentFloyd_GoUp_SaysNotHere_AndDoesNotMove()
    {
        var target = GetTarget();
        MeetFloyd();
        StartHere<DeckNine>();

        var response = await target.GetResponse("floyd, go up");

        response.Should().Contain("Floyd isn't here.");
        Context.CurrentLocation.Should().BeOfType<DeckNine>();
    }

    [Test]
    public async Task AddressingAbsentFloyd_DropDiary_SaysNotHere_AndKeepsItem()
    {
        var target = GetTarget();
        MeetFloyd();
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
        MeetFloyd();
        StartHere<DeckNine>();

        var response = await target.GetResponse("floyd, sing");

        response.Should().Contain("Floyd isn't here.");
        response.Should().NotContain("tune");
    }

    [Test]
    public async Task AddressingBlather_AfterThePodLands_IsDeterministicNotNarrated()
    {
        // Issue #545 review: IsGoneForGood exists to stop the narrator answering "where is X?" by
        // inventing a whereabouts, but only Floyd opted in. Once the escape pod is down on Resida the
        // Feinstein and everyone still aboard are beyond reach forever, so Blather must get his own
        // fixed line rather than an improvisation about what he might be up to.
        var target = GetTarget();
        StartHere<DeckNine>();
        GetLocation<EscapePod>().LandedSafely = true;
        Mock.Get(target.GenerationClient)
            .Setup(c => c.GenerateNarration(It.IsAny<TalkingToAbsentCharacterRequest>(), It.IsAny<string>()))
            .ReturnsAsync("Blather is probably off inspecting another deck.");

        var response = await target.GetResponse("blather, go up");

        response.Should().Contain("Blather isn't here.");
        response.Should().NotContain("another deck");
        Context.CurrentLocation.Should().BeOfType<DeckNine>();
    }

    [Test]
    public async Task AddressingAmbassador_AfterThePodLands_IsDeterministicNotNarrated()
    {
        var target = GetTarget();
        StartHere<DeckNine>();
        GetLocation<EscapePod>().LandedSafely = true;
        Mock.Get(target.GenerationClient)
            .Setup(c => c.GenerateNarration(It.IsAny<TalkingToAbsentCharacterRequest>(), It.IsAny<string>()))
            .ReturnsAsync("The ambassador has slithered off somewhere.");

        var response = await target.GetResponse("ambassador, go up");

        response.Should().Contain("The ambassador isn't here.");
        response.Should().NotContain("slithered");
    }

    [Test]
    public async Task AddressingBlather_WhileStillAboard_StillGetsTheNarratedAbsence()
    {
        // The gate must be keyed on the pod having landed, not on the explosion: Blather is alive
        // and barking at the player for several turns after the ship is hit, so while the player is
        // still aboard his absence is an ordinary "he's elsewhere" the narrator may phrase freely.
        var target = GetTarget();
        StartHere<DeckNine>();
        Mock.Get(target.GenerationClient)
            .Setup(c => c.GenerateNarration(It.IsAny<TalkingToAbsentCharacterRequest>(), It.IsAny<string>()))
            .ReturnsAsync("Blather is probably off inspecting another deck.");

        var response = await target.GetResponse("blather, go up");

        response.Should().Contain("another deck");
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
    public async Task AddressingAbsentAmbassadorWithLeadingArticle_SaysNotHere_AndDoesNotMove()
    {
        var target = GetTarget();
        StartHere<DeckNine>();

        var response = await target.GetResponse("the ambassador, go up");

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
        MeetFloyd();
        StartHere<DeckNine>();

        var response = await target.GetResponse("floyd go up");

        response.Should().Contain("Floyd isn't here.");
        Context.CurrentLocation.Should().BeOfType<DeckNine>();
    }

    [Test]
    public async Task AddressingAbsentFloydWithoutComma_DropDiary_SaysNotHere_AndKeepsItem()
    {
        var target = GetTarget();
        MeetFloyd();
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

    // Floyd answers to "robot", so addressing "the robot" reaches him (owner's call).
    [Test]
    public async Task TellingTheRobot_WhileFloydAbsent_IsAttributedToFloyd()
    {
        var target = GetTarget();
        StartHere<DeckNine>();

        var response = await target.GetResponse("tell the robot to go up");

        response.Should().Contain("Floyd isn't here.");
        Context.CurrentLocation.Should().BeOfType<DeckNine>();
    }

    [TestCase("hey floyd, go up", TestName = "HeyFloyd")]
    [TestCase("yo robot", TestName = "YoRobot")]
    [TestCase("the robot, go up", TestName = "TheRobot")]
    public async Task AddressingAbsentFloydCasually_SaysNotHere(string input)
    {
        var target = GetTarget();
        MeetFloyd();
        StartHere<DeckNine>();

        var response = await target.GetResponse(input);

        response.Should().Contain("Floyd isn't here.");
        Context.CurrentLocation.Should().BeOfType<DeckNine>();
    }

    // Unusual phrasing the deterministic matcher won't catch: the conversation classifier (stubbed
    // here to say "conversational") is the backstop that still recognizes it as addressing Floyd.
    [Test]
    public async Task AddressingAbsentFloydWithUnusualPhrasing_DefersToClassifier_SaysNotHere()
    {
        var target = GetTarget();
        MeetFloyd();
        StartHere<DeckNine>();
        ParseConversationMock
            .Setup(p => p.ParseAsync("could you let floyd know to wait for me"))
            .ReturnsAsync((true, ""));

        var response = await target.GetResponse("could you let floyd know to wait for me");

        response.Should().Contain("Floyd isn't here.");
        Context.CurrentLocation.Should().BeOfType<DeckNine>();
    }

    [Test]
    public async Task TellingAbsentFloydToDoSomething_SaysNotHere()
    {
        var target = GetTarget();
        MeetFloyd();
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

    // The absent-NPC guard force-instantiates the whole declared roster early (see
    // ConversationHandler.CollectAllKnownTalkers). This pins the contract that doing so has no
    // observable side effects: instantiating an NPC must not place it in a location or register it
    // as an actor. If a future talkable NPC's Init() does either, this test fails loudly.
    [Test]
    public void InstantiatingTheTalkableRoster_DoesNotPlaceOrRegisterAnyNpc()
    {
        GetTarget();
        StartHere<DeckNine>();

        var floyd = GetItem<Floyd>();
        var blather = GetItem<Blather>();
        var ambassador = GetItem<Ambassador>();

        floyd.CurrentLocation.Should().BeNull();
        blather.CurrentLocation.Should().BeNull();
        ambassador.CurrentLocation.Should().BeNull();

        Context.Actors.Should().NotContain(floyd);
        Context.Actors.Should().NotContain(blather);
        Context.Actors.Should().NotContain(ambassador);
    }
}
