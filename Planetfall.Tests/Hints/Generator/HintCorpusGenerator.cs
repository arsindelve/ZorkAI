using System.Text;
using System.Text.RegularExpressions;
using GameEngine.Hints.Data;
using Planetfall.Tests.Walkthrough;

namespace Planetfall.Tests.Hints.Generator;

/// <summary>
///     Generates the Planetfall hint corpus from the game itself. Replays the verified walkthroughs through
///     the real engine, cuts them into puzzles, observes each puzzle's completion signals in the live state,
///     works out which puzzles are optional, extracts the words players use for each, and has the model
///     write the three rungs from the exact verified commands. Writes Planetfall/Hints/Generated/planetfall-hints.json
///     for review and check-in. [Explicit]: it plays the game (fast) and calls OpenAI for the rungs.
///     Set HINT_GEN_DRY=1 to skip the model (placeholder rungs) and just inspect the segmentation.
/// </summary>
[TestFixture]
[Explicit("Generates the hint corpus — plays the walkthroughs and calls OpenAI (OPEN_AI_KEY)")]
public class HintCorpusGenerator : WalkthroughTestBase
{
    private const string FullWalkthrough = "WalkthroughTestOne.cs";
    private const string MinimalWalkthrough = "WalkthroughDontFixAnything.cs";

    [Test]
    public async Task Generate()
    {
        var dry = Environment.GetEnvironmentVariable("HINT_GEN_DRY") == "1";
        var full = await Replay(WalkthroughSource.Load(FullWalkthrough));
        var minimal = await Replay(WalkthroughSource.Load(MinimalWalkthrough));

        var segments = Segmenter.Segment(full);
        var minimalSegments = Segmenter.Segment(minimal);
        TestContext.Out.WriteLine($"{segments.Count} segments in the full walkthrough, {minimalSegments.Count} in the minimal one.");

        var data = new HintData { Game = "Planetfall", GeneratedFrom = { FullWalkthrough, MinimalWalkthrough } };
        var previousMandatory = (string?)null;
        var ids = new HashSet<string>(StringComparer.Ordinal);
        var minimalCursor = 0; // segments align in order: each minimal puzzle accounts for one full one

        for (var i = 0; i < segments.Count; i++)
        {
            var segment = segments[i];
            var next = i + 1 < segments.Count ? segments[i + 1] : null;

            // The same transit again (an elevator ride, a teleport) is not a new puzzle — but the same command
            // again that leaves a new mark on the world (the second pour that fixes the comms) is.
            var done = PredicateFinder.Find(full, segment, next, optional: false);
            var transit = done.All(p => p.Path.EndsWith(".VisitCount", StringComparison.Ordinal) || !PredicateFinder.HoldsToEnd(full, segment, p));
            if (transit && segments.Take(i).Any(earlier => earlier.SamePuzzleAs(segment))) continue;
            if (done.Count == 0)
            {
                // Nothing observable marks this one (a ride back to a known room): a hint could never tell
                // it was solved, so it cannot be a blocker. The fallback solver still covers the question.
                TestContext.Out.WriteLine($"(dropped: no observable completion) {segment.Location}: {string.Join(" | ", segment.Commands)}");
                continue;
            }

            var match = minimalSegments.FindIndex(minimalCursor, m => m.SamePuzzleAs(segment));
            var optional = match < 0;
            if (!optional) minimalCursor = match + 1;
            else done = PredicateFinder.Find(full, segment, next, optional: true);
            if (done.Count == 0) continue; // optional and unobservable: nothing to hint towards
            var later = segments.Skip(i + 1).SelectMany(s => Aliases(s.Commands, s.Location)).Distinct().ToList();

            // A long trek to somewhere new is a puzzle of its own for a lost player: a navigation node.
            // (Only the first arrival: a return trip to a known room is done the moment it was first reached.)
            var firstArrival = !full.Before(segment.ApproachStart).TryGetValue("Location:" + segment.LocationType + ".VisitCount", out var visits) || visits == "0";
            if (firstArrival && segment.Approach.Count(IsMove) > 4)
            {
                var routeWords = RoomWords(segment.ApproachRooms.Append(segment.Location));
                var written = dry ? null : await RungWriter.Write(segment, later.Except(routeWords).Take(60).ToList(), navigation: true, exit: null);
                var nav = new HintNodeData
                {
                    Id = Unique(ids, "REACH_" + Symbol(segment.Location)),
                    Title = written?.Title ?? "Find your way to the " + segment.Location,
                    Location = segment.ApproachFrom,
                    Optional = optional,
                    Prerequisites = previousMandatory is null ? new() : new() { previousMandatory },
                    Aliases = routeWords,
                    Done = { new StatePredicate { Path = "Location:" + segment.LocationType + ".VisitCount", Op = "gte", Value = "1" } },
                    Rungs = written?.Rungs.ToList() ?? new List<string>
                    {
                        "Where you need to be next is some way from here. Keep exploring.",
                        $"Make your way to the {segment.Location}.",
                        $"From the {segment.ApproachFrom}: {Compress(segment.Approach)}."
                    },
                    Approach = segment.Approach.ToList()
                };
                data.Nodes.Add(nav);
                if (!optional) previousMandatory = nav.Id;
                Report(nav);
            }

            var node = new HintNodeData
            {
                Id = Unique(ids, Symbol(segment.Location) + "_" + Symbol(FirstNoun(segment.Commands) ?? segment.Commands[0])),
                Location = segment.Location,
                Optional = optional,
                Prerequisites = previousMandatory is null ? new() : new() { previousMandatory },
                Aliases = Aliases(segment.Commands, segment.Location).Concat(RoomWords(segment.ApproachRooms)).Distinct().ToList(),
                Done = done,
                Commands = segment.Commands.ToList(),
                Approach = segment.Approach.ToList()
            };

            // A short walk to the next puzzle belongs at the end of this one's solution ("...then go port").
            var exit = next is not null && next.Location != segment.Location && next.Approach.Count is > 0 and <= 4 && next.Approach.All(IsMove)
                ? string.Join(", ", next.Approach) : null;
            var ladder = dry ? null : await RungWriter.Write(segment, later.Except(node.Aliases).Take(60).ToList(), navigation: false, exit);
            node.Title = ladder?.Title ?? $"{segment.Location}: {segment.Commands[0]}";
            node.Rungs = ladder?.Rungs.ToList() ?? new List<string> { $"Something to do in the {segment.Location}.", $"Try: {segment.Commands[0]}.", $"{segment.Location}: {Compress(segment.Commands)}" };

            data.Nodes.Add(node);
            if (!optional) previousMandatory = node.Id;
            Report(node);
        }

        var outPath = Path.Combine(WalkthroughSource.RepoRoot(), "Planetfall", "Hints", "Generated", "planetfall-hints.json");
        Directory.CreateDirectory(Path.GetDirectoryName(outPath)!);
        await File.WriteAllTextAsync(outPath, data.ToJson(), new UTF8Encoding(false));
        TestContext.Out.WriteLine($"Wrote {data.Nodes.Count} nodes to {outPath}");
    }

