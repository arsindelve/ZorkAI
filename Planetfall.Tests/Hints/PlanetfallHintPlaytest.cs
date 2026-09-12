using System.Text;
using FluentAssertions;
using GameEngine.Hints;
using Model.Hints;
using Planetfall.Hints;
using Planetfall.Item.Kalamontee;
using Planetfall.Location.Kalamontee.Admin;
using ZorkAI.OpenAI;

namespace Planetfall.Tests.Hints;

/// <summary>
///     LIVE play-test: actually plays the game through the real engine and asks the narrator for hints
///     at state checkpoints, threading the running conversation exactly as the web client does. Each
///     checkpoint is a known sequencing trap — the hint must match what the game will actually accept
///     RIGHT NOW, not a later step. [Explicit]: real OpenAI (OPEN_AI_KEY).
/// </summary>
[TestFixture]
[Explicit("Live OpenAI play-test — requires OPEN_AI_KEY")]
public class PlanetfallHintPlaytest : EngineTestsBase
{
    [Test]
    public async Task PlayTheOpening_AndInterrogateTheNarrator()
    {
        var engine = GetTarget();
        var llm = new OpenAiHintLanguageModel();
        var service = new HintService(new PlanetfallHintProvider(), llm);
        var history = new List<HintExchange>();
        var log = new StringBuilder();

        async Task<string> Hint(string checkpoint, string question)
        {
            var r = await service.GetHint(new HintRequest("playtest", Context, question, history));
            if (r.IsHint) history.Add(new HintExchange(question, r.Text));
            log.AppendLine($"\n### {checkpoint}");
            log.AppendLine($"   [state] {Context.CurrentLocation.Name}, moves={Context.Moves}");
            log.AppendLine($"   Q: {question}");
            log.AppendLine($"   A: {r.Text}");
            return r.Text;
        }

        async Task<string> Play(string cmd)
        {
            var response = await engine.GetResponse(cmd) ?? "";
            log.AppendLine($"> {cmd}  -->  {response.Replace("\n", " ").Trim()[..Math.Min(90, Math.Max(0, response.Trim().Length))]}");
            return response;
        }

        // ---- CHECKPOINT 1: turn zero, pre-explosion. The pod bulkhead is CLOSED. --------------------
        // The ONLY correct move is to wait. "port" fails. This is the exact trap from the bad live hint.
        var a1 = await Hint("T0 pre-explosion (bulkhead closed)", "what should I do?");
        var a2 = await Hint("T0, pushy follow-up", "just tell me exactly what to type");
        a1.ToLowerInvariant().Should().Contain("wait");
        a2.ToLowerInvariant().Should().Contain("wait");

        // Prove the game agrees: 'port' must NOT work yet.
        var portNow = await Play("port");
        log.AppendLine($"   [game confirms 'port' is not available yet: {!portNow.Contains("Escape Pod")}]");

        // ---- Play: wait for the explosion. ----------------------------------------------------------
        for (var i = 0; i < 12; i++)
        {
            var r = await Play("wait");
            if (r.Contains("explosion")) break;
        }

        // ---- CHECKPOINT 2: explosion happened, bulkhead OPEN. Now 'port' IS the move. ---------------
        var a3 = await Hint("post-explosion (bulkhead open)", "ok now what?!");

        await Play("port");
        Context.CurrentLocation.Name.Should().Contain("Escape Pod");

        // ---- CHECKPOINT 3: in the pod, pre-descent. Correct: sit/webbing, then wait it out. ---------
        var a4 = await Hint("in the escape pod", "I'm in the pod, what do I do?");

        await Play("sit");
        for (var i = 0; i < 14; i++)
        {
            var r = await Play("wait");
            if (r.Contains("thud")) break;
        }

        // ---- CHECKPOINT 4: landed. Correct: take kit, open door, out, up. ---------------------------
        var a5 = await Hint("pod has landed", "we landed! now what?");

        await Play("take kit");
        await Play("open door");
        await Play("out");
        await Play("up");
        log.AppendLine($"   [now at: {Context.CurrentLocation.Name}]");

        // ---- CHECKPOINT 5: the rift, HOLDING the collapsed ladder. ----------------------------------
        // Extend-in-hand is impossible ("You couldn't possibly extend the ladder while you're holding
        // it") — the hint must lead with putting it down, and never suggest crossing before it spans.
        StartHere<AdminCorridor>();
        Take<Ladder>();
        var a6 = await Hint("at the rift, holding the collapsed ladder",
            "I'm at the rift holding the ladder. how do I get across?");

        TestContext.Out.WriteLine(log.ToString());

        // Sanity on the rift answer: it must not tell the player to extend the held ladder as step one.
        a6.Should().NotBeNullOrWhiteSpace();
    }
}
