using GameEngine;
using Model.AIParsing;
using Model.Intent;
using Model.Interface;
using Newtonsoft.Json;

namespace UnitTests.Parser.Census;

/// <summary>
///     One thing a player can do that changes the game: where they are standing, what they may be
///     holding, and every verb and every noun that does it. The verbs and nouns are a full product -
///     each verb with each noun has this effect.
/// </summary>
/// <param name="Reply">What the game says. Documentation only; nothing compares against it.</param>
public sealed record Interaction(string Room, string? Holding, string[] Verbs, string[] Nouns, string Reply)
{
    [JsonIgnore] public CensusContext Context => new(Room, Holding);

    public override string ToString()
    {
        return $"{Context}: {Verbs[0]} {Nouns[0]}" +
               (Verbs.Length * Nouns.Length > 1 ? $" (+{Verbs.Length * Nouns.Length - 1} wordings)" : "");
    }

    /// <summary>
    ///     The wordings to try: every verb with the first noun, and the first verb with every other
    ///     noun. That is each word once, not every pairing - the pairings add nothing a parser can get
    ///     wrong that the words alone do not.
    /// </summary>
    public IEnumerable<(string Verb, string Noun)> Wordings()
    {
        foreach (var verb in Verbs)
            yield return (verb, Nouns[0]);

        foreach (var noun in Nouns.Skip(1))
            yield return (Verbs[0], noun);
    }
}

/// <summary>
///     Every interaction in a game that has an in-game effect, found by asking the game: in each room,
///     and with each item in hand, each verb the game knows is aimed at each noun in reach, in a fresh
///     game, and the combination is kept when the game is in a different state afterwards than it is
///     after a turn in which nothing happened.
/// </summary>
public static class InteractionSet
{
    /// <param name="part">
    ///     Which share of the rooms and items to cover, when the work is divided between processes: part
    ///     <c>Index</c> (from zero) of <c>Of</c>. Every part's result goes through <see cref="Tidy" />
    ///     together afterwards. Null covers everything.
    /// </param>
    public static async Task<List<Interaction>> Enumerate(InteractionCensus census, Action<string>? progress = null,
        (int Index, int Of)? part = null)
    {
        // (where, what the game is like afterwards, the verbs that do it) -> the nouns it works on
        var found = new Dictionary<(CensusContext Where, string State, string Verbs), (List<string> Nouns, string Reply)>();
        var plan = census.Survey();

        for (var index = 0; index < plan.Count; index++)
        {
            if (part is { } share && index % share.Of != share.Index)
                continue;

            var (context, nouns) = plan[index];

            foreach (var noun in nouns)
            {
                var nothingHappened = await census.StateAfterDoingNothing(context, noun);
                string? nothingHappenedAnywhere = null;
                var effects = new Dictionary<string, (List<string> Verbs, string Reply)>();

                foreach (var verb in census.Verbs)
                {
                    // The quick probe first. Most verbs aimed at most nouns are left to the narrator, and
                    // most of the rest leave the game exactly as a do-nothing turn does; neither needs a
                    // second look.
                    var quick = await census.Probe(context, verb, noun);
                    if (!quick.Answered || !quick.ReachedTheParser || quick.State == nothingHappened)
                        continue;

                    // Something is different. Whether it is the game, or only what the turn happened to
                    // bring into memory, takes the whole world to say.
                    nothingHappenedAnywhere ??= await census.StateAfterDoingNothing(context, noun, true);

                    var probe = await census.ProbeEffect(context, verb, noun);
                    if (!probe.Answered || probe.State == nothingHappenedAnywhere)
                        continue;

                    if (!effects.TryGetValue(probe.State, out var effect))
                        effects[probe.State] = effect = ([], probe.Response);

                    effect.Verbs.Add(verb);
                }

                foreach (var (state, (verbs, reply)) in effects)
                {
                    var key = (context, state, string.Join('|', verbs));

                    if (!found.TryGetValue(key, out var entry))
                        found[key] = entry = ([], reply);

                    entry.Nouns.Add(noun);
                }
            }

            progress?.Invoke($"{index + 1}/{plan.Count} {context}");
        }

        return Tidy(found.Select(entry => new Interaction(entry.Key.Where.Location, entry.Key.Where.Holding,
            entry.Key.Verbs.Split('|'), entry.Value.Nouns.ToArray(),
            entry.Value.Reply.Length > 160 ? entry.Value.Reply[..160] + "..." : entry.Value.Reply)));
    }