    private static void Report(HintNodeData node)
    {
        TestContext.Out.WriteLine($"{node.Id,-34} {(node.Optional ? "optional" : "        ")} {node.Title}");
        TestContext.Out.WriteLine($"    commands: {string.Join(" | ", node.Commands)}");
        TestContext.Out.WriteLine($"    done: {string.Join(" AND ", node.Done)}");
    }

    // ---- replay ---------------------------------------------------------------------------------

    private async Task<Trace> Replay(List<WalkthroughStep> steps)
    {
        StartOver();
        var records = new List<StepRecord>();
        var initial = StateFlattener.Flatten(Context);

        for (var i = 0; i < steps.Count; i++)
        {
            var before = Context.CurrentLocation.Name;
            await DoWithSetup(steps[i].Command, steps[i].Setup);
            records.Add(new StepRecord(i, steps[i].Command, steps[i].Setup, LastResponse, before,
                Context.CurrentLocation.Name, Context.CurrentLocation.GetType().Name, Context.Score, StateFlattener.Flatten(Context)));
        }

        // Nouns are read at the end: the engine registers some objects only when first touched.
        return new Trace(initial, records, StateFlattener.Nouns());
    }

    // ---- helpers --------------------------------------------------------------------------------

    private static readonly HashSet<string> Moves = new(StringComparer.OrdinalIgnoreCase)
        { "n", "s", "e", "w", "ne", "nw", "se", "sw", "u", "d", "up", "down", "in", "out", "north", "south", "east", "west", "port", "starboard" };

