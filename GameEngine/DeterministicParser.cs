using System.Collections.Concurrent;
using Model;
using Model.Intent;
using Model.Movement;

namespace GameEngine;

/// <summary>
/// Layer 1 of the parser plan: a deterministic, vocabulary-driven parser for standard interactive-fiction
/// grammar — <c>verb [noun] [preposition noun]</c> over the game's own vocabulary. It resolves the vast
/// majority of real Zork commands instantly and reproducibly, with NO AI call:
///
///   * "go to &lt;place&gt;" / "walk into &lt;place&gt;"        -> GoToDestinationIntent
///   * "put &lt;item&gt; in &lt;container&gt;", "kill &lt;x&gt; with &lt;y&gt;", "tie &lt;a&gt; to &lt;b&gt;" -> MultiNounIntent
///   * "look under &lt;x&gt;" / "peer through &lt;x&gt;"        -> SimpleIntent (adverb)
///   * "take &lt;x&gt;", "open &lt;x&gt;", "turn on &lt;x&gt;"      -> SimpleIntent
///
/// The AI parser is only for input this grammar can't cleanly extract — conversational / verbose / polite
/// phrasing ("would you please put the sword in the case, ok?"). Standard grammar never touches the AI.
///
/// It stays conservative: it returns null (defer to the AI) whenever it cannot confidently resolve the verb
/// AND the noun(s) against the known vocabulary, so it is never wrong, only sometimes silent. Both halves of
/// that vocabulary are harvested from the game rather than listed here: nouns from what items and locations
/// declare, verbs from every <see cref="VerbFamily" /> a handler matches on (<see cref="Repository.GetVerbs" />).
/// Multi-word verb phrases ("turn on", "put on", "look at") are canonicalised to the engine's canonical verb.
/// </summary>
public class DeterministicParser
{
    private static readonly ConcurrentDictionary<string, Vocabulary> VocabCache = new();

    private readonly Vocabulary _vocab;

    // Multi-word verb phrases -> the engine's canonical single verb. These MUST be canonicalised because the
    // second word would otherwise be read as a preposition/noun ("turn on lamp" -> verb "turn", "on lamp").
    //
    // This is a table of REWRITES, not vocabulary, which is why it is not harvested like the verbs are. A
    // phrase can sit in several families whose owners want different things from it, and the only reading
    // that is safe everywhere is the one the AI parser's prompt already makes ("turn on" -> "activate").
    // Phrases that are not listed here are deliberately left to the AI: recognising one verbatim just because
    // some object declares it would, for example, turn "put down the lamp" into a verb only Stationfall's
    // goo answers, instead of the drop the AI makes of it.
    private static readonly Dictionary<string, string> MultiWordVerbCanon = new(StringComparer.OrdinalIgnoreCase)
    {
        ["turn on"] = "activate",
        ["switch on"] = "activate",
        ["turn off"] = "deactivate",
        ["switch off"] = "deactivate",
        ["put on"] = "don",
        ["take off"] = "doff",
        ["pick up"] = "take",
        ["look at"] = "examine",
        ["look in"] = "examine",
        ["look into"] = "examine",
        ["look inside"] = "examine"
    };

    // Prepositions that connect two nouns ("kill troll WITH sword", "put sword IN case", "tie rope TO
    // railing"). "under"/"through" are also here but the leading exploratory case (rest[0]) is handled first.
    private static readonly HashSet<string> ConnectingPrepositions = new(StringComparer.OrdinalIgnoreCase)
    {
        "with", "using", "to", "in", "into", "inside", "on", "onto", "from", "at", "against", "about",
        "under", "through", "over", "behind", "beside"
    };

    // Verbs that lead an EXPLORATORY "verb under/through <object>" command ("look under rug", "peer through
    // crack"). Look/examine family only, NOT movement verbs — "go through <opening>" is a request to MOVE
    // through it, which the AI resolves to a MoveIntent, so those fall through to the AI.
    private static readonly VerbFamily ExploratoryVerbs =
        new("look", "peer", "peek", "glance", "examine", "search", "inspect");

    // "enter <thing>" / "board <thing>" is a request to get into it, which is its own intent.
    private static readonly VerbFamily BoardVerbs = new("enter", "board");

    // Getting OUT of a thing is the disembark intent, and only the AI path builds that (with the door and
    // sub-location handling that goes with it). These are declined, not parsed. The trap: because verbs are
    // harvested, "exit" and "leave" become known the moment any object answers them (Planetfall's safety
    // webbing does), and "exit the blue door" would then come out as a verb+noun that nothing answers.
    private static readonly VerbFamily DisembarkVerbs = new("exit", "leave", "disembark");

    private static readonly string[] TravelPrefixes =
        ["go to ", "walk to ", "head to ", "travel to ", "go into ", "walk into ", "run to ", "proceed to "];

    public DeterministicParser(string gameName)
    {
        _vocab = VocabCache.GetOrAdd(gameName, Load);
    }

