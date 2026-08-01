using System.Text;
using GameEngine;
using GameEngine.StaticCommand.Implementation;
using Model.AIGeneration;
using Model.Interaction;
using Model.Interface;
using ZorkOne;

namespace UnitTests.Engine;

/// <summary>
///     Everything about "which one do you mean?", in one place. Disambiguation used to be tested from
///     three different directories — the processor's matching rules (GlobalCommands), the end-to-end
///     prompt/answer flow (SingleNounProcessors) and the save/restore round-trip (Engine) — which made
///     it easy to fix one layer and miss the others. Merged from the former DisambiguationProcessorTests
///     / DisambiguationTests / DisambiguationPersistenceTests.
///
///     Issue #472 is why the persistence half exists: the deployed game is stateless — every command is a
///     separate request whose full state travels in the serialized session, and the
///     <see cref="GameEngine{TInfocomGame,TContext}" /> is rebuilt per request. A disambiguation (and the
///     sibling "it"/"them" clarification) is armed in an in-memory engine field, so before that fix it was
///     lost across the request boundary and the player's answer was parsed as a brand-new command.
///     Those tests serialize + deserialize between the PROMPT turn and the ANSWER turn, mirroring the
///     Lambda round-trip. A single-process test that skips the round-trip does NOT catch the bug — the
///     engine field survives in memory — so the round-trip is the essential part.
/// </summary>
public class DisambiguationTests : EngineTestsBase
{
    /// <summary>
    ///     The Maintenance Room is the disambiguation fixture: it holds four differently-colored buttons,
    ///     so a bare "press button" must prompt. It is a DarkLocation, hence the explicit lighting.
    /// </summary>
    private GameEngine<ZorkI, ZorkIContext> StartInMaintenanceRoom()
    {
        var engine = GetTarget();
        var room = Repository.GetLocation<MaintenanceRoom>();
        room.IsNoLongerDark = true;
        engine.Context.CurrentLocation = room;
        return engine;
    }

    /// <summary>Same, plus the two knives that make a bare "knife" ambiguous.</summary>
    private GameEngine<ZorkI, ZorkIContext> StartInMaintenanceRoomWithBothKnives()
    {
        var engine = GetTarget();
        var room = Repository.GetLocation<MaintenanceRoom>();
        room.IsNoLongerDark = true;
        engine.Context.ItemPlacedHere(Repository.GetItem<NastyKnife>());
        engine.Context.ItemPlacedHere(Repository.GetItem<RustyKnife>());
        engine.Context.CurrentLocation = room;
        return engine;
    }

    /// <summary>
    ///     Rebuild the engine from a saved-game blob exactly as the stateless deployment does on the next
    ///     request: a fresh engine (which also resets the Repository) with the prior state restored into it.
    /// </summary>
    private GameEngine<ZorkI, ZorkIContext> RoundTrip(GameEngine<ZorkI, ZorkIContext> engine)
    {
        var saved = engine.SaveGame();
        var restored = GetTarget();
        restored.RestoreGame(saved);
        return restored;
    }

    // ----- The processor's matching rules. It matches answers by SUBSTRING, so several of a prompt's
    // choices can match one reply and the tie-breaking rules decide which button the player actually gets.

    private static Task<string> Answer(Dictionary<string, string> choices, string reply)
    {
        var result = new DisambiguationInteractionResult("Which one do you mean?", choices, "press the {0}");
        return new DisambiguationProcessor(result)
            .Process(reply, Mock.Of<IContext>(), Mock.Of<IGenerationClient>(), Runtime.Console);
    }

    [Test]
    public async Task LongerMatchWins_BecauseItIsTheMoreSpecificAnswer()
    {
        var choices = new Dictionary<string, string>
        {
            { "red", "red button" },
            { "red elevator button", "red elevator button" }
        };

        var response = await Answer(choices, "the red elevator button");

        response.Should().Be("press the red elevator button");
    }

