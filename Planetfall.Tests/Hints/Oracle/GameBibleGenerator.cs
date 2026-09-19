using System.Text;
using GameEngine.Hints.Data;
using Newtonsoft.Json;
using Planetfall.Tests.Walkthrough;
using ZorkAI.OpenAI;

namespace Planetfall.Tests.Hints.Oracle;

/// <summary>
///     Writes the Planetfall game bible — the hint oracle's understanding of the game — the way a person would
///     come to understand it: by playing it through (the verified walkthrough, replayed through the real engine,
///     every response captured), by reading how it works (the game's source, area by area), and by reading the
///     official hint book (the invisiclues) and the lore notes. A strong model digests each source area into
///     notes, then writes the bible section by section from the notes plus the full playthrough.
///     Output: Planetfall/Hints/Oracle/planetfall-bible.md and planetfall-initial-state.json. Review, check in.
///     [Explicit]: many large calls to the strongest model (OPEN_AI_KEY; HINT_ORACLE_MODEL to override).
/// </summary>
[TestFixture]
[Explicit("Writes the game bible — large calls to the strongest OpenAI model (OPEN_AI_KEY)")]
public class GameBibleGenerator : WalkthroughTestBase
{
    private static string Model =>
        Environment.GetEnvironmentVariable("HINT_ORACLE_MODEL") is { Length: > 0 } m ? m : OpenAiHintOracle.DefaultModel;

    private static readonly (string Title, string Ask)[] Sections =
    {
        ("How the game plays, and what the player is up against",
            "The premise as the player experiences it; how the parser game is played and the verbs that matter here; scoring; " +
            "the carrying limit; and above all the clocks: days and time, hunger and thirst and what can be eaten or drunk and where, " +
            "sleep and where it is safe, the Disease and how it progresses and what it does to the player, anything that changes " +
            "with the day (flooding, failures). For each clock: the warning signs the player sees, how long they have, what kills them, what helps."),
        ("The story, and when the player can know each part of it",
            "The full truth of the story: the Feinstein, the planet Resida, its people, the Disease, the Project, what happened to everyone, " +
            "why the Feinstein was destroyed, what the ending means. Then — crucially — a timeline of revelation: for each piece of the truth, " +
            "exactly where and when in the game the player learns it (or can first infer it), so a guide knows what would be a spoiler at any " +
            "given point. Translate the game's corrupted phonetic English into plain English; note that the player sees the corrupted form."),
        ("Geography: the places and how they connect",
            "Every region and its rooms, how they connect (directions), what is notable in each room, and the transport systems: the elevators, " +
            "the shuttle, the teleport booths — how each is operated and which access card opens what and where each card is found. " +
            "Written so a guide can tell a lost player how to get from anywhere to anywhere."),
        ("Act one: the Feinstein, the escape pod, the landing, and reaching the complex",
            "Everything from the first turn to arriving in the Kalamontee complex. For every situation the player faces: what they see, " +
            "what the idea is, the exact commands that work, what goes wrong if they dawdle or choose badly, and what players typically misunderstand."),
        ("Act two: Kalamontee",
            "Every puzzle and useful object in the Kalamontee complex — tools, Floyd's activation, the key in the crevice, the padlocked door, the " +
            "ladder and the rift, the offices and the access cards, the kitchen, the flask and the machine shop's fluids, the elevators, the tower " +
            "and communications repair, and the way down to the shuttle. For each: what the player sees, the underlying idea and why the solution " +
            "makes sense, the exact working commands, the order constraints (what must come first), what is optional, and the usual confusions."),
        ("Act three: Lawanda, the repairs, the computer, and the endgame",
            "Everything in Lawanda: the repair room and the fromitz board, planetary defense, course control and the bedistor, the library and " +
            "terminals, the teleport booths, the laser and its batteries, the bio lock and what happens there, the computer room, miniaturization " +
            "and the relay (speck, microbe), the lab office and the gas mask, the mutants and the run to the cryo-elevator, and the endings " +
            "(which repairs are needed for the best ending and which are optional). Same treatment: what is seen, the idea, exact commands, " +
            "order constraints, what is optional, the usual confusions."),
        ("Floyd",
            "Who Floyd is and how he behaves; how he is switched on and how long it takes; what he can be asked to do and the exact phrasing; " +
            "the places where he does something special; what he reveals; when and why he dies and what it means for the player afterwards; " +
            "what happens at the ending. What a guide should and should not give away about him, and when."),
        ("Dead ends, red herrings, ways to die, and ways to make the game unwinnable",
            "Every object, room and device that looks important but is not needed (and what to tell a player who asks about it). Every way to die. " +
            "Every way to make the game unwinnable or to lock oneself out of the best ending, how a guide can recognise from the player's state that " +
            "it has happened or is about to, and what to tell them.")
    };