    internal static bool IsMove(string command) => Moves.Contains(command.Trim());

    private static readonly HashSet<string> Stopwords = new(StringComparer.OrdinalIgnoreCase)
    {
        "the", "a", "an", "on", "in", "with", "to", "into", "through", "off", "under", "from", "of", "at", "all", "it",
        "and", "then", "wait", "look", "score", "examine", "take", "drop", "open", "close", "press", "push", "pull",
        "put", "slide", "type", "key", "activate", "deactivate", "read", "remove", "set", "shoot", "throw", "wear",
        "sit", "extend", "place", "unlock", "pour", "go", "floyd", "north", "south", "east", "west", "up", "down"
    };

    internal static List<string> Aliases(IEnumerable<string> commands, string location)
    {
        var words = new List<string>();
        foreach (var command in commands)
        {
            if (IsMove(command)) continue;
            var tokens = Regex.Split(command.ToLowerInvariant(), "[^a-z0-9]+").Where(t => t.Length > 0).ToList();
            // the verb is the first word; the nouns are what players will say
            foreach (var t in tokens.Skip(1).Where(t => !Stopwords.Contains(t) && (t.Length >= 3 || char.IsDigit(t[0]))))
                words.Add(t);
        }
        words.AddRange(location.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries).Where(w => w.Length >= 3 && !Stopwords.Contains(w)));
        return words.Distinct().ToList();
    }

    /// <summary>The words of room names ("Admin Corridor South" -> admin, corridor, south), for the router.</summary>
    internal static List<string> RoomWords(IEnumerable<string> rooms) =>
        rooms.SelectMany(r => r.ToLowerInvariant().Split(new[] { ' ', '-' }, StringSplitOptions.RemoveEmptyEntries))
            .Where(w => w.Length >= 3 && !Stopwords.Contains(w)).Distinct().ToList();

    private static string? FirstNoun(IReadOnlyList<string> commands)
    {
        foreach (var command in commands)
        {
            var tokens = Regex.Split(command.ToLowerInvariant(), "[^a-z0-9]+").Where(t => t.Length > 0).ToList();
            var noun = tokens.Skip(1).FirstOrDefault(t => !Stopwords.Contains(t) && t.Length >= 3);
            if (noun is not null) return noun;
        }

        return null;
    }

    /// <summary>"wait, wait, wait, n" as "wait ×3, n".</summary>
    internal static string Compress(IReadOnlyList<string> commands)
    {
        var parts = new List<string>();
        for (var i = 0; i < commands.Count;)
        {
            var j = i;
            while (j < commands.Count && commands[j].Equals(commands[i], StringComparison.OrdinalIgnoreCase)) j++;
            parts.Add(j - i > 1 ? $"{commands[i]} ×{j - i}" : commands[i]);
            i = j;
        }

        return string.Join(", ", parts);
    }

    private static string Symbol(string text) =>
        Regex.Replace(text.ToUpperInvariant(), "[^A-Z0-9]+", "_").Trim('_');

    private static string Unique(HashSet<string> ids, string id)
    {
        var candidate = id;
        for (var n = 2; !ids.Add(candidate); n++) candidate = id + "_" + n;
        return candidate;
    }
}

/// <summary>One replayed walkthrough step and the whole world after it.</summary>
public sealed record StepRecord(
    int Index, string Command, string? Setup, string Response, string LocationBefore, string LocationAfter,
    string LocationType, int ScoreAfter, IReadOnlyDictionary<string, string> After);

public sealed record Trace(IReadOnlyDictionary<string, string> Initial, IReadOnlyList<StepRecord> Steps,
    IReadOnlyDictionary<string, string[]> Nouns)
{
    private readonly Dictionary<string, string?> _firstSeen = new(StringComparer.Ordinal);

    public IReadOnlyDictionary<string, string> Before(int index) => index <= 0 ? Initial : Steps[index - 1].After;

    /// <summary>
    ///     A key's value when the game started — or, for an object the engine registers only when first touched,
    ///     its value when it first appeared: its default either way.
    /// </summary>
    public string? FirstSeen(string key)
    {
        if (_firstSeen.TryGetValue(key, out var cached)) return cached;
        var value = Initial.TryGetValue(key, out var initial) ? initial
            : Steps.Select(s => s.After.GetValueOrDefault(key)).FirstOrDefault(v => v is not null);
        _firstSeen[key] = value;
        return value;
    }
}

