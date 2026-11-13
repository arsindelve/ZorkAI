using Model.Intent;
using Model.Movement;

namespace UnitTests.Parser;

/// <summary>
///     What a model that understood the player would answer, in provider-neutral terms: the kind of
///     intent, the verb, the nouns, the preposition joining them, the direction. A scripted AI is built
///     from this - today by rendering it as OpenAI's structured output, later as whatever the next
///     provider returns - so the cases themselves never mention a provider.
/// </summary>
public sealed record ModelAnswer(
    string Intent,
    string? Verb = null,
    string[]? Nouns = null,
    string? Preposition = null,
    string? Direction = null,
    string? Adjective = null);

/// <summary>
///     What the engine must be handed, whichever layer produced it. Every field is optional and each
///     accepts alternatives, because two correct parses of one sentence need not be identical: "light the
///     lamp" may come back as "light" or as "activate", and both reach the same handler.
/// </summary>
public sealed class IntentExpectation
{
    public required Type IntentType { get; init; }
    public string[]? Verbs { get; init; }
    public string[]? NounOne { get; init; }
    public string[]? NounTwo { get; init; }
    public string[]? Prepositions { get; init; }
    public string? Adverb { get; init; }
    public Direction? Direction { get; init; }
    public bool EitherNounOrder { get; init; }

    /// <summary>Another parse the engine handles just as well; satisfying either is enough.</summary>
    public IntentExpectation? Alternative { get; init; }

    public void Verify(IntentBase? actual, string input, string layer)
    {
        var problems = new List<string>();
        for (var expectation = this; expectation is not null; expectation = expectation.Alternative)
        {
            var problem = expectation.Mismatch(actual, input);
            if (problem is null)
                return;

            problems.Add(problem);
        }

        // What the parser was handed (the model's answer, rendered as tags) is the first thing anyone
        // reading a failure against a live model wants to see.
        var raw = string.IsNullOrWhiteSpace(actual?.Message)
            ? string.Empty
            : $" The parser was handed: {actual.Message.ReplaceLineEndings(" ").Trim()}";

        Assert.Fail(
            $"\"{input}\" resolved by {layer}: {string.Join(" Nor is it the alternative: ", problems)}{raw}");
    }

    /// <summary>What is wrong with <paramref name="actual" />, or null when it is what this expects.</summary>
    private string? Mismatch(IntentBase? actual, string input)
    {
        if (actual is null)
            return $"expected {IntentType.Name}, but nothing was resolved.";

        if (actual.GetType() != IntentType)
            return $"expected {IntentType.Name}, but got {actual.GetType().Name} ({Describe(actual)}).";

        // Handlers read the raw sentence (the rug checks it for "under"), so it must survive the parse.
        var original = input.Trim();

        return actual switch
        {
            SimpleIntent simple =>
                NotOneOf(Verbs, simple.Verb, "verb")
                ?? NotOneOf(NounOne, simple.Noun, "noun")
                ?? (Adverb is null ? null : NotOneOf([Adverb], simple.Adverb, "adverb"))
                ?? NotOneOf([original], simple.OriginalInput, "original input"),

            MultiNounIntent multi when EitherNounOrder && Matches(NounOne, multi.NounTwo) &&
                                       Matches(NounTwo, multi.NounOne) => NotOneOf(Verbs, multi.Verb, "verb"),

            MultiNounIntent multi =>
                NotOneOf(Verbs, multi.Verb, "verb")
                ?? NotOneOf(NounOne, multi.NounOne, "first noun")
                ?? NotOneOf(NounTwo, multi.NounTwo, "second noun")
                ?? NotOneOf(Prepositions, multi.Preposition, "preposition")
                ?? NotOneOf([original], multi.OriginalInput, "original input"),

            TakeIntent take => NotOneOf(NounOne, take.Noun, "noun")
                               ?? NotOneOf([original], take.OriginalInput, "original input"),

            DropIntent drop => NotOneOf(NounOne, drop.Noun, "noun")
                               ?? NotOneOf([original], drop.OriginalInput, "original input"),

            EnterSubLocationIntent board => NotOneOf(NounOne, board.Noun, "noun"),
            ExitSubLocationIntent disembark => NotOneOf(NounOne, disembark.NounOne, "noun"),
            GoToDestinationIntent goTo => NotOneOf(NounOne, goTo.Destination, "destination"),

            MoveIntent move when Direction is not null && move.Direction != Direction =>
                $"the direction was {move.Direction} and must be {Direction}.",
            MoveIntent move => NotOneOf(NounOne, move.Noun, "noun"),

            _ => null
        };
    }