    // Booth 2 labels its two buttons "1" and "3", so "one" answers the brown button while "tan" answers
    // the other one -- and a reply of "the tan one" contains BOTH keys, at equal length. The player named
    // the tan button first, and that is the one they meant. Getting this wrong teleports them to the
    // wrong booth, so it is worth pinning.
    //
    // Note the residual hazard this does NOT solve: "one" is both Booth 2's label for a button and an
    // English pronoun, so a reply that puts the pronoun first ("the one that is tan") still resolves to
    // the wrong button. No tie-breaking rule can separate those two senses -- it needs the key gone, the
    // same lesson DestinationNavigation already learned when it stopped emitting single-character keys.
    [Test]
    public async Task EqualLengthMatches_AreBrokenByWhereThePlayerMentionedThem()
    {
        var choices = new Dictionary<string, string>
        {
            { "brown", "brown button" },
            { "tan", "tan button" },
            { "one", "brown button" },
            { "three", "tan button" }
        };

        var response = await Answer(choices, "the tan one");

        response.Should().Be("press the tan button");
    }

    // The tie-break used to fall through to whatever order the caller happened to list its choices in,
    // which a Dictionary does not promise and which no test held in place: re-sorting a prompt's choices
    // could silently change which button a player got. Declaration order must not be able to decide.
    [Test]
    public async Task DeclarationOrderDoesNotDecideTheWinner()
    {
        var oneOrder = new Dictionary<string, string>
        {
            { "round", "round button" },
            { "white", "white button" }
        };

        var reversed = new Dictionary<string, string>
        {
            { "white", "white button" },
            { "round", "round button" }
        };

        var first = await Answer(oneOrder, "the round white button");
        var second = await Answer(reversed, "the round white button");

        first.Should().Be("press the round button");
        second.Should().Be(first);
    }

    [Test]
    public async Task AnUnrecognizedReply_IsPassedThroughUntouched()
    {
        var choices = new Dictionary<string, string> { { "red", "red button" } };

        var response = await Answer(choices, "go north");

        response.Should().Be("go north");
    }

    [Test]
    public async Task AnEmptyReply_YieldsNoCommand()
    {
        var choices = new Dictionary<string, string> { { "red", "red button" } };

        var response = await Answer(choices, "");

        response.Should().BeEmpty();
    }

    // ----- The end-to-end prompt/answer flow, in a single process.

    [Test]
    public async Task SoManyButtons_StepOne()
    {
        var engine = StartInMaintenanceRoom();

        var response = await engine.GetResponse("press button");

        response.Should()
            .Contain(
                "Which button do you mean, the blue button, the red button, the yellow button or the brown button?");
    }

    [Test]
    public async Task SoManyButtons_StepTwo_Correct()
    {
        var engine = StartInMaintenanceRoom();

        await engine.GetResponse("press button");
        var response = await engine.GetResponse("yellow one");

        response.Should().Contain("Click");
    }

    [Test]
    public async Task SoManyButtons_StepTwo_SomethingElse()
    {
        var engine = StartInMaintenanceRoom();

        await engine.GetResponse("press button");
        var response = await engine.GetResponse("wait");

        response.Should().Contain("Time passes");
    }

    [Test]
    public async Task SoManyKnives()
    {
        var engine = StartInMaintenanceRoomWithBothKnives();

        var response = await engine.GetResponse("drop knife");

        response.Should().Contain("Do you mean the nasty knife or the rusty knife");
    }

    [Test]
    public async Task SoManyKnives_Dropped()
    {
        var engine = StartInMaintenanceRoomWithBothKnives();

        await engine.GetResponse("drop knife");
        var response = await engine.GetResponse("rusty");

        response.Should().Contain("Dropped");
    }

    // ----- Issue #472: the same flow, but with a stateless save/restore between prompt and answer.

    [Test]
    public async Task Knife_Disambiguation_AnswerResolves_AcrossSaveRestore()
    {
        var engine = StartInMaintenanceRoomWithBothKnives();

        var prompt = await engine.GetResponse("drop knife");
        prompt.Should().Contain("Do you mean the nasty knife or the rusty knife");

        // The stateless deployment reconstructs the engine here; the answer arrives on a fresh engine.
        var restored = RoundTrip(engine);

        var response = await restored.GetResponse("rusty");

        response.Should().Contain("Dropped", "the restored engine must still resolve the pending prompt");
        restored.Context.HasItem<RustyKnife>().Should().BeFalse("the rusty knife was the one dropped");
        restored.Context.HasItem<NastyKnife>().Should().BeTrue("only the rusty knife was chosen");
    }

