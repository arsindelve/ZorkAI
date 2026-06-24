using GameEngine;
using GameEngine.Hints;
using Model.Hints;
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
    public async Task GameSpanLoop()
    {
        int n = 0;
        async Task Ask(string area, string q, Action<PlanetfallContext>? state = null)
        {
            GetTarget();
            state?.Invoke(Context);
            var r = await Service().GetHint(new HintRequest($"s{++n}", Context, q));
            TestContext.Out.WriteLine($"#{n} [{area}] Q: {q}\n   -> {r.Text}\n");
        }

        void Floyd(PlanetfallContext c) => Repository.GetItem<Floyd>().HasEverBeenOn = true;
        void Lawanda(PlanetfallContext c) { Floyd(c); Repository.GetItem<Floyd>().HasGottenTheFromitzBoard = true; }
        void FloydDead(PlanetfallContext c) { Floyd(c); Repository.GetItem<Floyd>().HasDied = true; }

        await Ask("OPENING", "the ship is exploding, what do I do?");
        await Ask("LANDING", "I'm in the escape pod on the ground, now what?");
        await Ask("FLOYD", "how do I wake up the robot?");
        await Ask("CREVICE", "I see a crevice in the wall, what now?", Floyd);
        await Ask("PADLOCK", "how do I open the padlocked door?", Floyd);
        await Ask("RIFT", "there's a deep rift blocking me, how do I cross?", Floyd);
        await Ask("OFFICES", "where do I find the access cards?", Floyd);
        await Ask("KITCHEN", "how do I get into the kitchen?", Floyd);
        await Ask("SURVIVAL-EAT", "I'm starving, where's food?", c => { Floyd(c); c.Hunger = HungerLevel.Hungry; });
        await Ask("SURVIVAL-SLEEP", "I'm exhausted, where can I sleep?", c => { Floyd(c); c.Tired = TiredLevel.Tired; });
        await Ask("TOWER", "how do I get up the tower / upper elevator?", Floyd);
        await Ask("COMMS", "how do I fix the communications?", c => { Floyd(c); c.Day = 2; });
        await Ask("SHUTTLE", "how do I take the shuttle to the other complex?", c => { Floyd(c); c.Day = 3; });
        await Ask("DEFENSE", "how do I fix the planetary defense?", Lawanda);
        await Ask("COURSE", "how do I fix the course / the cube and bedistor?", Lawanda);
        await Ask("LASER", "what do I do with the laser?", Lawanda);
        await Ask("BIOLAB", "how do I get the card past the mutations in the bio lab?", Lawanda);
        await Ask("CURE", "I have the mini card and laser, how do I cure the disease?", c => { Lawanda(c); c.Day = 5; });
        await Ask("MUTANTS", "the mutants are chasing me, what do I do?", c => { FloydDead(c); c.Day = 6; });
        await Ask("WON?", "the cryo doors just closed, did I win? what now?", c => { FloydDead(c); c.Day = 6; });
    }

    [Test]
    public async Task SourceVsProse_RiftPuzzle()
    {
        // Find the repo root (walk up to Zork.sln), then read the ACTUAL source for the rift closure.
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Zork.sln"))) dir = dir.Parent;
        var root = dir!.FullName;

        // Full dependency closure for "cross the rift from scratch": the crossing, the ladder, the
        // storeroom + padlock that hold it, and the crevice -> steel key -> magnet chain that unlocks it.
        string[] files =
        {
            "Planetfall/Location/Kalamontee/Admin/AdminCorridor.cs",
            "Planetfall/Location/Kalamontee/Admin/RiftLocationBase.cs",
            "Planetfall/Item/Kalamontee/Ladder.cs",
            "Planetfall/Location/Kalamontee/StorageWest.cs",
            "Planetfall/Item/Kalamontee/Padlock.cs",
            "Planetfall/Location/Kalamontee/Admin/AdminCorridorSouth.cs",
            "Planetfall/Item/Kalamontee/Admin/Key.cs",
            "Planetfall/Item/Kalamontee/Mech/Magnet.cs"
        };
        var sourceBundle = "You are looking at the ACTUAL C# source of this game. Reason over it as ground truth.\n\n" +
            string.Join("\n\n", files.Select(f => $"// ===== {f} =====\n{File.ReadAllText(Path.Combine(root, f.Replace('/', Path.DirectorySeparatorChar)))}"));

        var llm = new OpenAiHintLanguageModel();
        var provider = new PlanetfallHintProvider();
        var persona = provider.Persona;

        string[] questions =
        {
            "how do I cross the rift?",
            "I tried to put the ladder across the rift but it fell in and is gone. what did I do wrong?",
            "how do I get the ladder out of the storeroom?"
        };

        foreach (var q in questions)
        {
            GetTarget();
            Repository.GetItem<Floyd>().HasEverBeenOn = true;
            var context = provider.DescribePlayerContext(Context);

            // SOURCE: feed the real code as the knowledge base, same two-tier flow.
            var solution = await llm.Solve(sourceBundle, context, new List<HintExchange>(), q, persona);
            var srcHint = await llm.Reveal(context, solution, new List<HintExchange>(), q, persona);

            // PROSE: the current PlanetfallHintDocs-based path.
            var prose = await new HintService(provider, new InMemoryHintMemoryStore(), llm)
                .GetHint(new HintRequest("prose", Context, q));

            TestContext.Out.WriteLine($"Q: {q}\n  [SOURCE] {srcHint}\n  [PROSE ] {prose.Text}\n");
        }
    }

    [Test]
    public async Task WholeSource_NoScoping()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Zork.sln"))) dir = dir.Parent;
        var planetfall = Path.Combine(dir!.FullName, "Planetfall");

        // Load the ENTIRE Planetfall game source. No scoping, no DAG->files map, no curation.
        var allFiles = Directory.GetFiles(planetfall, "*.cs", SearchOption.AllDirectories);
        var sourcePart = string.Join("\n\n", allFiles.Select(f =>
            $"// ===== {Path.GetRelativePath(planetfall, f)} =====\n{File.ReadAllText(f)}"));

        // The walkthrough TESTS are part of the source too — and they ARE the verified solution:
        // each [TestCase("command", _, "expected response")] is one exact, correct move in order.
        var walkDir = Path.Combine(dir.FullName, "Planetfall.Tests", "Walkthrough");
        var walkFiles = Directory.GetFiles(walkDir, "*.cs");
        var walkPart = string.Join("\n\n", walkFiles.Select(f =>
            $"// ===== {Path.GetFileName(f)} =====\n{File.ReadAllText(f)}"));

        var bundle =
            "You are reading the COMPLETE C# source of the game Planetfall, plus its end-to-end WALKTHROUGH " +
            "TESTS. This is the only ground truth — reason over it; never use outside knowledge of the game.\n\n" +
            "LORE: the game's canonical backstory — the planet's history, the Disease, the cryogenic Project, " +
            "the culture and technology — lives in the Lawanda LIBRARY COMPUTER menus " +
            "(Item/Lawanda/Library/Computer/: HistoryMenu, ProjectMenu, CultureMenu, GeographyMenu, " +
            "TechnologyMenu, MainMenu), the spools (Red/Green/BrownSpool), and the Feinstein Diary. For ANY " +
            "'why/what/who/history/backstory' question, draw your answer from THOSE files. IMPORTANT: that " +
            "in-game text is written in Planetfall's deliberately corrupted far-future phonetic English (e.g. " +
            "\"Foor moor deetaald infoormaashun\" = \"For more detailed information\"). INTERPRET its meaning " +
            "and answer in normal modern English — never quote the garbled spelling back to the player.\n\n" +
            "Part 1 — GAME SOURCE (the mechanics, objects, rooms, exact verbs):\n\n" + sourcePart +
            "\n\n========================================================================\n" +
            "Part 2 — VERIFIED WALKTHROUGHS. These NUnit tests play the game start to finish and are run on " +
            "every build, so they are the canonical, proven-correct solution path. Each line of the form " +
            "[TestCase(\"<command>\", _, \"<expected response substring>\")] is ONE exact game command, in " +
            "order, with a snippet of its expected output. When you work out steps, prefer the EXACT commands " +
            "shown here and keep them in this order.\n\n" + walkPart;

        TestContext.Out.WriteLine($"[loaded {allFiles.Length} source + {walkFiles.Length} walkthrough files, {bundle.Length:N0} chars (~{bundle.Length / 4000}k tokens)]\n");

        var llm = new OpenAiHintLanguageModel(model: "gpt-5.4-mini");
        var provider = new PlanetfallHintProvider();
        var persona = provider.Persona;
        var empty = new List<HintExchange>();

        // The puzzles that kept breaking: rift mechanics + the two-elevator / shuttle confusion.
        string[] questions =
        {
            "I put the ladder across the rift but it fell in and is gone. what did I do wrong?",
            "how do I get up the tower to fix communications?",
            "how do I take the shuttle to the other complex?",
            "what's the difference between the upper and lower elevators?",
            "is the reactor important?",
            "what actually happened to all the people who lived here?",
            "what was the Project, and did it work?"
        };

        foreach (var q in questions)
        {
            GetTarget();
            Repository.GetItem<Floyd>().HasEverBeenOn = true;
            var context = provider.DescribePlayerContext(Context);
            var solution = await llm.Solve(bundle, context, empty, q, persona);
            if (string.IsNullOrWhiteSpace(solution))
            {
                TestContext.Out.WriteLine($"########## Q: {q}\n!!! SOLVE EMPTY — model id likely rejected by the API\n");
                continue;
            }
            var hint = await llm.Reveal(context, solution, empty, q, persona);
            TestContext.Out.WriteLine($"########## Q: {q}");
            TestContext.Out.WriteLine($"=== LLM1 SOLVE (full internal solution) ===\n{solution}");
            TestContext.Out.WriteLine($"=== LLM2 REVEAL (what the player sees) ===\n{hint}\n");
        }
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