    [Test]
    public async Task WriteTheBible()
    {
        var root = WalkthroughSource.RepoRoot();
        var outDir = Path.Combine(root, "Planetfall", "Hints", "Oracle");
        Directory.CreateDirectory(outDir);

        // 1. Play the game.
        StartOver();
        var playthrough = await Playthrough();
        await WriteBaseline();
        TestContext.Out.WriteLine($"Playthrough: {playthrough.Length:N0} chars.");

        // 2. Read the source, area by area, into notes.
        var areas = SourceAreas(root);
        var notes = await Task.WhenAll(areas.Select(async a =>
        {
            var text = await Ask(
                "You are studying the source code of a text adventure in order to understand the game completely, as a player would " +
                "experience it. Write thorough study notes in plain prose for someone who will later coach players. Cover, for this part " +
                "of the game: every room (its name as the player sees it, what is there, exits by direction), every object (what it is, " +
                "where it starts, what can be done with it, what it is for), every puzzle and its exact working commands and why the " +
                "solution works, every condition, timer, random event, death and failure message worth knowing, anything a companion " +
                "character does here, and any text the player reads (if written in corrupted phonetic English, give the plain-English " +
                "meaning). Say what is NOT useful too. Describe the game, never the code: no class names, no C#. Be complete rather than brief.",
                $"PART OF THE GAME: {a.Name}\n\nSOURCE:\n{a.Text}");
            TestContext.Out.WriteLine($"Notes: {a.Name} ({a.Text.Length:N0} chars of source -> {text.Length:N0} chars)");
            return $"##### STUDY NOTES: {a.Name}\n{text}";
        }));

        var invisiclues = await File.ReadAllTextAsync(Path.Combine(root, "Docs", "hints", "planetfall", "invisiclues.md"));
        var lore = await File.ReadAllTextAsync(Path.Combine(root, "Docs", "hints", "planetfall", "05-lore.md"));
        var sources =
            $"##### A COMPLETE, VERIFIED PLAYTHROUGH (every command, and what the game replied)\n{playthrough}\n\n" +
            $"{string.Join("\n\n", notes)}\n\n##### THE OFFICIAL HINT BOOK (InvisiClues)\n{invisiclues}\n\n##### LORE NOTES\n{lore}";
        TestContext.Out.WriteLine($"Sources for the bible: {sources.Length:N0} chars.");

        // 3. Write the bible, a section at a time, each from all the sources.
        var written = await Task.WhenAll(Sections.Select(async s =>
        {
            var text = await Ask(
                "You have finished the Infocom text adventure Planetfall many times and understand it completely. From the SOURCES " +
                "(a verified playthrough, study notes on how every part of the game works, the official hint book, lore notes) write one " +
                "section of a GAME BIBLE: the briefing you would give a friend who is about to sit beside new players and help them — " +
                "someone who must understand the game, not look things up. Write flowing, precise prose organised under short plain " +
                "sub-headings. Explain WHY things work and how players go wrong, not just what to type; but always include the exact " +
                "working commands (from the playthrough) and the exact room and object names the player sees. Where the sources disagree, " +
                "the playthrough is the truth. State only what the sources support. Be complete: if a player could ask about it, it belongs here.",
                $"SECTION TO WRITE: {s.Title}\nIT MUST COVER: {s.Ask}\n\nSOURCES:\n{sources}");
            TestContext.Out.WriteLine($"Section: {s.Title} ({text.Length:N0} chars)");
            return $"# {s.Title}\n\n{text.Trim()}";
        }));

        var bible = string.Join("\n\n\n", written) + "\n";
        await File.WriteAllTextAsync(Path.Combine(outDir, "planetfall-bible.md"), bible, new UTF8Encoding(false));
        TestContext.Out.WriteLine($"Wrote the bible: {bible.Length:N0} chars (~{bible.Length / 4:N0} tokens).");
    }

