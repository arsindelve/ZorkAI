using System.Reflection;
using System.Text.RegularExpressions;
using GameEngine;
using Model.Intent;
using Model.Interface;
using Model.Movement;
using Newtonsoft.Json;
using UnitTests.Parser.Census;

namespace UnitTests.Parser.Walkthroughs;

/// <summary>
///     One way of reading a sentence that the game answers. The verbs and the nouns are alternatives:
///     any of the verbs with any of the nouns is this reading, and each one listed was tried against the
///     game and left it in the same state.
/// </summary>
/// <param name="Kind">act, take, drop, board, disembark, goto or move - the parser's intent categories.</param>
public sealed record Reading(string Kind, string[] Verbs, string[] Nouns, string? Preposition = null,
    string[]? SecondNouns = null, string? Direction = null);

/// <summary>
///     A sentence a walkthrough types that changes the game, the room it is typed in, and every reading
///     of it the game answers identically. The first reading is the one the walkthrough was written
///     against.
/// </summary>
/// <param name="Reply">What the game says. Documentation only; nothing compares against it.</param>
/// <param name="From">The walkthrough it was first seen in.</param>
public sealed record WalkthroughInteraction(string Room, string Input, Reading[] Readings, string Reply, string From);

/// <summary>What a tool studying a walkthrough is given of its game, for the length of one step.</summary>
public sealed class GameUnderStudy
{
    public required ProbeParser Parser { get; init; }

    public required Func<string?, Task<string?>> Respond { get; init; }

    public required Func<string> Save { get; init; }

    /// <summary>Puts the game back as it was saved, and re-attaches whatever the fixture pins the dice with.</summary>
    public required Action<string> Restore { get; init; }

    public required Func<IContext> Context { get; init; }

    /// <param name="pinTheDice">
    ///     Re-attaches the fixture's random choosers. They live on the game's objects, and a restore
    ///     builds those anew.
    /// </param>
    public static GameUnderStudy Of<TGame, TContext>(GameEngine<TGame, TContext> engine, ProbeParser parser,
        Action pinTheDice)
        where TGame : IInfocomGame, new()
        where TContext : class, IContext, new()
    {
        // A "which one do you mean?" that a tried reading provoked is held by the engine, not by the
        // saved game, so putting the game back does not withdraw it - and the next sentence would be
        // taken as its answer. In play the question and the restore are never in the same engine; here
        // they are, so the engine is made to forget it. A question the saved game itself was waiting on
        // is rebuilt by the restore.
        var pendingQuestion = engine.GetType().GetField("_processorInProgress",
                                  BindingFlags.NonPublic | BindingFlags.Instance)
                              ?? throw new InvalidOperationException(
                                  "The engine no longer keeps its pending question in _processorInProgress.");

        return new GameUnderStudy
        {
            Parser = parser,
            Respond = engine.GetResponse,
            Save = engine.SaveGame,
            Restore = saved =>
            {
                pendingQuestion.SetValue(engine, null);
                engine.RestoreGame(saved);
                pinTheDice();
            },
            Context = () => engine.Context
        };
    }
}

/// <summary>A walkthrough fixture that lets a tool look at each step before it is taken.</summary>
public interface IStudiedWalkthrough
{
    Func<GameUnderStudy, string, Task>? BeforeEachStep { get; set; }
}

