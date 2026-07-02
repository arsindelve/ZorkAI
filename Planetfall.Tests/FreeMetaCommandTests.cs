using FluentAssertions;
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
}
