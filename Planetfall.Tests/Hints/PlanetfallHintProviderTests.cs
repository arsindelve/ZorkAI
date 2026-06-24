using FluentAssertions;
using GameEngine;
using GameEngine.Hints;
using Model.Hints;
using Planetfall.Hints;
using Planetfall.Item.Computer;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Planetfall.Location.Kalamontee.Admin;

namespace Planetfall.Tests.Hints;

/// <summary>
///     Tests the Planetfall provider against real game state: the DAG-localized player-context (what we
///     hand LLM 1) and the docs/proactive wiring. No OpenAI — these assert the deterministic localizer.
/// </summary>
[TestFixture]
public class PlanetfallHintProviderTests : EngineTestsBase
{
    [SetUp]
    public void SetUp() => GetTarget(); // Repository.Reset() + a real PlanetfallContext (Context)

    private static PlanetfallHintProvider Provider() => new();

    [Test]
    public void FreshGame_ContextNamesTheOpeningAsTheActiveBlocker()
    {
        var context = Provider().DescribePlayerContext(Context);
        context.Should().Contain("survive the explosion");
        context.Should().Contain("Score: 0/80");
    }

    [Test]
    public void FloydActivated_ThenItsReflectedAndTheBlockerAdvances()
    {
        Repository.GetItem<Floyd>().HasEverBeenOn = true;
        var context = Provider().DescribePlayerContext(Context);
        context.Should().Contain("alive and with you");
    }

    [Test]
    public void FloydDead_ContextSaysSo_SoLateHintsDontRelyOnHim()
    {
        Repository.GetItem<Floyd>().HasDied = true;
        Provider().DescribePlayerContext(Context).Should().Contain("DEAD");
    }

    [Test]
    public void CommunicationsFixed_BackfillsAndShowsAsAccomplished()
    {
        Repository.GetLocation<SystemsMonitors>().Fixed.Add("KUMUUNIKAASHUNZ");
        var context = Provider().DescribePlayerContext(Context);
        // back-filled: comms done implies the whole tower chain is done, so it's no longer the blocker.
        context.Should().Contain("communications");
        context.Should().NotContain("The next required step blocking them: reach the tower");
    }

    [Test]
    public void CureDone_ShownAsAccomplished()
    {
        Repository.GetItem<Relay>().SpeckDestroyed = true;
        Provider().DescribePlayerContext(Context).Should().Contain("cure the Disease");
    }

    [Test]
    public void Docs_AreSubstantial_AndCoverSolutionLoreAndDeadEnds()
    {
        var docs = Provider().Docs;
        docs.Should().Contain("SOLUTION WALKTHROUGH");
        docs.Should().Contain("The Disease");      // lore
        docs.Should().Contain("reactor");           // dead end
        docs.Should().Contain("infirmary bed");     // death trap
    }

    [Test]
    public void TiredPlayer_ProducesASleepNudge()
    {
        Context.Tired = TiredLevel.Tired;
        var service = new HintService(Provider(), new InMemoryHintMemoryStore(), new NullLlm());
        service.ProactiveNudges(Context).Should().Contain(n => n.Category == "sleep");
    }
}

/// <summary>No-op LLM for the proactive test (which never calls the model).</summary>
internal sealed class NullLlm : IHintLanguageModel
{
    public Task<string> Solve(string docs, string playerContext, string question, HintPersona persona) =>
        Task.FromResult("");

    public Task<string> Reveal(string playerContext, string solution, IReadOnlyList<HintExchange> history,
        string question, HintPersona persona) => Task.FromResult("");
}
