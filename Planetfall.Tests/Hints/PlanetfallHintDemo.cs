using GameEngine;
using GameEngine.Hints;
using Planetfall.Hints;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using ZorkAI.OpenAI;

namespace Planetfall.Tests.Hints;

/// <summary>Live demo — prints real two-tier output across scenarios. [Explicit], needs OPEN_AI_KEY.</summary>
[TestFixture]
[Explicit("Live OpenAI demo — requires OPEN_AI_KEY")]
public class PlanetfallHintDemo : EngineTestsBase
{
    private HintService Service() =>
        new(new PlanetfallHintProvider(), new InMemoryHintMemoryStore(), new OpenAiHintLanguageModel());

    [Test]
    public async Task PrintTranscript()
    {
        void H(string s) => TestContext.Out.WriteLine("\n========== " + s + " ==========");

        async Task Ask(HintService svc, string sess, string q)
        {
            var r = await svc.GetHint(new HintRequest(sess, Context, q));
            TestContext.Out.WriteLine($"Q: {q}\n  {r.Text}");
        }

        // Same puzzle, three escalating asks in one session — disclosure paced by chat history.
        H("Progressive disclosure (one session, three asks)");
        GetTarget();
        var ladder = Service();
        await Ask(ladder, "L", "I'm stuck, what should I do?");
        await Ask(ladder, "L", "I need more help");
        await Ask(ladder, "L", "ok just tell me exactly");

        // Dead end, lore, mechanic, misconception, mutants late-game (Floyd dead) — all via Solve+Reveal.
        H("Other modes");
        GetTarget();
        await Ask(Service(), "A", "is the reactor important?");
        GetTarget();
        await Ask(Service(), "B", "why is everything deserted?");
        GetTarget();
        Context.Day = 5;
        await Ask(Service(), "C", "why am I getting sick?");
        GetTarget();
        await Ask(Service(), "D", "how do I get off this planet?");
        GetTarget();
        Repository.GetItem<Floyd>().HasDied = true; // aux booth: Floyd long dead
        await Ask(Service(), "E", "how do I get past the mutants?");
    }
}