    private static string Describe(IntentBase intent)
    {
        return intent switch
        {
            SimpleIntent simple => $"verb '{simple.Verb}', noun '{simple.Noun}'",
            MultiNounIntent multi => $"'{multi.Verb}' '{multi.NounOne}' '{multi.Preposition}' '{multi.NounTwo}'",
            MoveIntent move => $"{move.Direction}",
            GoToDestinationIntent goTo => $"destination '{goTo.Destination}'",
            EnterSubLocationIntent board => $"noun '{board.Noun}'",
            ExitSubLocationIntent disembark => $"noun '{disembark.NounOne}'",
            _ => intent.GetType().Name
        };
    }

    private static bool Matches(string[]? accepted, string? actual)
    {
        return accepted is null ||
               accepted.Any(option => option.Equals(actual, StringComparison.InvariantCultureIgnoreCase));
    }

    private static string? NotOneOf(string[]? accepted, string? actual, string what)
    {
        return Matches(accepted, actual)
            ? null
            : $"the {what} was \"{actual}\" and must be one of: {string.Join(", ", accepted!)}.";
    }
}

/// <summary>Where a line of player input gets its meaning.</summary>
public enum ResolvedBy
{
    /// <summary>A global or system command, or a bare direction: answered before any parsing.</summary>
    BeforeParsing,

    /// <summary>The deterministic grammar, with no model in the loop.</summary>
    Grammar,

    /// <summary>The AI parser, because the grammar declined.</summary>
    Ai
}

/// <summary>
///     One line of player input and what it must mean. The same case is checked at every layer that can
///     see it: the grammar must resolve it or decline it as the case says, the AI asked directly must
///     produce the expected intent, and the production parser must answer from the right one of the two.
/// </summary>
public sealed class ParserCase
{
    public required string Game { get; init; }

    /// <summary>The class name of the room the player is standing in; the AI is shown its description.</summary>
    public required string Location { get; init; }

    public required string Input { get; init; }

    public required ResolvedBy ResolvedBy { get; init; }

    /// <summary>What the grammar must produce. Null when it must decline.</summary>
    public IntentExpectation? FromGrammar { get; init; }

    /// <summary>What the AI must produce when asked. Null only for input that never reaches a parser.</summary>
    public IntentExpectation? FromAi { get; init; }

    /// <summary>The answer a scripted AI gives for this input.</summary>
    public ModelAnswer? ModelAnswer { get; init; }

    /// <summary>For input that never reaches a parser: what the earlier stage makes of it.</summary>
    public IntentExpectation? BeforeParsing { get; init; }

    public override string ToString()
    {
        return $"{Game}: {Input}";
    }
}

/// <summary>
///     The vocabulary cases are written in: <c>Act("open", "mailbox")</c>, <c>Take("lamp")</c>,
///     <c>Move("north")</c>. Each describes the canonical meaning once and derives from it the scripted
///     model answer, what the AI path must produce, and what the grammar must produce.
/// </summary>
public sealed class Meaning
{
    private readonly string _kind;
    private readonly string? _verb;
    private readonly string[] _nouns;
    private readonly string? _preposition;
    private readonly string? _direction;

    private readonly List<string> _otherVerbs = [];
    private readonly List<string> _otherNounOnes = [];
    private readonly List<string> _otherNounTwos = [];
    private readonly List<string> _otherPrepositions = [];
    private string? _typedVerb;
    private string? _adverb;
    private bool _aiOnly;
    private bool _anyVerb;
    private bool _anyNoun;
    private bool _eitherNounOrder;
    private bool _anyDirection;
    private ModelAnswer? _scripted;
    private IntentExpectation? _expectedFromAi;
    private readonly List<Meaning> _aiAlternatives = [];

    private Meaning(string kind, string? verb, string[] nouns, string? preposition = null, string? direction = null)
    {
        _kind = kind;
        _verb = verb;
        _nouns = nouns;
        _preposition = preposition;
        _direction = direction;
    }

    public static Meaning Act(string verb, string noun) => new("act", verb, [noun]);

    public static Meaning Act(string verb, string nounOne, string preposition, string nounTwo) =>
        new("act", verb, [nounOne, nounTwo], preposition);

    public static Meaning Take(string noun) => new("take", "take", [noun]);

    public static Meaning Drop(string noun) => new("drop", "drop", [noun]);

    public static Meaning Board(string noun) => new("board", "board", [noun]);

    public static Meaning Disembark(string noun) => new("disembark", "exit", [noun]) { _aiOnly = true };