    [Test]
    public async Task Button_Disambiguation_AnswerResolves_AcrossSaveRestore()
    {
        var engine = StartInMaintenanceRoom();

        var prompt = await engine.GetResponse("press button");
        prompt.Should().Contain("Which button do you mean");

        var restored = RoundTrip(engine);

        // "yellow one" does NOT independently parse as "press the yellow button" — it only resolves if
        // the restored engine still routes it through the pending disambiguation processor.
        var response = await restored.GetResponse("yellow one");

        response.Should().Contain("Click");
    }

    [Test]
    public async Task Disambiguation_PlainNounAnswer_ThatDoesNotIndependentlyResolve_StillWorks()
    {
        // The control case from the issue: an answer that would be inert as a standalone command
        // ("rusty" alone is not a full command) must resolve when it answers the pending prompt.
        var engine = StartInMaintenanceRoomWithBothKnives();

        await engine.GetResponse("drop knife");
        var restored = RoundTrip(engine);

        var response = await restored.GetResponse("nasty");

        response.Should().Contain("Dropped");
        restored.Context.HasItem<NastyKnife>().Should().BeFalse();
    }

    [Test]
    public async Task Disambiguation_UnrelatedAnswerAfterRestore_ProcessesAsNewCommand()
    {
        // Answering with something that is not one of the choices abandons the prompt and processes the
        // new command normally (matching the in-memory behavior), and must NOT drop a knife.
        var engine = StartInMaintenanceRoomWithBothKnives();

        await engine.GetResponse("drop knife");
        var restored = RoundTrip(engine);

        var response = await restored.GetResponse("wait");

        response.Should().Contain("Time passes");
        restored.Context.HasItem<NastyKnife>().Should().BeTrue("no knife should have been dropped");
        restored.Context.HasItem<RustyKnife>().Should().BeTrue("no knife should have been dropped");
    }

    [Test]
    public async Task Disambiguation_DoesNotHijackTheTurnAfterItIsResolved()
    {
        // After the pending prompt is answered across a restore, the NEXT command must be parsed
        // normally — the pending state must not linger and swallow it.
        var engine = StartInMaintenanceRoomWithBothKnives();

        await engine.GetResponse("drop knife");
        var afterPrompt = RoundTrip(engine);
        await afterPrompt.GetResponse("rusty"); // resolves the prompt

        // Round-trip again (next stateless request) and issue an unrelated command.
        var afterAnswer = RoundTrip(afterPrompt);
        var response = await afterAnswer.GetResponse("wait");

        response.Should().Contain("Time passes", "the resolved prompt must not linger and hijack this turn");
    }

    [Test]
    public async Task It_Clarification_AnswerResolves_AcrossSaveRestore()
    {
        // The "it"/"them" clarification path is the same _processorInProgress mechanism the issue calls
        // out. "take it" with no antecedent asks "What item are you referring to?"; the noun answer must
        // survive the request boundary too.
        var engine = GetTarget();
        engine.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();

        var prompt = await engine.GetResponse("take it");
        prompt.Should().Contain("What item are you referring to");

        var restored = RoundTrip(engine);

        var response = await restored.GetResponse("lantern");

        response.Should().Contain("Taken");
        restored.Context.HasItem<Lantern>().Should().BeTrue();
    }

    [Test]
    public async Task Them_Clarification_AnswerResolves_AcrossSaveRestore()
    {
        // "drop them" with only a single non-plural item held asks for clarification too; that path must
        // round-trip as well.
        var engine = GetTarget();
        engine.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();
        await engine.GetResponse("take lantern");

        var prompt = await engine.GetResponse("drop them");
        prompt.Should().Contain("What item are you referring to");

        var restored = RoundTrip(engine);

        var response = await restored.GetResponse("lantern");

        response.Should().Contain("Dropped");
        restored.Context.HasItem<Lantern>().Should().BeFalse();
    }

