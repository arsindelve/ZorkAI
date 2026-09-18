using System.Text;
using GameEngine;
using GameEngine.Hints;
using GameEngine.Hints.Data;
using Model.Hints;
using Model.Interface;
using Planetfall.Item.Computer;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Planetfall.Location.Kalamontee.Admin;

namespace Planetfall.Hints;

/// <summary>
///     Planetfall's plug-in for the hint engine (Docs/hints/07). Supplies the puzzle DAG and its
///     progress mapper, the authored rung ladders, the tier-gated lore source, the soft-lock and survival
///     rules, the narrator persona — and, for the fallback solver, the complete game source plus the
///     verified walkthrough, with the player's actual serialized save game as their situation.
/// </summary>
public sealed class PlanetfallHintProvider : IHintProvider
{
    private static readonly PlanetfallPuzzleGraph Graph = new();
    private static readonly Lazy<string> Knowledge = new(BuildKnowledge);
    private static readonly PlanetfallGame Game = new();

    /// <summary>
    ///     The production provider: the generated corpus (Planetfall.Tests/Hints/Generator, checked in as
    ///     Hints/Generated/planetfall-hints.json). Falls back to the hand-written graph and ladders only when
    ///     no corpus is embedded.
    /// </summary>
    public PlanetfallHintProvider() : this(LoadGenerated())
    {
    }

    /// <summary>A given corpus (the generated data), or null for the hand-written graph and ladders.</summary>
    public PlanetfallHintProvider(HintData? corpus)
    {
        if (corpus is null)
        {
            PuzzleGraph = Graph;
            ProgressMapper = Graph; // the graph owns the node definitions, so it maps too
            PuzzleCorpus = new PlanetfallHintCorpus();
            return;
        }

        var graph = new DataPuzzleGraph(corpus);
        PuzzleGraph = graph;
        ProgressMapper = graph;
        PuzzleCorpus = new DataHintCorpus(corpus);
    }

    /// <summary>The hand-written puzzle graph and ladders (Docs/hints/planetfall/01 and 06): the baseline the
    ///     generated corpus is measured against, and the fallback when nothing is embedded.</summary>
    public static PlanetfallHintProvider HandWritten() => new((HintData?)null);

    public bool IsGenerated => PuzzleGraph is DataPuzzleGraph;

    /// <summary>The checked-in generated corpus, if one is embedded (Hints/Generated/planetfall-hints.json); parsed once.</summary>
    public static HintData? LoadGenerated() => Generated.Value;

    private static readonly Lazy<HintData?> Generated = new(() =>
    {
        var asm = typeof(PlanetfallHintProvider).Assembly;
        var name = asm.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith("planetfall-hints.json", StringComparison.OrdinalIgnoreCase));
        if (name is null) return null;
        using var stream = asm.GetManifestResourceStream(name)!;
        using var reader = new StreamReader(stream);
        return HintData.FromJson(reader.ReadToEnd());
    });

    public IPuzzleGraph PuzzleGraph { get; }
    public IProgressMapper ProgressMapper { get; }
    public IHintCorpus PuzzleCorpus { get; }
    public ILoreSource LoreSource { get; } = new PlanetfallLoreSource();
    public IReadOnlyList<ISoftLockRule> SoftLockRules => PlanetfallHintRules.SoftLocks;
    public IReadOnlyList<IProactiveRule> ProactiveRules => PlanetfallHintRules.Proactive;

    // The game's identity and its decision-critical flags ride on the persona: the hint LLM implementation
    // is shared by every game, so it can't know either of them (#484).
    public HintPersona Persona => new(
        "You are the invisible narrator of the Infocom game Planetfall: dry, warm, a little wry, and quietly on " +
        "the player's side. Stay in character. Never mention being an AI, a system, or 'hints'. A joke may " +
        "decorate an answer, but it never replaces one.",
        Game.GameName,
        "what's done, Floyd alive/dead, their health");

    public string Docs => Knowledge.Value;

    /// <summary>
    ///     The player's situation for the fallback solver. The complete serialized save game is the ground
    ///     truth — but a raw 100KB JSON buries decision-critical flags, and the model was observed to miss
    ///     them (e.g. answering as if a dead Floyd were alive). So we lead with the KEY-STATE highlight, then
    ///     attach the full JSON behind it for completeness.
    /// </summary>
    public string DescribePlayerContext(IContext state)
    {
        return DescribeKeyState(state) +
               "\n\nFULL SERIALIZED SAVE GAME (complete state, authoritative for anything not above):\n" +
               (state.Engine?.SaveGame() ?? "(unavailable)");
    }

    public string DescribeKeyState(IContext state)
    {
        // Reads the global Repository singletons. This mirrors the engine's existing model of one game
        // context per process; the hint path is read-only.
        var floyd = Repository.GetItem<Floyd>();
        var sys = Repository.GetLocation<SystemsMonitors>();
        var sb = new StringBuilder();
        sb.AppendLine("KEY STATE (read this FIRST — it governs your answer):");
        sb.AppendLine($"- Current location: {state.CurrentLocation?.Name ?? "unknown"}");
        sb.AppendLine($"- Floyd: {(floyd.HasDied
            ? "DEAD — he died at the bio lab. He is GONE and can no longer help with anything; never tell the player to use Floyd."
            : floyd.HasEverBeenOn ? "alive and accompanying the player"
            : "not yet activated")}");
        sb.AppendLine(
            $"- The Disease cure (laser the microbe): {(Repository.GetItem<Relay>().SpeckDestroyed ? "ALREADY DONE" : "not done")}");
        sb.AppendLine($"- Planetary systems: communications {(sys.CommunicationsFixed ? "FIXED" : "not fixed")}, " +
                      $"defense {(sys.PlanetaryDefenseFixed ? "FIXED" : "not fixed")}, course {(sys.CourseControlFixed ? "FIXED" : "not fixed")}");
        if (state is PlanetfallContext c)
            sb.AppendLine($"- Day {c.Day}; health: {c.SicknessDescription} hunger {c.Hunger}; tired {c.Tired}");
        return sb.ToString();
    }

    // -- knowledge bundle for the fallback solver: preamble + the real source + the real walkthrough ---

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
        // The game's own .cs files + the one complete walkthrough are embedded as resources at build time
        // (see Planetfall.csproj). We assemble the bundle from them here, in C#.
        var asm = typeof(PlanetfallHintProvider).Assembly;
        var sources = asm.GetManifestResourceNames()
            .Where(n => n.EndsWith(".cs", StringComparison.Ordinal))
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToList();

        string Read(string name)
        {
            using var stream = asm.GetManifestResourceStream(name)!;
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        void Append(StringBuilder sb, IEnumerable<string> names)
        {
            foreach (var n in names)
                sb.Append("// ===== ").Append(n).Append(" =====\n").Append(Read(n)).Append("\n\n");
        }

        const string walkthrough = "WalkthroughTestOne.cs";
        var sb = new StringBuilder(Preamble);
        sb.AppendLine("Part 1 - GAME SOURCE (mechanics, objects, rooms, exact verbs):\n");
        Append(sb, sources.Where(n => !n.EndsWith(walkthrough, StringComparison.Ordinal)));
        sb.AppendLine("\nPart 2 - THE COMPLETE VERIFIED WALKTHROUGH (the proven solution path, start to finish):\n");
        Append(sb, sources.Where(n => n.EndsWith(walkthrough, StringComparison.Ordinal)));
        return sb.ToString();
    }
}