    public static Meaning Move(string direction) => new("move", "go", [], direction: direction) { _aiOnly = true };

    /// <summary>A move that names the thing moved through: "enter the blue door" read as going in.</summary>
    public static Meaning Move(string direction, string through) =>
        new("move", "go", [through], direction: direction) { _aiOnly = true };

    public static Meaning GoTo(string destination) => new("goto", "go", [destination]);

    public static Meaning Look() => new("look", null, []) { _aiOnly = true };

    public static Meaning Inventory() => new("inventory", null, []) { _aiOnly = true };

    /// <summary>Several separate commands run together on one line, which the game asks to be split up.</summary>
    public static Meaning SeveralCommands() => new("multiple", null, []) { _aiOnly = true };

    /// <summary>Input that means nothing the game can act on.</summary>
    public static Meaning Nothing() => new("act", null, []) { _aiOnly = true };

    /// <summary>The grammar must decline this input; only the AI resolves it.</summary>
    public Meaning AiOnly()
    {
        _aiOnly = true;
        return this;
    }

    /// <summary>The grammar resolves it too (for kinds that default to AI-only, such as a move).</summary>
    public Meaning AlsoByGrammar()
    {
        _aiOnly = false;
        return this;
    }

    /// <summary>
    ///     The verb the grammar reports, when it differs from the canonical one: the player typed "light",
    ///     which the grammar passes through and a model reduces to "activate".
    /// </summary>
    public Meaning TypedAs(string verb)
    {
        _typedVerb = verb;
        return this;
    }

    public Meaning OrVerb(params string[] verbs)
    {
        _otherVerbs.AddRange(verbs);
        return this;
    }

    public Meaning OrNoun(params string[] nouns)
    {
        _otherNounOnes.AddRange(nouns);
        return this;
    }

    public Meaning OrSecondNoun(params string[] nouns)
    {
        _otherNounTwos.AddRange(nouns);
        return this;
    }

    public Meaning OrPreposition(params string[] prepositions)
    {
        _otherPrepositions.AddRange(prepositions);
        return this;
    }

    /// <summary>Any verb will do from the AI: the sentence has no single right one ("yeet the sword").</summary>
    public Meaning AnyVerb()
    {
        _anyVerb = true;
        return this;
    }

    /// <summary>The AI need not name the object ("grab the bottle" is a take either way).</summary>
    public Meaning AnyNoun()
    {
        _anyNoun = true;
        return this;
    }

    public Meaning EitherNounOrder()
    {
        _eitherNounOrder = true;
        return this;
    }

    /// <summary>The sentence is a move, but which way depends on reading the room ("keep following the path").</summary>
    public Meaning AnyDirection()
    {
        _anyDirection = true;
        return this;
    }

    /// <summary>The preposition that follows the verb and rides along as the intent's adverb.</summary>
    public Meaning WithAdverb(string adverb)
    {
        _adverb = adverb;
        return this;
    }

    /// <summary>
    ///     Script the model to answer something other than the canonical parse - a known way real models
    ///     get this sentence wrong - while the expected intent stays what the engine needs. This is how
    ///     the pipeline's repairs are held in place.
    /// </summary>
    public Meaning WhenTheModelSays(ModelAnswer degraded)
    {
        _scripted = degraded;
        return this;
    }

    /// <summary>Override what the AI path must produce, where it is not the plain reading of the kind.</summary>
    public Meaning AiProduces(IntentExpectation expectation)
    {
        _expectedFromAi = expectation;
        return this;
    }

    /// <summary>
    ///     Another parse the AI may give that the engine handles just as well: "enter the blue door" as
    ///     a board, as a move inward that names the door, or as a move the way the door leads. Call it
    ///     once for each such parse - and only for one the engine really does answer identically.
    /// </summary>
    public Meaning OrFromAi(Meaning alternative)
    {
        _aiAlternatives.Add(alternative);
        return this;
    }

    internal ParserCase Build(string game, string location, string input)
    {
        return new ParserCase
        {
            Game = game,
            Location = location,
            Input = input,
            ResolvedBy = _aiOnly ? ResolvedBy.Ai : ResolvedBy.Grammar,
            ModelAnswer = _scripted ?? new ModelAnswer(_kind, _verb, _nouns, _preposition ?? _adverb, _direction),
            FromAi = _expectedFromAi ?? ExpectedFromAi(),
            FromGrammar = _aiOnly ? null : ExpectedFromGrammar()
        };
    }

