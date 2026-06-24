using System.Reflection;
using System.Text;
using GameEngine;
using GameEngine.Hints;
using Model.Hints;
using Model.Interface;
using Planetfall.Item.Computer;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Planetfall.Location.Kalamontee.Admin;

namespace Planetfall.Hints;

/// <summary>
///     Planetfall's plug-in for the two-tier hint engine. There is no hand-written knowledge base: the
///     knowledge IS the game. LLM 1 (Solve) reasons over the complete C# source + the CI-verified
///     walkthrough tests + an explicit pointer at the in-game lore; the player's situation is the actual
///     serialized save game (complete by construction). The only thing left that isn't a real game
///     artifact is the survival proactive rules.
/// </summary>
public sealed class PlanetfallHintProvider : IHintProvider
{
    private static readonly Lazy<string> Knowledge = new(BuildKnowledge);

    public string Docs => Knowledge.Value;

    public HintPersona Persona => new(
        "You are the invisible, incorporeal narrator of the Infocom game Planetfall — dry, lightly " +
        "sarcastic, in character. Never mention being an AI; never break the fourth wall about 'hints'.");

    public IReadOnlyList<IProactiveRule> ProactiveRules { get; } = new IProactiveRule[]
    {
        new TiredRule(), new HungerRule(), new DiseaseRule()
    };

    /// <summary>
    ///     The player's situation, fed to both tiers. The complete serialized save game is the ground
    ///     truth — but a raw 100KB JSON buries decision-critical flags, and the model was observed to miss
    ///     them (e.g. answering as if a dead Floyd were alive). So we lead with a small KEY-STATE highlight
    ///     of the few flags that change the answer, computed live off the same game objects (not a hand
    ///     summary that can drift), then attach the full JSON behind it for completeness.
    /// </summary>
    public string DescribePlayerContext(IContext state) =>
        DescribeKeyState(state) +
        "\n\nFULL SERIALIZED SAVE GAME (complete state, authoritative for anything not above):\n" +
        (state.Engine?.SaveGame() ?? "(unavailable)");