    /// <summary>
    ///     The baseline the oracle's "what has changed in the world" is measured against: every object's state as
    ///     first seen. The engine registers most objects only when first touched, so the state at turn zero is
    ///     nearly empty; a full playthrough touches everything, and an object's first appearance is its default.
    ///     No model calls.
    /// </summary>
    [Test]
    public async Task WriteBaseline()
    {
        StartOver();
        var baseline = new SortedDictionary<string, string>(StringComparer.Ordinal);
        void Absorb()
        {
            foreach (var (key, value) in StateFlattener.Flatten(Context)) baseline.TryAdd(key, value);
        }

        Absorb();
        foreach (var step in WalkthroughSource.Load("WalkthroughTestOne.cs"))
        {
            await DoWithSetup(step.Command, step.Setup);
            Absorb();
        }

        var outDir = Path.Combine(WalkthroughSource.RepoRoot(), "Planetfall", "Hints", "Oracle");
        Directory.CreateDirectory(outDir);
        await File.WriteAllTextAsync(Path.Combine(outDir, "planetfall-initial-state.json"),
            JsonConvert.SerializeObject(baseline, Formatting.Indented), new UTF8Encoding(false));
        TestContext.Out.WriteLine($"Baseline: {baseline.Count} properties.");
    }

    // ---- inputs ---------------------------------------------------------------------------------

    private async Task<string> Playthrough()
    {
        var sb = new StringBuilder();
        foreach (var step in WalkthroughSource.Load("WalkthroughTestOne.cs"))
        {
            await DoWithSetup(step.Command, step.Setup);
            if (step.Command.Equals("score", StringComparison.OrdinalIgnoreCase)) continue;
            var response = LastResponse.Replace("\r", "").Trim();
            sb.AppendLine($"[{Context.CurrentLocation.Name}] > {step.Command}");
            sb.AppendLine(response.Length <= 900 ? response : response[..900] + " …");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private sealed record Area(string Name, string Text);

    private static List<Area> SourceAreas(string root)
    {
        var game = Path.Combine(root, "Planetfall");
        string Read(IEnumerable<string> files) => string.Join("\n\n",
            files.Where(f => !f.EndsWith("Prompts.cs", StringComparison.Ordinal)) // LLM persona text, not game knowledge
                .OrderBy(f => f, StringComparer.Ordinal)
                .Select(f => $"// ===== {Path.GetRelativePath(game, f)} =====\n{File.ReadAllText(f)}"));
        IEnumerable<string> Dir(params string[] parts) =>
            Directory.EnumerateFiles(Path.Combine(new[] { game }.Concat(parts).ToArray()), "*.cs", SearchOption.AllDirectories);

        var kalamonteeItems = Dir("Item", "Kalamontee").OrderBy(f => f, StringComparer.Ordinal).ToList();
        var half = kalamonteeItems.Count / 2;
        var core = Directory.EnumerateFiles(game, "*.cs", SearchOption.TopDirectoryOnly)
            .Concat(Directory.EnumerateFiles(Path.Combine(game, "Item"), "*.cs", SearchOption.TopDirectoryOnly))
            .Concat(Directory.EnumerateFiles(Path.Combine(game, "Location"), "*.cs", SearchOption.TopDirectoryOnly))
            .Concat(Dir("Command"))
            .Concat(Directory.EnumerateFiles(Path.Combine(root, "StellarPatrol"), "*.cs", SearchOption.AllDirectories)
                .Where(f => !f.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar) &&
                            !f.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar)));

        return new List<Area>
        {
            new("The Feinstein and the escape pod (rooms and objects)", Read(Dir("Location", "Feinstein").Concat(Dir("Item", "Feinstein")))),
            new("Kalamontee: the rooms", Read(Dir("Location", "Kalamontee"))),
            new("Kalamontee: the objects, part one", Read(kalamonteeItems.Take(half))),
            new("Kalamontee: the objects, part two (includes Floyd)", Read(kalamonteeItems.Skip(half))),
            new("The shuttle, and Lawanda: the rooms", Read(Dir("Location", "Shuttle").Concat(Dir("Location", "Lawanda")))),
            new("Lawanda: the objects; the computer interior (rooms and objects)", Read(Dir("Item", "Lawanda").Concat(Dir("Item", "Computer")).Concat(Dir("Location", "Computer")))),
            new("The rules of the world: time, hunger, sleep, sickness, dreams, carrying, special commands", Read(core))
        };
    }

    private static Task<string> Ask(string system, string user) => OpenAiDirect.Ask(Model, system, user);
}