    private string[] Verbs()
    {
        return new[] { _verb, _typedVerb }.Where(verb => verb is not null).Select(verb => verb!)
            .Concat(_otherVerbs).Distinct().ToArray();
    }

    private string[] NounOnes() => [_nouns[0], .._otherNounOnes];

    private string[] NounTwos() => [_nouns[1], .._otherNounTwos];

    private IntentExpectation ExpectedFromAi()
    {
        // The alternatives hang off the expectation as a chain, tried in the order they were given.
        IntentExpectation? next = null;
        foreach (var alternative in Enumerable.Reverse(_aiAlternatives))
            next = WithAlternative(alternative.ExpectedFromAiAlone(), next);

        return WithAlternative(ExpectedFromAiAlone(), next);
    }

    private static IntentExpectation WithAlternative(IntentExpectation expected, IntentExpectation? alternative)
    {
        if (alternative is null)
            return expected;

        return new IntentExpectation
        {
            IntentType = expected.IntentType, Verbs = expected.Verbs, NounOne = expected.NounOne,
            NounTwo = expected.NounTwo, Prepositions = expected.Prepositions, Adverb = expected.Adverb,
            Direction = expected.Direction, EitherNounOrder = expected.EitherNounOrder, Alternative = alternative
        };
    }

    private IntentExpectation ExpectedFromAiAlone()
    {
        return _kind switch
        {
            "multiple" => new IntentExpectation { IntentType = typeof(MultipleCommandsIntent) },
            "act" when _nouns.Length == 0 => new IntentExpectation { IntentType = typeof(NullIntent) },
            "act" when _nouns.Length == 2 => new IntentExpectation
            {
                IntentType = typeof(MultiNounIntent), Verbs = _anyVerb ? null : Verbs(), NounOne = NounOnes(),
                NounTwo = NounTwos(), Prepositions = [_preposition!, .._otherPrepositions],
                EitherNounOrder = _eitherNounOrder
            },
            "act" => new IntentExpectation
            {
                IntentType = typeof(SimpleIntent), Verbs = _anyVerb ? null : Verbs(), NounOne = NounOnes(),
                Adverb = _adverb
            },
            "take" => new IntentExpectation { IntentType = typeof(TakeIntent), NounOne = _anyNoun ? null : NounOnes() },
            "drop" => new IntentExpectation { IntentType = typeof(DropIntent), NounOne = _anyNoun ? null : NounOnes() },
            "board" => new IntentExpectation { IntentType = typeof(EnterSubLocationIntent), NounOne = NounOnes() },
            "disembark" => new IntentExpectation { IntentType = typeof(ExitSubLocationIntent), NounOne = NounOnes() },
            "goto" => new IntentExpectation { IntentType = typeof(GoToDestinationIntent), NounOne = NounOnes() },
            "move" => new IntentExpectation
            {
                IntentType = typeof(MoveIntent),
                Direction = _anyDirection ? null : DirectionParser.ParseDirection(_direction),
                NounOne = _nouns.Length == 0 ? null : NounOnes()
            },
            "look" => new IntentExpectation { IntentType = typeof(LookIntent) },
            "inventory" => new IntentExpectation { IntentType = typeof(InventoryIntent) },
            _ => throw new InvalidOperationException(_kind)
        };
    }

    private IntentExpectation ExpectedFromGrammar()
    {
        // The grammar reports the verb the player typed and never guesses, so its expectation is exact.
        string[] verb = [_typedVerb ?? _verb!];

        return _kind switch
        {
            "act" when _nouns.Length == 2 => new IntentExpectation
            {
                IntentType = typeof(MultiNounIntent), Verbs = verb, NounOne = [_nouns[0]], NounTwo = [_nouns[1]],
                Prepositions = [_preposition!]
            },
            // To the grammar a take or a drop is a verb and a noun like any other; only the AI path has
            // the dedicated intents. Both routes end at the same processor.
            "act" or "take" or "drop" => new IntentExpectation
                { IntentType = typeof(SimpleIntent), Verbs = verb, NounOne = [_nouns[0]], Adverb = _adverb },
            "board" => new IntentExpectation { IntentType = typeof(EnterSubLocationIntent), NounOne = [_nouns[0]] },
            "goto" => new IntentExpectation { IntentType = typeof(GoToDestinationIntent), NounOne = [_nouns[0]] },
            "move" => new IntentExpectation
                { IntentType = typeof(MoveIntent), Direction = DirectionParser.ParseDirection(_direction) },
            _ => throw new InvalidOperationException($"The grammar never resolves a '{_kind}'.")
        };
    }
}
