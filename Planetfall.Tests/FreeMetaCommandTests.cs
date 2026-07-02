using FluentAssertions;
using GameEngine;
using Model.AIParsing;
using Model.Intent;
using Moq;
using Planetfall.GlobalCommand;
using Planetfall.Location.Kalamontee.Dorm;

namespace Planetfall.Tests;

/// <summary>
/// Issue #354: "score", "look", "inventory" (and "time") are meta/informational verbs that must be
/// free actions - they should never advance Context.Moves or tick the Planetfall survival clock
/// (Chronometer.CurrentTime). Before the fix these fell through to the same turn-processing path as
/// real actions (take, open, movement), so simply checking your status could push the clock past a
/// sleep/hunger threshold and trigger an unwarranted death.
/// </summary>
public class FreeMetaCommandTests : EngineTestsBase
{
    [TestCase("score")]
    [TestCase("look")]
    [TestCase("inventory")]
    [TestCase("i")]
    [TestCase("time")]
    [TestCase("diagnose")]
    public async Task MetaCommand_DoesNotAdvanceMovesOrSurvivalClock(string command)
    {
        var engine = GetTarget();
        StartHere<DormA>();

        var movesBefore = engine.Moves;
        var timeBefore = engine.Context.CurrentTime;

        await engine.GetResponse(command);

        engine.Moves.Should().Be(movesBefore);
        engine.Context.CurrentTime.Should().Be(timeBefore);
    }

    [Test]
    public async Task MetaCommand_RepeatedChecks_NeverAdvanceMovesOrSurvivalClock()
    {
        // Mirrors the golden-walkthrough regression from issue #354: several consecutive "free"
        // status checks in a row must not silently burn turns/clock ticks.
        var engine = GetTarget();
        StartHere<DormA>();

        var movesBefore = engine.Moves;
        var timeBefore = engine.Context.CurrentTime;

        await engine.GetResponse("score");
        await engine.GetResponse("look");
        await engine.GetResponse("inventory");
        await engine.GetResponse("score");

        engine.Moves.Should().Be(movesBefore);
        engine.Context.CurrentTime.Should().Be(timeBefore);
    }

    [Test]
    public async Task RealAction_StillAdvancesMovesAndSurvivalClock()
    {
        // Control: the fix must not turn every command into a free action - only the meta/informational
        // ones. A genuine turn-consuming action (waiting) must still advance both.
        var engine = GetTarget();
        StartHere<DormA>();

        var movesBefore = engine.Moves;
        var timeBefore = engine.Context.CurrentTime;

        await engine.GetResponse("wait");

        engine.Moves.Should().Be(movesBefore + 1);
        engine.Context.CurrentTime.Should().BeGreaterThan(timeBefore);
    }

    [TestCase("g")]
    [TestCase("again")]
    public async Task MetaCommand_ReplayedViaAgain_StillDoesNotAdvanceMovesOrSurvivalClock(string again)
    {
        // Follow-up to issue #354: "g"/"again" replays the previous command. Replaying a free
        // command must stay free too - the literal text "g" doesn't match any free-command pattern,
        // so without resolving the replay target first, the engine was treating the replay as a
        // real turn even though it repeats an informational check.
        var engine = GetTarget();
        StartHere<DormA>();

        var movesBefore = engine.Moves;
        var timeBefore = engine.Context.CurrentTime;

        await engine.GetResponse("look");
        engine.Moves.Should().Be(movesBefore, "the first 'look' should not consume a turn");

        await engine.GetResponse(again);

        engine.Moves.Should().Be(movesBefore, "replaying 'look' via '{0}' should also not consume a turn", again);
        engine.Context.CurrentTime.Should().Be(timeBefore);
    }

    [Test]
    public async Task AiRecognizedLookPhrasing_NotInStaticList_StillDoesNotTickSurvivalClock()
    {
        // Follow-up to issue #354: a look/inventory phrasing the AI parser recognizes but
        // GlobalCommandFactory's static switch doesn't (e.g. "what is this place?") reaches
        // LookProcessor via the AI-fallback path (ProcessComplexIntent), not the fast static path.
        // Context.Moves has already advanced by the time that classification is known (it can't be
        // un-done), but the survival-clock tick - the actual death risk issue #354 is about - can
        // and must still be skipped for it.
        var mockAiParser = new Mock<IAIParser>();
        mockAiParser
            .Setup(p => p.AskTheAIParser("what is this place?", It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new LookIntent());
        var parser = new IntentParser(mockAiParser.Object, new PlanetfallGlobalCommandFactory());

        var engine = GetTarget(parser);
        StartHere<DormA>();

        var timeBefore = engine.Context.CurrentTime;

        var response = await engine.GetResponse("what is this place?");

        response.Should().Contain("Dorm");
        engine.Context.CurrentTime.Should().Be(timeBefore);
    }
}