    private static Vocabulary Load(string gameName)
    {
        // The vocabulary is every noun the GAME declares — item NounsForMatching plus each location's
        // NounsForMatching and scenery nouns — harvested game-agnostically by the Repository. The engine
        // deliberately holds NO per-game noun list of its own: scenery nouns live on the locations that
        // handle them (LocationBase.SceneryNouns), so a new game or a new piece of scenery is understood
        // here automatically, without touching the engine.
        //
        // GetSceneryNouns is unioned in for the game-wide scenery (IInfocomGame.GlobalScenery) that lives on
        // the game itself, not on any item or location, so GetNouns cannot see it.
        var nouns = new HashSet<string>(Repository.GetNouns(gameName), StringComparer.OrdinalIgnoreCase);
        nouns.UnionWith(Repository.GetSceneryNouns(gameName));

        // Room names are a separate vocabulary: "kitchen" is a destination, not a noun. Stored padded and
        // article-free so IsKnownDestination can do DestinationNavigation's whole-word phrase match.
        var destinations = Repository.GetDestinationNames(gameName)
            .Select(name => $" {Normalize(name)} ")
            .Distinct()
            .ToArray();

        // The verbs are harvested exactly as the nouns are: every word of every VerbFamily the engine or
        // this game declares (Repository.GetVerbs). There is no verb list in this class to fall behind -
        // a handler cannot answer a verb that is not in a family. Single words only: a multi-word phrase
        // reaches the grammar through MultiWordVerbCanon or not at all.
        var verbs = Repository.GetVerbs(gameName)
            .SelectMany(family => family)
            .Where(word => !word.Contains(' '))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return new Vocabulary(nouns, destinations, verbs);
    }

    /// <summary>
    ///     True when <paramref name="destination" /> is a whole word or phrase of some room's name or alias -
    ///     the rule <c>DestinationNavigation</c> resolves "go to" by ("office" -> "Lab Office"). A word that
    ///     names no room defers to the AI rather than becoming a "go to" the engine can only refuse.
    /// </summary>
    private bool IsKnownDestination(string destination)
    {
        var target = $" {destination} ";
        return _vocab.DestinationTerms.Any(term => term.Contains(target, StringComparison.Ordinal));
    }

    /// <summary>
    /// Attempts a deterministic parse. Returns the intent when confident; null to signal "let the AI parse".
    /// </summary>
    public IntentBase? Parse(string? rawInput)
    {
        if (string.IsNullOrWhiteSpace(rawInput))
            return null;

        var original = rawInput.Trim();
        var input = Normalize(original);
        if (input.Length == 0)
            return null;

        // 1. "go to <place>" / "walk into <place>" -> destination navigation.
        foreach (var prefix in TravelPrefixes)
        {
            if (!input.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                continue;

            var destination = input[prefix.Length..].Trim();
            if (string.IsNullOrWhiteSpace(destination)) return null;
            var dir = DirectionParser.ParseDirection(destination);
            if (dir != Direction.Unknown)
                return new MoveIntent { Direction = dir, Message = original };
            // Gate on ROOM names, not nouns: "kitchen" is not a noun, so checking the noun vocabulary here
            // rejected every real destination and sent "go to the kitchen" to the AI.
            return IsKnownDestination(destination)
                ? new GoToDestinationIntent { Destination = destination, Message = original }
                : null;
        }

        var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length < 2)
            return null;

        // 2. Resolve the verb: a leading multi-word phrase canonicalises ("turn on" -> "activate");
        //    otherwise the first word must be a verb the engine actually knows.
        string verb;
        string[] rest;
        if (MultiWordVerbCanon.TryGetValue($"{words[0]} {words[1]}", out var canonical))
        {
            verb = canonical;
            rest = words[2..];
        }
        else
        {
            verb = words[0];
            rest = words[1..];
            if (!_vocab.Verbs.Contains(verb))
                return null; // not an imperative we recognise -> let the AI try
        }

        if (rest.Length == 0 || DisembarkVerbs.Matches(verb))
            return null;

        // 3. Exploratory "look/peer under|through <object>" -> SimpleIntent with the preposition as adverb.
        if (rest.Length >= 2 && (rest[0] == "under" || rest[0] == "through") && ExploratoryVerbs.Matches(verb))
        {
            var noun = string.Join(' ', rest[1..]);
            return _vocab.Nouns.Contains(noun)
                ? new SimpleIntent { Verb = verb, Noun = noun, Adverb = rest[0], OriginalInput = original, Message = original }
                : null;
        }

        // 4. Multi-noun: "verb <noun1> <preposition> <noun2>". Both must be known nouns, or we defer to the AI
        //    (so a mis-split can never turn into a wrong single-noun command).
        var prepIndex = Array.FindIndex(rest, w => ConnectingPrepositions.Contains(w));
        if (prepIndex >= 1 && prepIndex < rest.Length - 1)
        {
            var nounOne = string.Join(' ', rest[..prepIndex]);
            var nounTwo = string.Join(' ', rest[(prepIndex + 1)..]);
            return _vocab.Nouns.Contains(nounOne) && _vocab.Nouns.Contains(nounTwo)
                ? new MultiNounIntent
                {
                    Verb = verb, NounOne = nounOne, NounTwo = nounTwo, Preposition = rest[prepIndex],
                    OriginalInput = original, Message = original
                }
                : null;
        }

        // 5. Single noun: the whole post-verb phrase must be an exact known noun (so a hidden second noun,
        //    or trailing words we don't understand, defers to the AI rather than being guessed).
        var single = string.Join(' ', rest);
        if (!_vocab.Nouns.Contains(single))
            return null;
        if (BoardVerbs.Matches(verb))
            return new EnterSubLocationIntent { Noun = single, Message = original };
        return new SimpleIntent { Verb = verb, Noun = single, OriginalInput = original, Message = original };
    }

    /// <summary>Lowercases, trims trailing sentence punctuation, and strips articles.</summary>
    private static string Normalize(string input)
    {
        var lowered = input.ToLowerInvariant().Trim().TrimEnd('?', '!', '.', ',', ';');
        lowered = lowered
            .Replace(" the ", " ")
            .Replace(" a ", " ")
            .Replace(" an ", " ");
        if (lowered.StartsWith("the ")) lowered = lowered[4..];
        if (lowered.StartsWith("a ")) lowered = lowered[2..];
        if (lowered.StartsWith("an ")) lowered = lowered[3..];
        return lowered.Trim();
    }

    private readonly record struct Vocabulary(HashSet<string> Nouns, string[] DestinationTerms, HashSet<string> Verbs);
}