    // ----- White-box: the descriptor is what actually persists, and it is armed/cleared correctly. -----

    [Test]
    public async Task Prompt_ArmsPendingDisambiguationDescriptor_OnContext()
    {
        var engine = StartInMaintenanceRoomWithBothKnives();

        engine.Context.PendingDisambiguation.Should().BeNull("no prompt has been asked yet");

        await engine.GetResponse("drop knife");

        engine.Context.PendingDisambiguation.Should().NotBeNull("the prompt must be recorded on the Context so it can round-trip");
        engine.Context.PendingDisambiguation!.PossibleResponses.Values
            .Should().Contain("rusty knife").And.Contain("nasty knife");
        engine.Context.PendingDisambiguation.ReplacementString.Should().Contain("{0}");
    }

    [Test]
    public async Task SaveGameJson_CarriesPendingDisambiguation_WhilePromptIsOpen()
    {
        var engine = StartInMaintenanceRoomWithBothKnives();

        await engine.GetResponse("drop knife");

        // The serialized session — the ONLY thing that travels across the stateless request boundary —
        // must contain the pending prompt's payload. This is the crux of issue #472. (The property NAME
        // alone is always present because Newtonsoft writes nulls, so assert on the response map, which
        // only appears when the descriptor is actually populated.)
        var json = engine.SaveGame();
        json.Should().Contain("PossibleResponses");
        json.Should().Contain("rusty knife");
    }

    [Test]
    public async Task PendingDisambiguation_IsClearedAfterAnswer_AndDoesNotLeakIntoTheNextSave()
    {
        var engine = StartInMaintenanceRoomWithBothKnives();

        await engine.GetResponse("drop knife");
        var restored = RoundTrip(engine);
        await restored.GetResponse("rusty");

        restored.Context.PendingDisambiguation.Should().BeNull("answering the prompt must clear the pending state");
        restored.SaveGame().Should().NotContain("PossibleResponses",
            "a resolved prompt must not leak into the next turn's serialized session");
    }

    [Test]
    public async Task UnrelatedAnswer_ClearsPendingDisambiguation()
    {
        var engine = StartInMaintenanceRoomWithBothKnives();

        await engine.GetResponse("drop knife");
        var restored = RoundTrip(engine);
        await restored.GetResponse("wait"); // abandons the prompt

        restored.Context.PendingDisambiguation.Should().BeNull("abandoning the prompt must clear it too");
    }

    [Test]
    public async Task NormalCommand_DoesNotArmAnyPendingState()
    {
        var engine = GetTarget();
        engine.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();

        await engine.GetResponse("take lantern");

        engine.Context.PendingDisambiguation.Should().BeNull();
        engine.Context.PendingClarificationCommand.Should().BeNull();
        engine.SaveGame().Should().NotContain("PossibleResponses");
    }

    [Test]
    public async Task ItClarification_ArmsAndClearsPendingClarificationCommand()
    {
        var engine = GetTarget();
        engine.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();

        await engine.GetResponse("take it");
        engine.Context.PendingClarificationCommand.Should().Be("take it");

        var restored = RoundTrip(engine);
        restored.Context.PendingClarificationCommand.Should().Be("take it", "the clarification must survive the round-trip");

        await restored.GetResponse("lantern");
        restored.Context.PendingClarificationCommand.Should().BeNull("resolving the clarification clears it");
    }

    [Test]
    public async Task Disambiguation_SurvivesTheExactLambdaBase64RoundTrip()
    {
        // Faithful reproduction of ZorkOneController's session handling: SaveGame() -> UTF8 -> base64 ->
        // (DynamoDB) -> base64 decode -> RestoreGame(). Base64 is lossless, so this must behave exactly
        // like RoundTrip above; included to pin the real transport the bug was reported on.
        var engine = StartInMaintenanceRoomWithBothKnives();

        await engine.GetResponse("drop knife");

        var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(engine.SaveGame()));
        var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));

        var restored = GetTarget();
        restored.RestoreGame(decoded);
        var response = await restored.GetResponse("rusty");

        response.Should().Contain("Dropped");
    }
}