/// <summary>A puzzle: the substantive commands typed in one room, and the moves that led there.</summary>
public sealed class Segment
{
    public string Location { get; init; } = "";
    public string LocationType { get; init; } = "";
    public string ApproachFrom { get; init; } = "";
    public int ApproachStart { get; init; }
    public List<string> Approach { get; } = new();
    public List<string> ApproachRooms { get; } = new();
    public List<(string Command, string Response)> ApproachExchanges { get; } = new();
    public List<string> Commands { get; } = new();
    public List<(string Command, string Response)> Exchanges { get; } = new();
    public int FirstStep { get; set; }
    public int LastStep { get; set; }

    private readonly List<StepRecord> _idleTail = new();
    private int _lastMark = -1;

    public bool HasMark => _lastMark >= 0;
    public int IdleTail => _idleTail.Count;

    /// <summary>A step that leaves a mark on the world.</summary>
    public void AddMark(StepRecord step)
    {
        Add(step);
        _idleTail.Clear();
        _lastMark = step.Index;
    }

    /// <summary>A wait, a look, a failed try: part of the puzzle, but not what solved it.</summary>
    public void AddIdle(StepRecord step)
    {
        Add(step);
        _idleTail.Add(step);
    }

    private void Add(StepRecord step)
    {
        if (Commands.Count == 0) FirstStep = step.Index;
        Commands.Add(step.Command);
        Exchanges.Add((step.Command, step.Response));
        LastStep = step.Index;
    }

    private static readonly HashSet<string> Idle = new(StringComparer.OrdinalIgnoreCase) { "wait", "z", "look", "score" };

    /// <summary>The commands that do something (waiting and looking aside).</summary>
    public IEnumerable<string> Actions => Commands.Where(c => !Idle.Contains(c)).Select(c => c.ToLowerInvariant());

    /// <summary>Same room, same actions: the same puzzle (in another walkthrough, or again in this one).</summary>
    public bool SamePuzzleAs(Segment other) =>
        LocationType == other.LocationType && Actions.SequenceEqual(other.Actions);
}

/// <summary>
///     Cuts a replayed walkthrough into puzzles. A step is a puzzle step when it is not a move and it leaves a
///     mark on the world (some object's state is no longer what it was at the start of the game). Puzzle steps
///     in one room form a segment, together with the room's other commands around them (waits, looks, failed
///     tries); moves and everything typed in rooms that left no mark are the next segment's approach.
/// </summary>
public static class Segmenter
{
    public static List<Segment> Segment(Trace trace)
    {
        var segments = new List<Segment>();
        Segment? open = null;
        var approach = new List<StepRecord>();
        var pending = new List<StepRecord>(); // same-room commands before the room's first marking step
        string? pendingRoom = null;
        var approachFrom = trace.Steps.Count > 0 ? trace.Steps[0].LocationBefore : "";

        void Close()
        {
            if (open is null) return;
            segments.Add(open);
            approachFrom = open.Location;
            open = null;
        }

        void FlushPending()
        {
            approach.AddRange(pending);
            pending.Clear();
            pendingRoom = null;
        }

        foreach (var step in trace.Steps)
        {
            if (step.Command.Equals("score", StringComparison.OrdinalIgnoreCase)) continue;

            if (open is not null && step.LocationAfter != open.Location) Close();

            var move = HintCorpusGenerator.IsMove(step.Command);
            if (move || !PredicateFinder.LeavesAMark(trace, step.Index))
            {
                if (open is not null)
                {
                    open.AddIdle(step);
                    continue;
                }

                if (move || pendingRoom != step.LocationAfter) FlushPending();
                if (move) approach.Add(step);
                else
                {
                    pendingRoom = step.LocationAfter;
                    pending.Add(step);
                }

                continue;
            }

            if (open is null)
            {
                if (pendingRoom != step.LocationAfter) FlushPending();
                open = new Segment
                {
                    Location = step.LocationAfter, LocationType = step.LocationType, ApproachFrom = approachFrom,
                    ApproachStart = approach.Count > 0 ? approach[0].Index : pending.Count > 0 ? pending[0].Index : step.Index
                };
                foreach (var a in approach)
                {
                    open.Approach.Add(a.Command);
                    open.ApproachExchanges.Add((a.Command, a.Response));
                    if (!open.ApproachRooms.Contains(a.LocationAfter)) open.ApproachRooms.Add(a.LocationAfter);
                }

                approach.Clear();
                foreach (var p in pending) open.AddIdle(p);
                pending.Clear();
                pendingRoom = null;
            }

            // A mark that ends a long wait (the pod landing, the shuttle reaching the station) closes the puzzle:
            // whatever the player does next in the same room is a puzzle of its own.
            if (open.IdleTail >= 3)
            {
                open.AddMark(step);
                Close();
                continue;
            }

            open.AddMark(step);
        }

        Close();
        return segments;
    }