/// <summary>
///     Plays every walkthrough of a game and, at each sentence that reaches a parser and changes the
///     game, asks the game what else it would have accepted. The fork is the save-game: the state is
///     saved before the step, one reading is tried, the state is put back, the next is tried. A reading
///     is kept when the game ends up exactly where the walkthrough's own reading leaves it.
///     <para>
///         That is how the cases that need setup or two nouns ("unlock the grate with the key") get
///         into the interaction set: the walkthrough is the setup.
///     </para>
/// </summary>
public sealed class WalkthroughHarvest
{
    // "it" and "them" mean whatever the turn before was about, so the sentence cannot be judged alone.
    private static readonly Regex Pronoun = new(@"\b(it|them)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // What a model is told to call these, whatever the player typed (see the parser prompt).
    private static readonly string[] CanonicalVerbs = ["activate", "deactivate", "don", "doff"];

    private readonly Dictionary<string, WalkthroughInteraction> _found = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _studied = new(StringComparer.OrdinalIgnoreCase);
    private readonly DeterministicParser _grammar;
    private readonly VerbFamily[] _families;
    private string _walkthrough = string.Empty;

    /// <param name="gameName">The game's assembly name, which the vocabulary is keyed by.</param>
    public WalkthroughHarvest(string gameName)
    {
        // Both read the game's vocabulary, which builds its objects - so before any game is in play.
        Repository.Reset();
        _grammar = new DeterministicParser(gameName);
        _families = Repository.GetVerbs(gameName);
    }

    public IReadOnlyCollection<WalkthroughInteraction> Found => _found.Values;

    /// <summary>
    ///     Sentences the production grammar reads in a way the game does not answer as the walkthrough's
    ///     reading is answered. Each is a place the game would behave differently in play than under test.
    /// </summary>
    public List<string> GrammarDisagreements { get; } = [];

    /// <summary>
    ///     Sentences whose outcome was never the same twice running from the same saved game, wherever a
    ///     walkthrough types them: a room that throws the player out a random way, a death (the game that
    ///     starts over sets its clock at random), the first sight of an object that rolls its own starting
    ///     state. Nothing can be learned by trying readings against an outcome that will not hold still,
    ///     so they are left out.
    /// </summary>
    public IReadOnlyList<string> NotReproducible =>
        _notReproducible.Where(sentence => !_found.ContainsKey(sentence)).Order().ToList();

    private readonly HashSet<string> _notReproducible = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>What became of every sentence looked at: how many never reached a parser, changed nothing, were kept.</summary>
    public Dictionary<string, int> Tally { get; } = new()
    {
        ["typed"] = 0, ["has a pronoun"] = 0, ["never reached a parser as typed"] = 0, ["changed nothing"] = 0,
        ["kept"] = 0
    };

    /// <summary>The tally with the sentences that never held still counted in.</summary>
    public IReadOnlyDictionary<string, int> Summary => new Dictionary<string, int>(Tally)
    {
        ["typed"] = Tally["typed"] + NotReproducible.Count, ["not reproducible"] = NotReproducible.Count
    };

    /// <summary>
    ///     Plays every walkthrough fixture derived from <paramref name="walkthroughBase" />, as the test
    ///     runner does: the fixture's one-time setup, then each test method's cases in the order written.
    /// </summary>
    public async Task Replay(Type walkthroughBase, Action<string>? progress = null)
    {
        var fixtures = walkthroughBase.Assembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } && type.IsSubclassOf(walkthroughBase) &&
                           // A fixture derived from another fixture replays it through a different parser.
                           type.BaseType is { IsAbstract: true } &&
                           !type.IsDefined(typeof(ExplicitAttribute), true))
            .OrderBy(type => type.FullName, StringComparer.Ordinal);