    /// <summary>
    ///     The interactions in the order the file keeps them, each kept once. An item answers the same
    ///     way lying in its room and in the player's hands, mostly. Where it does, the room is enough;
    ///     holding it is kept only for what can only be done while holding it.
    /// </summary>
    public static List<Interaction> Tidy(IEnumerable<Interaction> interactions)
    {
        var once = new HashSet<string>();

        return interactions
            .OrderBy(interaction => interaction.Holding is not null)
            .ThenBy(interaction => interaction.Room).ThenBy(interaction => interaction.Holding)
            .ThenBy(interaction => interaction.Nouns[0]).ThenBy(interaction => string.Join('|', interaction.Verbs))
            .Where(interaction => once.Add($"{string.Join('|', interaction.Nouns)}#{string.Join('|', interaction.Verbs)}"))
            .ToList();
    }

    // ------------------------------------------------------------------ generating the file

    /// <summary>
    ///     Set to "1/4", "2/4" ... in as many shells as there are cores, and each run of the generator
    ///     covers that share of the game and leaves its result in a temporary file. The engine keeps the
    ///     game in statics, so one process can only ever play one game at a time; this is how the half
    ///     hour is divided. <see cref="MergeParts" /> then writes the file.
    /// </summary>
    public const string PartVariable = "INTERACTION_SET_PART";

    private static string PartsDirectory => Path.Combine(Path.GetTempPath(), "zorkai-interaction-set");

    /// <summary>What the generating test does: the whole game, or the share <see cref="PartVariable" /> names.</summary>
    public static async Task<string> Regenerate(InteractionCensus census, string testProject, string fileName,
        Action<string> progress)
    {
        var part = PartFromEnvironment();

        if (part is null)
        {
            var everything = await Enumerate(census, progress);
            Save(PathFor(testProject, fileName), everything);
            return Describe(everything);
        }

        Directory.CreateDirectory(PartsDirectory);
        var stem = Path.Combine(PartsDirectory, $"{fileName}.part{part.Value.Index + 1}of{part.Value.Of}");
        File.WriteAllText($"{stem}.progress", string.Empty);

        var share = await Enumerate(census, line =>
        {
            progress(line);
            File.AppendAllText($"{stem}.progress", line + Environment.NewLine);
        }, part);

        Save($"{stem}.json", share);
        return $"Part {part.Value.Index + 1} of {part.Value.Of}: {Describe(share)}. When every part is done, run " +
               "MergeTheInteractionSetParts.";
    }

    /// <summary>Writes the file from the parts, once every part is there.</summary>
    public static string MergeParts(string testProject, string fileName)
    {
        var parts = Directory.Exists(PartsDirectory)
            ? Directory.GetFiles(PartsDirectory, $"{fileName}.part*of*.json").Order().ToArray()
            : [];

        var expected = parts.Select(path => int.Parse(path[(path.LastIndexOf("of", StringComparison.Ordinal) + 2)..^5]))
            .Distinct().ToArray();

        if (expected.Length != 1 || parts.Length != expected[0])
            throw new InvalidOperationException(
                $"Expected every part of one run in {PartsDirectory}, and found {parts.Length}: " +
                string.Join(", ", parts.Select(Path.GetFileName)));

        var everything = Tidy(parts.SelectMany(path =>
            JsonConvert.DeserializeObject<List<Interaction>>(File.ReadAllText(path)) ?? []));

        Save(PathFor(testProject, fileName), everything);

        foreach (var leftover in Directory.GetFiles(PartsDirectory, $"{fileName}.part*"))
            File.Delete(leftover);

        return Describe(everything);
    }

    private static (int Index, int Of)? PartFromEnvironment()
    {
        var value = Environment.GetEnvironmentVariable(PartVariable);
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var numbers = value.Split('/');
        if (numbers.Length == 2 && int.TryParse(numbers[0], out var index) && int.TryParse(numbers[1], out var of) &&
            index >= 1 && index <= of)
            return (index - 1, of);

        throw new InvalidOperationException($"{PartVariable} must look like 2/4, not \"{value}\".");
    }