    public static IEnumerable<string> Diff(IReadOnlyDictionary<string, string> before, IReadOnlyDictionary<string, string> after)
    {
        foreach (var key in before.Keys.Union(after.Keys))
            if (!before.TryGetValue(key, out var b) || !after.TryGetValue(key, out var a) || a != b)
                yield return key;
    }
}

/// <summary>
///     A puzzle's completion signals: the state that changed while it was being solved and was still that
///     way when the next puzzle began (the graph back-fills earlier puzzles from later ones, so a signal need
///     not last the whole game — a flask is emptied, a shuttle stops). Prefers signals on the objects the
///     puzzle's own commands name; falls back to any signal; and as a last resort to "reached the next room".
/// </summary>
public static class PredicateFinder
{
    // Clocks, counters and plumbing: they change constantly and mean nothing to a stuck player.
    private static readonly Regex Noise = new(
        "(Moves|Time|Countdown|Counter|Remaining|Timer|LastRolled|Speed|Warmth|Just|ThisTurn|Chooser|Generation|Position|Turns" +
        "|Callback|Description|LastLocation|PreviousLocation|TurnFlags|ScriptedSequence|InLobby|InteractionHasHappened|ItemBeingHeld)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>
    ///     Does this one step change some object's state away from what it was at the start of the game?
    ///     Objects moving (the player's inventory, a wandering robot) do not count on their own: a puzzle
    ///     that only moves things is caught by the flag the engine sets when it matters (HasEverBeenPickedUp).
    /// </summary>
    public static bool LeavesAMark(Trace trace, int step) =>
        Candidates(trace, step, step, step).Any(c =>
            !c.Predicate.Path.EndsWith(".VisitCount", StringComparison.Ordinal) &&
            !c.Predicate.Path.EndsWith(".CurrentLocation", StringComparison.Ordinal));

    /// <summary>
    ///     Every change between the two steps inclusive that still holds at <paramref name="untilStep" />:
    ///     a value that changed away from its initial one, a count that rose and never fell, a list that gained
    ///     an element. Objects the engine registers lazily arrive with their defaults; those count only when
    ///     the default is already telling (a flag set, a count above zero).
    /// </summary>
    public static List<(StatePredicate Predicate, string Object)> Candidates(Trace trace, int firstStep, int lastStep, int untilStep)
    {
        var before = trace.Before(firstStep);
        var after = trace.Steps[lastStep].After;
        var candidates = new List<(StatePredicate Predicate, string Object)>();

        foreach (var key in Segmenter.Diff(before, after))
        {
            var dot = key.LastIndexOf('.');
            if (dot < 0) continue;
            var obj = key[..dot];
            var prop = key[(dot + 1)..];
            if (Noise.IsMatch(prop)) continue;

            var existed = before.TryGetValue(key, out var b);
            if (!after.TryGetValue(key, out var a)) continue;

            if (a.StartsWith("list:", StringComparison.Ordinal))
            {
                var wasIn = (b ?? "list:")[5..].Split('|', StringSplitOptions.RemoveEmptyEntries).ToHashSet();
                foreach (var added in a[5..].Split('|', StringSplitOptions.RemoveEmptyEntries).Where(x => !wasIn.Contains(x)))
                {
                    var p = new StatePredicate { Path = key, Op = "contains", Value = added };
                    if (HoldsThrough(trace, lastStep, untilStep, p)) candidates.Add((p, obj));
                }

                continue;
            }

            if (int.TryParse(a, out var ai))
            {
                if (!existed || !int.TryParse(b, out var bi)) continue; // a new object's counts are its defaults
                var p = new StatePredicate { Path = key, Op = "gte", Value = a };
                if (ai > bi && HoldsThrough(trace, lastStep, untilStep, p)) candidates.Add((p, obj));
                continue;
            }

            if (!existed && a != "True") continue; // a new object's defaults are not an achievement
            if (a == trace.FirstSeen(key)) continue; // back to the default is not an achievement either
            var eq = new StatePredicate { Path = key, Op = "eq", Value = a };
            if (HoldsThrough(trace, lastStep, untilStep, eq)) candidates.Add((eq, obj));
        }

        // An object that has a flag for "done" (HasEverBeenPickedUp, IsAcrossRift...) needs no location predicate,
        // which would come undone the moment the player puts the thing down.
        var flagged = candidates.Where(c => c.Predicate.Op == "eq" && c.Predicate.Value == "True").Select(c => c.Object).ToHashSet();
        candidates.RemoveAll(c => c.Predicate.Path.EndsWith(".CurrentLocation", StringComparison.Ordinal) && flagged.Contains(c.Object));
        return candidates;
    }

    /// <param name="optional">An optional puzzle has no dependent to back-fill it, so its signals must last.</param>
    public static List<StatePredicate> Find(Trace trace, Segment segment, Segment? next, bool optional)
    {
        // A signal must hold until the moment the next puzzle is solved (its last step), when back-fill takes over.
        var until = next is null || optional ? trace.Steps.Count - 1 : Math.Max(segment.LastStep, next.LastStep - 1);
        var candidates = Candidates(trace, segment.FirstStep, segment.LastStep, until);

        var words = HintCorpusGenerator.Aliases(segment.Commands, segment.Location);
        var strong = candidates.Where(c => trace.Nouns.TryGetValue(c.Object, out var ns) &&
                                           ns.SelectMany(n => n.ToLowerInvariant().Split(' ')).Any(words.Contains)).ToList();
        if (strong.Count > 0) return strong.Select(c => c.Predicate).ToList();

        var meaningful = candidates.Where(c => !c.Predicate.Path.EndsWith(".VisitCount", StringComparison.Ordinal)).ToList();
        if (meaningful.Count > 0) return meaningful.Select(c => c.Predicate).ToList();

        // No lasting mark at all (a ride, a teleport): done once the player has come out the other side — in a
        // room they had never been in before, and whose visit the engine actually counts (both checked against
        // the trace: a room already visited would make the puzzle "done" from the start, and back-fill would
        // then wrongly close everything before it). Nothing qualifies: no predicate, and the caller drops the node.
        var before = trace.Before(segment.FirstStep);
        var horizon = next?.FirstStep ?? trace.Steps.Count; // the walk to the next puzzle, no further
        foreach (var step in trace.Steps.Skip(segment.LastStep + 1).TakeWhile(s => s.Index < horizon))
        {
            if (step.LocationType == segment.LocationType) continue;
            var key = "Location:" + step.LocationType + ".VisitCount";
            var wasThere = before.TryGetValue(key, out var v) && v != "0";
            var counted = step.After.TryGetValue(key, out var after) && after != "0";
            if (!wasThere && counted)
                return new List<StatePredicate> { new() { Path = key, Op = "gte", Value = "1" } };
        }

        return new List<StatePredicate>();
    }

    /// <summary>A lasting mark, as opposed to the transient state of a ride or a teleport.</summary>
    public static bool HoldsToEnd(Trace trace, Segment segment, StatePredicate predicate) =>
        HoldsThrough(trace, segment.LastStep, trace.Steps.Count - 1, predicate);

    private static bool HoldsThrough(Trace trace, int lastStep, int untilStep, StatePredicate predicate)
    {
        for (var i = lastStep + 1; i <= untilStep && i < trace.Steps.Count; i++)
            if (!predicate.Holds(trace.Steps[i].After))
                return false;
        return true;
    }
}