    public string DescribeKeyState(IContext state)
    {
        // Reads the global Repository singletons. This mirrors the engine's existing model of one game
        // context per process; the hint path is read-only and adds no new concurrency assumption beyond
        // what the rest of the engine already relies on.
        var floyd = Repository.GetItem<Floyd>();
        var sys = Repository.GetLocation<SystemsMonitors>();
        var sb = new StringBuilder();
        sb.AppendLine("KEY STATE (read this FIRST — it governs your answer):");
        sb.AppendLine($"- Current location: {state.CurrentLocation?.Name ?? "unknown"}");
        sb.AppendLine($"- Floyd: {(floyd.HasDied
            ? "DEAD — he died at the bio lab. He is GONE and can no longer help with anything; never tell the player to use Floyd."
            : floyd.HasEverBeenOn ? "alive and accompanying the player"
            : "not yet activated")}");
        sb.AppendLine($"- The Disease cure (laser the microbe): {(Repository.GetItem<Relay>().SpeckDestroyed ? "ALREADY DONE" : "not done")}");
        sb.AppendLine($"- Planetary systems: communications {(sys.CommunicationsFixed ? "FIXED" : "not fixed")}, " +
                      $"defense {(sys.PlanetaryDefenseFixed ? "FIXED" : "not fixed")}, course {(sys.CourseControlFixed ? "FIXED" : "not fixed")}");
        if (state is PlanetfallContext c)
            sb.AppendLine($"- Day {c.Day}; health: {c.SicknessDescription} hunger {c.Hunger}; tired {c.Tired}");
        return sb.ToString();
    }

    // -- knowledge bundle: framing preamble + the real source + the real walkthroughs ----------------

    private const string Preamble =
        "You are reading the COMPLETE C# source of the game Planetfall, plus its end-to-end WALKTHROUGH " +
        "TESTS. This is the only ground truth — reason over it; never use outside knowledge of the game.\n\n" +
        "PLAYER SITUATION: the player's current state is given to you as the game's own serialized SAVE GAME " +
        "(JSON of every object's mutable state — inventory, flags, locations). Read it to know exactly what " +
        "they have done and have; answer for where they actually are.\n\n" +
        "WALKTHROUGHS: each line of the form [TestCase(\"<command>\", _, \"<expected response>\")] is ONE exact, " +
        "correct game command in order. These tests run on every build, so they are the proven solution path — " +
        "prefer the EXACT commands shown there, in that order.\n\n" +
        "LORE: the canonical backstory — the planet's history, the Disease, the cryogenic Project, the culture " +
        "and technology — lives in the Lawanda library computer menus (Item/Lawanda/Library/Computer/: History, " +
        "Project, Culture, Geography, Technology), the spools, and the Feinstein Diary. For any 'why/what/who/" +
        "history/backstory' question, draw from THOSE. That in-game text is written in Planetfall's deliberately " +
        "corrupted far-future phonetic English (e.g. \"Foor moor deetaald infoormaashun\" = \"For more detailed " +
        "information\") — interpret its meaning and answer in normal modern English; never quote the garbled " +
        "spelling back to the player.\n\n";

    private static string BuildKnowledge()
    {
        // Production: a single text bundle is embedded at build time (Lambda has no .cs on disk).
        var asm = typeof(PlanetfallHintProvider).Assembly;
        var resource = asm.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith("HintKnowledge.g.txt"));
        if (resource is not null)
        {
            using var stream = asm.GetManifestResourceStream(resource)!;
            using var reader = new StreamReader(stream);
            return Preamble + reader.ReadToEnd();
        }

        // Dev/test fallback: read straight from the repo working tree.
        return Preamble + BuildFromDisk();
    }

    private static string BuildFromDisk()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Zork.sln"))) dir = dir.Parent;
        if (dir is null) return "(source not found)";

        var game = Path.Combine(dir.FullName, "Planetfall");
        var walk = Path.Combine(dir.FullName, "Planetfall.Tests", "Walkthrough");

        var sb = new StringBuilder();
        sb.AppendLine("Part 1 — GAME SOURCE (mechanics, objects, rooms, exact verbs):\n");
        foreach (var f in Directory.GetFiles(game, "*.cs", SearchOption.AllDirectories))
            sb.Append("// ===== ").Append(Path.GetRelativePath(game, f)).Append(" =====\n")
              .Append(File.ReadAllText(f)).Append("\n\n");

        sb.AppendLine("\nPart 2 — VERIFIED WALKTHROUGHS (the proven solution path):\n");
        if (Directory.Exists(walk))
            foreach (var f in Directory.GetFiles(walk, "*.cs"))
                sb.Append("// ===== ").Append(Path.GetFileName(f)).Append(" =====\n")
                  .Append(File.ReadAllText(f)).Append("\n\n");

        return sb.ToString();
    }

    // -- survival proactive rules (push channel; higher priority = more urgent) ----------------------

    private sealed class TiredRule : IProactiveRule
    {
        public ProactiveNudge? Evaluate(IContext s) =>
            s is PlanetfallContext c && (int)c.Tired >= 1
                ? new ProactiveNudge("sleep", "You're getting tired — find a safe place to sleep (a dorm bunk).", 3)
                : null;
    }

    private sealed class HungerRule : IProactiveRule
    {
        public ProactiveNudge? Evaluate(IContext s) =>
            s is PlanetfallContext c && (int)c.Hunger >= 1
                ? new ProactiveNudge("hunger", "You're getting hungry and thirsty — find food and water.", 3)
                : null;
    }

    private sealed class DiseaseRule : IProactiveRule
    {
        public ProactiveNudge? Evaluate(IContext s) =>
            s is PlanetfallContext c && c.Day >= 4 && !Repository.GetItem<Relay>().SpeckDestroyed
                ? new ProactiveNudge("disease", "You're getting sicker by the day — the cure is in the lab.", 5)
                : null;
    }
}
