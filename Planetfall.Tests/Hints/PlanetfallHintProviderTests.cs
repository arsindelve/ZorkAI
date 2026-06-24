using FluentAssertions;
using GameEngine;
using GameEngine.Hints;
using Planetfall.Hints;
using Planetfall.Item.Computer;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Planetfall.Location.Kalamontee.Admin;

namespace Planetfall.Tests.Hints;

/// <summary>
///     Drives the real <see cref="PlanetfallHintProvider" /> through the engine against live Planetfall
///     state (real Repository + Context, the repo's standard test pattern). A deterministic stub LLM
///     stands in for OpenAI so the localization / mapping / routing is asserted without network.
/// </summary>
[TestFixture]
public class PlanetfallHintProviderTests : EngineTestsBase
{
    [SetUp]
    public void SetUp() => GetTarget(); // Repository.Reset() + engine + a real PlanetfallContext (Context)

    private static HintService NewService() =>
        new(new PlanetfallHintProvider(), new InMemoryHintMemoryStore(), new TestStubLlm());

    [Test]
    public async Task FreshGame_BlocksOnTheOpeningPuzzle()
    {
        var result = await NewService().GetHint(new HintRequest("s", Context, null, false, null));

        result.Kind.Should().Be(HintKind.Progress);
        result.Topic.Should().Be("ESCAPE_POD");
        result.Rung.Should().Be(0); // the vaguest rung — "the ship is coming apart", not the solution
        result.Text.Should().Contain("ship");
    }

    [Test]
    public async Task FloydActivated_BackfillsOpeningAndBlocksOnNextStep()
    {
        Repository.GetItem<Floyd>().HasEverBeenOn = true;

        var result = await NewService().GetHint(new HintRequest("s", Context, null, false, null));

        // ESCAPE_POD/LAND back-fill from the verified FLOYD flag; the next mandatory open step is MAGNET.
        result.Topic.Should().Be("MAGNET");
    }

    [Test]
    public void CommunicationsFixed_BackfillsTheEntireTowerChain()
    {
        Repository.GetLocation<SystemsMonitors>().Fixed.Add("KUMUUNIKAASHUNZ");

        var progress = new PlanetfallHintProvider().ProgressMapper.Map(Context);

        progress.Nodes["COMM_FIX"].Should().Be(NodeStatus.Done);
        progress.Nodes["TOWER_UP"].Should().Be(NodeStatus.Done);   // back-filled prerequisite
        progress.Nodes["CROSS_RIFT"].Should().Be(NodeStatus.Done); // back-filled prerequisite
        progress.Nodes["ESCAPE_POD"].Should().Be(NodeStatus.Done); // back-filled to the root
    }

    [Test]
    public async Task DiseaseLateInTheGame_AttachesAWarningCaveat()
    {
        Context.Day = 7; // past the threshold, cure not done

        var result = await NewService().GetHint(new HintRequest("s", Context, null, false, null));

        result.SoftLock.Should().Be(SoftLockKind.Warning);
        result.Text.Should().Contain("Disease");
    }

    [Test]
    public void TiredPlayer_GetsAProactiveSleepNudge()
    {
        Context.Tired = TiredLevel.Tired;

        var nudges = NewService().ProactiveNudges(Context);

        nudges.Should().Contain(n => n.Category == "sleep");
    }

    [Test]
    public async Task WhyAmISick_RoutesToMechanicAndGroundsInTheDiseaseClock()
    {
        Context.Day = 3;

        var result = await NewService().GetHint(
            new HintRequest("s", Context, "why am I getting sick?", false, null));

        result.Kind.Should().Be(HintKind.Mechanic);
        result.Text.Should().Contain("Disease");
    }

    [Test]
    public async Task WhyIsEverythingDeserted_RoutesToLore()
    {
        var result = await NewService().GetHint(
            new HintRequest("s", Context, "why is everything deserted?", false, null));

        result.Kind.Should().Be(HintKind.Lore);
        result.Text.Should().Contain("automated");
    }
}

/// <summary>Deterministic stub LLM: keyword-routes intent and echoes content, so no OpenAI is needed.</summary>
internal sealed class TestStubLlm : IHintLanguageModel
{
    public Task<HintIntent> ClassifyIntent(string question)
    {
        var q = question.ToLowerInvariant();
        if (q.Contains("sick") || q.Contains("tired") || q.Contains("hungry"))
            return Task.FromResult(HintIntent.Mechanic);
        if (q.Contains("deserted") || q.Contains("who ") || q.Contains("what is") || q.Contains("history"))
            return Task.FromResult(HintIntent.Lore);
        return Task.FromResult(HintIntent.Progress);
    }

    public Task<string> PhraseRung(string rung, HintPersona persona) => Task.FromResult(rung);

    public Task<string> PhraseLore(string question, string groundedSource, HintPersona persona) =>
        Task.FromResult(groundedSource);
}