    private static string Describe(IReadOnlyCollection<Interaction> interactions)
    {
        return $"{interactions.Count} interactions, " +
               $"{interactions.Sum(interaction => interaction.Wordings().Count())} wordings";
    }

    public static void Save(string path, IEnumerable<Interaction> interactions)
    {
        File.WriteAllText(path, JsonConvert.SerializeObject(interactions, Formatting.Indented,
            new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }) + "\n");
    }

    /// <summary>A game's interaction set as it was when its tests were built; empty if never generated.</summary>
    /// <param name="tests">The game's test assembly, which the file is compiled into.</param>
    public static IReadOnlyList<Interaction> Load(System.Reflection.Assembly tests, string fileName)
    {
        return EmbeddedFile.Read(tests, fileName) is { } json
            ? JsonConvert.DeserializeObject<List<Interaction>>(json) ?? []
            : [];
    }

    /// <summary>The source file of a game's interaction set, for writing it.</summary>
    public static string PathFor(string testProject, string fileName)
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Zork.sln")))
            directory = directory.Parent;

        if (directory is null)
            throw new InvalidOperationException("Could not find the repository root above the test directory.");

        return Path.Combine(directory.FullName, testProject, "Parser", fileName);
    }

    /// <summary>
    ///     The check a parser has to pass for one interaction: every wording of it, sent through the
    ///     parser, leaves the game in exactly the state it is in when the engine is handed the verb and
    ///     the noun directly.
    /// </summary>
    /// <param name="parserFor">
    ///     The parser under test for one wording. It is given the verb and noun so that a scripted AI
    ///     can answer as a model would; a live one ignores them.
    /// </param>
    public static async Task<List<string>> Check(InteractionCensus census, Interaction interaction,
        Func<string, string, IIntentParser> parserFor)
    {
        var problems = new List<string>();

        foreach (var (verb, noun) in interaction.Wordings())
        {
            var input = $"{verb} {noun}";
            var nothingHappened = await census.StateAfterDoingNothing(interaction.Context, noun);
            var direct = await census.Probe(interaction.Context, verb, noun);

            if (!direct.Answered || !direct.ReachedTheParser || direct.State == nothingHappened)
            {
                problems.Add($"\"{input}\" no longer has an effect - regenerate the interaction set");
                continue;
            }

            var parsed = await census.Probe(interaction.Context, input, parserFor(verb, noun), noun);
            if (parsed.State == direct.State)
                continue;

            // Different, but perhaps only in what each turn brought into memory: the parser may have
            // reached the same handler by another road. The whole world settles it.
            var directAnywhere = await census.ProbeEffect(interaction.Context, verb, noun);
            var parsedAnywhere = await census.Probe(interaction.Context, input, parserFor(verb, noun), noun, true);

            if (parsedAnywhere.State != directAnywhere.State)
                problems.Add($"\"{input}\" through the parser: " +
                             Describe(parsedAnywhere,
                                 await census.StateAfterDoingNothing(interaction.Context, noun, true)) +
                             $"; expected: {directAnywhere.Response}");
        }

        return problems;
    }

    private static string Describe(ProbeResult parsed, string nothingHappened)
    {
        var reply = string.IsNullOrWhiteSpace(parsed.Response) ? "(no reply)" : parsed.Response;
        return parsed.State == nothingHappened ? $"nothing happened - {reply}" : $"something else happened - {reply}";
    }

    /// <summary>
    ///     The production parser - the grammar first, then the AI - with an AI that reads a wording the
    ///     way a model does: this verb, this noun. What is under test is everything around the model.
    /// </summary>
    public static IIntentParser ProductionParser(CensusGame game, string verb, string noun)
    {
        return new IntentParser(new AiThatReadsItPlainly(verb, noun), game.Commands, game.Name);
    }

    private sealed class AiThatReadsItPlainly(string verb, string noun) : IAIParser
    {
        public string LanguageModel => "scripted";

        public Task<IntentBase> AskTheAIParser(string input, string locationDescription, string sessionId)
        {
            return Task.FromResult<IntentBase>(new SimpleIntent
                { Verb = verb, Noun = noun, OriginalInput = input, Message = input });
        }
    }
}