        foreach (var type in fixtures)
        {
            var fixture = Activator.CreateInstance(type)!;
            if (fixture is not IStudiedWalkthrough studied)
                continue;

            _walkthrough = type.Name;
            studied.BeforeEachStep = Study;

            await Lifecycle(fixture, typeof(OneTimeSetUpAttribute));

            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(method => method.IsDefined(typeof(TestCaseAttribute)))
                .OrderBy(method => method.Name, StringComparer.Ordinal);

            foreach (var method in methods)
            foreach (var step in method.GetCustomAttributes<TestCaseAttribute>())
            {
                if (step.RunState != NUnit.Framework.Interfaces.RunState.Runnable)
                    continue;

                await Lifecycle(fixture, typeof(SetUpAttribute));

                try
                {
                    await Await(method.Invoke(fixture, Arguments(method, step.Arguments)));
                }
                catch (Exception problem)
                {
                    throw new InvalidOperationException(
                        $"{type.Name}.{method.Name} failed at \"{step.Arguments.FirstOrDefault()}\" while being " +
                        $"studied: {(problem.InnerException ?? problem).Message}", problem);
                }

                await Lifecycle(fixture, typeof(TearDownAttribute));
            }

            await Lifecycle(fixture, typeof(OneTimeTearDownAttribute));
            progress?.Invoke($"{type.Name}: {_found.Count} interactions so far");
        }
    }

    private async Task Study(GameUnderStudy game, string typed)
    {
        var input = typed.Trim();

        // Each sentence is studied once per game: the corpus it feeds holds an input once.
        if (!_studied.Add(input))
            return;

        Tally["typed"]++;

        if (Pronoun.IsMatch(input))
        {
            Tally["has a pronoun"]++;
            return;
        }

        var before = game.Save();
        var room = game.Context().CurrentLocation.GetType().Name;

        try
        {
            var walkthrough = await Turn(game, before, input, null);

            // Global commands, bare directions, conversation and answers to a question never reach a
            // parser; several sentences on one line reach it more than once.
            if (walkthrough.Handed.Count != 1 || !SameSentence(walkthrough.Asked, input) ||
                Describe(walkthrough.Handed[0]) is not { } reading)
            {
                Tally["never reached a parser as typed"]++;
                return;
            }

            var intent = walkthrough.Handed[0];

            // Only what changes the game: the state afterwards against a turn in which nothing happened.
            var nothing = await Turn(game, before, input, Nothing(intent, input));
            if (nothing.State == walkthrough.State)
            {
                Tally["changed nothing"]++;
                return;
            }

            // The same reading again must end in the same place, or comparing other readings with it
            // says nothing about them. Where it does not, the sentence may yet be typed somewhere it
            // does - the same command that is fatal in one walkthrough is routine in another.
            if ((await Turn(game, before, input, intent)).State != walkthrough.State)
            {
                _notReproducible.Add(input);
                _studied.Remove(input);
                Tally["typed"]--;
                return;
            }

            Tally["kept"]++;
            var readings = new List<Reading> { await WithSynonyms(game, before, input, intent, reading, walkthrough.State) };
            readings.AddRange(await OtherKinds(game, before, input, intent, readings[0], walkthrough));

            _found[input] = new WalkthroughInteraction(room, input, readings.ToArray(),
                Shorten(walkthrough.Reply), _walkthrough);
        }
        finally
        {
            // The walkthrough takes the step for real from exactly where it stood.
            game.Parser.Forced = null;
            game.Restore(before);
        }
    }

    // ------------------------------------------------------------------ trying a reading against the game

    private sealed record Outcome(string State, string Reply, List<IntentBase> Handed, string? Asked);

    private static async Task<Outcome> Turn(GameUnderStudy game, string before, string input, IntentBase? forced)
    {
        game.Restore(before);

        // The walkthrough pins the dice it cares about. Any others would make the same reading end in
        // different places on different tries, and no two readings could then be compared.
        InteractionCensus.PinTheDiceNobodyHasPinned();

        game.Parser.Forced = forced;
        game.Parser.Handed.Clear();
        game.Parser.LastSentence = null;

        string? reply;
        try
        {
            reply = await game.Respond(input);
        }
        catch (Exception problem)
        {
            reply = $"(threw {problem.GetType().Name})";
        }
        finally
        {
            game.Parser.Forced = null;
        }

        return new Outcome(InteractionCensus.StateOf(game.Context(), game.Save), reply ?? string.Empty,
            game.Parser.Handed.ToList(), game.Parser.LastSentence);
    }

    private static async Task<bool> Same(GameUnderStudy game, string before, string input, IntentBase reading,
        string state)
    {
        var outcome = await Turn(game, before, input, reading);
        return outcome.Handed.Count == 1 && outcome.State == state;
    }

    private static bool SameSentence(string? asked, string typed)
    {
        // "again" is parsed as the sentence before it, and a pronoun as the sentence it was rewritten to.
        return string.Equals(asked?.Trim(), typed, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>The same turn with a verb nothing answers: the clock moves and nothing else does.</summary>
    private static IntentBase Nothing(IntentBase intent, string input)
    {
        return intent is MultiNounIntent multi
            ? multi with { Verb = InteractionCensus.NothingVerb }
            : new SimpleIntent
            {
                Verb = InteractionCensus.NothingVerb, Noun = FirstNoun(intent), OriginalInput = input, Message = input
            };
    }

    // ------------------------------------------------------------------ the readings of one sentence

    private async Task<Reading> WithSynonyms(GameUnderStudy game, string before, string input, IntentBase intent,
        Reading reading, string state)
    {
        var grammar = _grammar.Parse(input);

        switch (intent)
        {
            case SimpleIntent simple:
            {
                var verbs = new List<string>(reading.Verbs);
                foreach (var verb in OtherVerbs(simple.Verb, (grammar as SimpleIntent)?.Verb))
                    if (await Same(game, before, input, simple with { Verb = verb }, state))
                        verbs.Add(verb);

                var nouns = new List<string>(reading.Nouns);
                foreach (var noun in OtherNouns(game, before, simple.Noun, (grammar as SimpleIntent)?.Noun))
                {
                    var reworded = Swap(simple.OriginalInput ?? input, simple.Noun, noun);
                    if (await Same(game, before, Swap(input, simple.Noun, noun),
                            simple with { Noun = noun, OriginalInput = reworded }, state))
                        nouns.Add(noun);
                }

                return reading with { Verbs = verbs.ToArray(), Nouns = nouns.ToArray() };
            }

            case MultiNounIntent multi:
            {
                var verbs = new List<string>(reading.Verbs);
                foreach (var verb in OtherVerbs(multi.Verb, (grammar as MultiNounIntent)?.Verb))
                    if (await Same(game, before, input, multi with { Verb = verb }, state))
                        verbs.Add(verb);

                var first = new List<string>(reading.Nouns);
                foreach (var noun in OtherNouns(game, before, multi.NounOne, (grammar as MultiNounIntent)?.NounOne))
                    if (await Same(game, before, Swap(input, multi.NounOne, noun),
                            multi with { NounOne = noun, OriginalInput = Swap(multi.OriginalInput, multi.NounOne, noun) },
                            state))
                        first.Add(noun);

                var second = new List<string>(reading.SecondNouns!);
                foreach (var noun in OtherNouns(game, before, multi.NounTwo, (grammar as MultiNounIntent)?.NounTwo))
                    if (await Same(game, before, Swap(input, multi.NounTwo, noun),
                            multi with { NounTwo = noun, OriginalInput = Swap(multi.OriginalInput, multi.NounTwo, noun) },
                            state))
                        second.Add(noun);

                return reading with { Verbs = verbs.ToArray(), Nouns = first.ToArray(), SecondNouns = second.ToArray() };
            }

            default:
                return reading;
        }
    }

    /// <summary>
    ///     Readings of another kind that the game answers the same way: the dedicated take and drop
    ///     intents a model produces where the walkthrough has a verb and a noun, a door entered as a move
    ///     inward, and whatever the production grammar makes of the sentence when that is a different kind
    ///     again.
    /// </summary>
    private async Task<List<Reading>> OtherKinds(GameUnderStudy game, string before, string input, IntentBase intent,
        Reading first, Outcome walkthrough)
    {
        var state = walkthrough.State;
        var readings = new List<Reading>();
        var others = new List<IntentBase>();

        // The one reading taken on the engine's word and not tried: a take or a drop arrives from a model
        // as its own kind of intent, and from the grammar as a verb and a noun, and both end at the same
        // processor. The dedicated intent cannot be tried here - on that road the engine asks OpenAI
        // which items are meant, and nothing in a test may.
        if (intent is SimpleIntent simple)
        {
            if (Verbs.TakeVerbs.Matches(simple.Verb))
                readings.Add(new Reading("take", ["take"], first.Nouns));

            if (Verbs.DropVerbs.Matches(simple.Verb))
                readings.Add(new Reading("drop", ["drop"], first.Nouns));
        }

        // A sentence the walkthrough reads as one noun, with the rest left in what was typed ("set laser
        // to 2": the laser reads the number out of the raw input), a model reads as two nouns joined by
        // the preposition. Where the game answers that reading the same way, it is as good.
        if (intent is SimpleIntent one && AsTwoNouns(one, input) is { } two)
            others.Add(two);

        if (intent is EnterSubLocationIntent board)
            others.Add(new MoveIntent { Direction = Direction.In, Noun = board.Noun, Message = input });

        if (intent is ExitSubLocationIntent disembark)
            others.Add(new MoveIntent { Direction = Direction.Out, Noun = disembark.NounOne, Message = input });

        foreach (var other in others)
            if (await Same(game, before, input, other, state) && Describe(other) is { } reading)
                readings.Add(await WithSynonyms(game, before, input, other, reading, state));

        // The grammar's own reading, where it is not simply the walkthrough's in other words.
        if (_grammar.Parse(input) is { } grammar && Describe(grammar) is { } read && !Covers(first, read) &&
            !readings.Any(reading => Covers(reading, read)))
        {
            var asTheGrammarReadsIt = await Turn(game, before, input, grammar);

            if (asTheGrammarReadsIt.Handed.Count == 1 && asTheGrammarReadsIt.State == state)
                readings.Add(read);
            else
                GrammarDisagreements.Add(
                    $"{input}: the grammar reads [{Show(read)}] and the game says \"{Shorten(asTheGrammarReadsIt.Reply)}\"; " +
                    $"the walkthrough reads [{Show(first)}] and the game says \"{Shorten(walkthrough.Reply)}\"");
        }

        return readings;
    }

    private static readonly Regex TwoNouns = new(
        @"\s(to|with|using|in|into|inside|on|onto|at|from|through|under|off|across|against|over|behind)\s+(?:the\s+|a\s+|an\s+)?(.+?)[.!?]*$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>"set laser to 2" as a model reads it: the noun the walkthrough has, the preposition, and what follows.</summary>
    private static MultiNounIntent? AsTwoNouns(SimpleIntent one, string input)
    {
        if (string.IsNullOrWhiteSpace(one.Noun))
            return null;

        var named = Regex.Match(input, $@"\b{Regex.Escape(one.Noun)}\b", RegexOptions.IgnoreCase);
        if (!named.Success)
            return null;

        var rest = TwoNouns.Match(input[(named.Index + named.Length)..]);
        if (!rest.Success || rest.Index != 0)
            return null;

        return new MultiNounIntent
        {
            Verb = one.Verb, NounOne = one.Noun, Preposition = rest.Groups[1].Value.ToLowerInvariant(),
            NounTwo = rest.Groups[2].Value.Trim().ToLowerInvariant(), OriginalInput = one.OriginalInput ?? input,
            Message = input
        };
    }

    private static bool Covers(Reading reading, Reading other)
    {
        static bool Has(string[]? accepted, string[]? wanted) =>
            (wanted ?? []).All(word => (accepted ?? []).Contains(word, StringComparer.OrdinalIgnoreCase));

        return reading.Kind == other.Kind && Has(reading.Verbs, other.Verbs) && Has(reading.Nouns, other.Nouns) &&
               Has(reading.SecondNouns, other.SecondNouns) &&
               string.Equals(reading.Preposition, other.Preposition, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(reading.Direction, other.Direction, StringComparison.OrdinalIgnoreCase);
    }

    private static string Show(Reading reading)
    {
        return string.Join(' ', new[]
        {
            reading.Kind, reading.Verbs.FirstOrDefault(), reading.Nouns.FirstOrDefault(), reading.Preposition,
            reading.SecondNouns?.FirstOrDefault(), reading.Direction
        }.Where(part => !string.IsNullOrEmpty(part)));
    }

    /// <summary>The words that share a grouping with this verb, and the names a model gives the common ones.</summary>
    private IEnumerable<string> OtherVerbs(string verb, string? grammarVerb)
    {
        return _families.Where(family => family.Matches(verb)).SelectMany(family => family)
            .Concat(CanonicalVerbs).Append(grammarVerb ?? verb)
            .Select(word => word.ToLowerInvariant()).Distinct()
            .Where(word => !word.Equals(verb, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    /// <summary>The other names of the thing this noun names, as the game stands before the step.</summary>
    private static IEnumerable<string> OtherNouns(GameUnderStudy game, string before, string? noun, string? grammarNoun)
    {
        if (string.IsNullOrWhiteSpace(noun))
            return [];

        game.Restore(before);

        string[] names;
        try
        {
            names = Repository.GetItemInScope(noun, game.Context())?.NounsForMatching ?? [];
        }
        catch
        {
            names = [];
        }

        return names.Append(grammarNoun ?? noun).Select(name => name.ToLowerInvariant()).Distinct()
            .Where(name => !name.Equals(noun, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    private static Reading? Describe(IntentBase intent)
    {
        static string Lower(string? text) => (text ?? string.Empty).Trim().ToLowerInvariant();

        return intent switch
        {
            SimpleIntent { Noun: { Length: > 0 } } simple => new Reading("act", [Lower(simple.Verb)], [Lower(simple.Noun)]),
            MultiNounIntent multi => new Reading("act", [Lower(multi.Verb)], [Lower(multi.NounOne)],
                Lower(multi.Preposition), [Lower(multi.NounTwo)]),
            TakeIntent { Noun: { Length: > 0 } } take => new Reading("take", ["take"], [Lower(take.Noun)]),
            DropIntent { Noun: { Length: > 0 } } drop => new Reading("drop", ["drop"], [Lower(drop.Noun)]),
            EnterSubLocationIntent board => new Reading("board", ["board"], [Lower(board.Noun)]),
            ExitSubLocationIntent disembark => new Reading("disembark", ["exit"], [Lower(disembark.NounOne)]),
            GoToDestinationIntent goTo => new Reading("goto", ["go"], [Lower(goTo.Destination)]),
            MoveIntent move when move.Direction != Direction.Unknown => new Reading("move", ["go"],
                string.IsNullOrWhiteSpace(move.Noun) ? [] : [Lower(move.Noun)], Direction: move.Direction.ToString()),
            _ => null
        };
    }

    private static string? FirstNoun(IntentBase intent)
    {
        return intent switch
        {
            SimpleIntent simple => simple.Noun,
            TakeIntent take => take.Noun,
            DropIntent drop => drop.Noun,
            EnterSubLocationIntent board => board.Noun,
            ExitSubLocationIntent disembark => disembark.NounOne,
            GoToDestinationIntent goTo => goTo.Destination,
            MoveIntent move => move.Noun,
            _ => null
        };
    }

    /// <summary>The sentence with one name for a thing put in place of another, where it uses that name.</summary>
    private static string Swap(string sentence, string? was, string becomes)
    {
        return string.IsNullOrWhiteSpace(was)
            ? sentence
            : new Regex($@"\b{Regex.Escape(was)}\b", RegexOptions.IgnoreCase).Replace(sentence, becomes, 1);
    }

    private static string Shorten(string reply)
    {
        var oneLine = string.Join(' ', reply.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        return oneLine.Length > 160 ? oneLine[..160] + "..." : oneLine;
    }

    // ------------------------------------------------------------------ running a fixture as the runner does

    private static async Task Lifecycle(object fixture, Type attribute)
    {
        static int Depth(Type? type) => type is null ? 0 : 1 + Depth(type.BaseType);

        var methods = fixture.GetType()
            .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(method => method.IsDefined(attribute, true) && method.GetParameters().Length == 0)
            .OrderBy(method => Depth(method.DeclaringType));

        // Setup runs from the base class down, teardown from the fixture up.
        var tearingDown = attribute == typeof(TearDownAttribute) || attribute == typeof(OneTimeTearDownAttribute);

        foreach (var method in tearingDown ? methods.Reverse() : methods)
            await Await(method.Invoke(fixture, null));
    }

    private static async Task Await(object? result)
    {
        if (result is Task task)
            await task;
    }

    /// <summary>A [TestCase]'s arguments as the method takes them: the trailing ones gathered into its params array.</summary>
    private static object?[] Arguments(MethodInfo method, object?[] given)
    {
        var parameters = method.GetParameters();
        if (parameters.Length == 0 || !parameters[^1].IsDefined(typeof(ParamArrayAttribute)))
            return given;

        var fixedCount = parameters.Length - 1;
        var elementType = parameters[^1].ParameterType.GetElementType()!;
        var rest = given.Skip(fixedCount).ToArray();

        var gathered = Array.CreateInstance(elementType, rest.Length);
        for (var index = 0; index < rest.Length; index++)
            gathered.SetValue(rest[index], index);

        return given.Take(fixedCount).Concat(Enumerable.Repeat<object?>(null, Math.Max(0, fixedCount - given.Length)))
            .Append(gathered).ToArray();
    }

    // ------------------------------------------------------------------ the file

    public static void Save(string path, IEnumerable<WalkthroughInteraction> interactions)
    {
        var ordered = interactions.OrderBy(interaction => interaction.Room, StringComparer.Ordinal)
            .ThenBy(interaction => interaction.Input, StringComparer.OrdinalIgnoreCase);

        File.WriteAllText(path, JsonConvert.SerializeObject(ordered, Formatting.Indented,
            new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }) + "\n");
    }

    /// <summary>
    ///     Where a game's walkthrough interactions are kept in the source tree, for writing them. They are
    ///     read back from the test assembly, which they are compiled into.
    /// </summary>
    public static string PathFor(string fileName)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Zork.sln")))
            directory = directory.Parent;

        if (directory is null)
            throw new InvalidOperationException("Could not find the repository root above the test directory.");

        return Path.Combine(directory.FullName, "UnitTests", "Parser", "Walkthroughs", fileName);
    }

    /// <summary>A game's walkthrough interactions as they were when the tests were built; none if never generated.</summary>
    public static IReadOnlyList<WalkthroughInteraction> Load(string fileName)
    {
        return EmbeddedFile.Read(typeof(WalkthroughHarvest).Assembly, fileName) is { } json
            ? JsonConvert.DeserializeObject<List<WalkthroughInteraction>>(json) ?? []
            : [];
    }
}
