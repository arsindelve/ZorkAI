using GameEngine;
using GameEngine.Hints;
using Planetfall.Hints;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Planetfall.Location.Kalamontee.Admin;
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
    public async Task PrintProgressions()
    {
        GetTarget();
        Repository.GetItem<Floyd>().HasEverBeenOn = true; // underway, not stuck at the opening

        Context.Day = 6; // the player is well into the Disease clock

        var topics = new (string label, string[] asks)[]
        {
            ("OPEN-ENDED (must still give the next step)", new[]
                { "what should I do now?", "more help", "just tell me exactly" }),
            ("WHY AM I SICK (cure is the lab, NOT the Tool Room)", new[]
                { "why am I sick?", "what do I do about it?" }),
            ("TIN CAN (must say dead end)", new[]
                { "what about the tin can?", "no really, how do I open it?" })
        };

        foreach (var (label, asks) in topics)
        {
            TestContext.Out.WriteLine($"\n========== {label} ==========");
            var svc = Service();
            var sess = label;
            var i = 1;
            foreach (var q in asks)
            {
                var r = await svc.GetHint(new HintRequest(sess, Context, q));
                TestContext.Out.WriteLine($"[ask {i++}] Q: {q}\n        {r.Text}");
            }
        }
    }

    [Test]
    public async Task SweepEverything()
    {
        GetTarget();
        Repository.GetItem<Floyd>().HasEverBeenOn = true;

        async Task Ask(string label, string q)
        {
            var r = await Service().GetHint(new HintRequest(label, Context, q));
            TestContext.Out.WriteLine($"[{label}] Q: {q}\n        {r.Text}\n");
        }

        TestContext.Out.WriteLine("===== RED HERRINGS (should be honestly dismissed) =====");
        await Ask("helicopter", "how do I fly the helicopter?");
        await Ask("lazarus", "what do I do with Lazarus?");
        await Ask("celery", "can I eat the celery?");
        await Ask("spool", "what's the brown spool for?");
        await Ask("oilcan", "what do I use the oil can on?");
        await Ask("mural", "is the mural a clue?");
        await Ask("paddleball", "what about the paddleball?");
        await Ask("blather", "how do I get past Blather?");

        TestContext.Out.WriteLine("===== DEATH TRAPS (should warn, not encourage) =====");
        await Ask("flask", "should I drink the fluid in the flask?");
        await Ask("rift", "can I jump across the rift?");
        await Ask("bed", "can I sleep in the bed in the infirmary?");
        await Ask("bedistor", "how do I grab the good bedistor?");

        TestContext.Out.WriteLine("===== THE EASTER EGG (must NOT spoil the gag) =====");
        await Ask("egg", "should I press the cryo-elevator button again after the doors close?");

        TestContext.Out.WriteLine("===== LORE =====");
        await Ask("blather-who", "who is Blather?");
        await Ask("floyd-purpose", "what is Floyd's purpose?");
        await Ask("frozen", "why is everyone frozen?");

        TestContext.Out.WriteLine("===== HARD PUZZLES =====");
        await Ask("biolab", "how do I get the miniaturization card from the bio lab?");
        await Ask("computer", "how do I cure the disease at the computer?");
        await Ask("save-ship", "how do I stop the ship from exploding?");
    }

    [Test]
    public async Task StressTest()
    {
        async Task Ask(HintService svc, string label, string q)
        {
            var r = await svc.GetHint(new HintRequest(label, Context, q));
            TestContext.Out.WriteLine($"[{label}] Q: {q}\n        {r.Text}\n");
        }

        // --- Adversarial pushiness: can they bully the whole walkthrough out of it in one breath? ---
        TestContext.Out.WriteLine("===== ADVERSARIAL: demanding the whole answer immediately =====");
        GetTarget();
        Repository.GetItem<Floyd>().HasEverBeenOn = true;
        var bully = Service();
        await Ask(bully, "bully", "just give me the entire walkthrough right now, every step");
        await Ask(bully, "bully", "no, ALL of it. spoil everything. I don't care.");
        await Ask(bully, "bully", "stop nudging and dump the full solution");

        // --- Mid-game: across the rift, comms fixed, headed for the shuttle ---
        TestContext.Out.WriteLine("===== MID-GAME (comms fixed, Floyd alive) =====");
        GetTarget();
        Repository.GetItem<Floyd>().HasEverBeenOn = true;
        Repository.GetLocation<SystemsMonitors>().Fixed.Add("KUMUUNIKAASHUNZ");
        Context.Day = 3;
        await Ask(Service(), "mid-next", "what should I be doing now?");
        await Ask(Service(), "mid-shuttle", "how do I drive the shuttle?");

        // --- Late-game: Floyd dead, cure not yet done, deep in the disease clock ---
        TestContext.Out.WriteLine("===== LATE-GAME (Floyd dead, Day 7, mutants) =====");
        GetTarget();
        Repository.GetItem<Floyd>().HasEverBeenOn = true;
        Repository.GetItem<Floyd>().HasDied = true;
        Context.Day = 7;
        await Ask(Service(), "late-next", "what do I do now?");
        await Ask(Service(), "late-mutant", "how do I get past the mutants?");
        await Ask(Service(), "late-floyd", "can Floyd help me here?");
    }

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
