using System.Collections.Concurrent;
using System.Reflection;
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

    // The three verb families the grammar treats specially are declared in Verbs.cs with every other verb:
    //   Verbs.ExploratoryVerbs - lead a "verb under/through <object>" command ("look under rug").
    //   Verbs.BoardVerbs       - "enter <thing>" is a request to get into it, which is its own intent.
    //   Verbs.DisembarkVerbs   - getting OUT of a thing; only the AI path builds that intent, so these
    //                            are declined, not parsed.

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
        //
        // Each noun is keyed by its normalised form - what Normalize makes of the player's input, so lower
        // case with the articles gone - and maps back to the noun as the game declares it. The key is what
        // the input is looked up by; the declared form is what goes into the intent, because that is what
        // a handler's MatchNoun compares against. For almost every noun the two are the same string. They
        // differ for one like Planetfall's "metal bar, curved into a u-shape", which could not be matched
        // at all while the lookup compared article-stripped input with the article-bearing declaration.
        var nounOwners = Repository.GetNounOwners(gameName);
        var nouns = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var noun in nounOwners.SelectMany(owner => owner.Nouns).Concat(Repository.GetSceneryNouns(gameName)))
        {
            var key = Normalize(noun);
            var declared = noun.ToLowerInvariant();

            // Should two declarations normalise alike ("the troll", "troll"), keep the plain one.
            if (key.Length > 0 && (!nouns.ContainsKey(key) || declared == key))
                nouns[key] = declared;
        }

        // Room names are a separate vocabulary: "kitchen" is a destination, not a noun. Stored padded and
        // article-free so IsKnownDestination can do DestinationNavigation's whole-word phrase match.
        var destinations = Repository.GetDestinationNames(gameName)
            .Select(name => $" {Normalize(name)} ")
            .Distinct()
            .ToArray();

        // The verbs are harvested exactly as the nouns are: every word of every VerbFamily the engine or
        // this game answers (Repository.GetVerbOwners). There is no verb list in this class to fall
        // behind - a handler cannot answer a verb that is not in a family. Single words only: a multi-word
        // phrase reaches the grammar through MultiWordVerbCanon or not at all.
        //
        // But a verb being KNOWN is not the same as its being ANSWERED, and who matches on a family says
        // which it is. A family an engine processor matches on is answered for any object, so its verbs
        // are trusted everywhere. A family a game class matches on is answered by that class alone: the
        // admin corridor matches "yank" and "fetch" for fishing out the key, and neither does anything to a
        // lever or a canteen, where the AI would have made them "pull" and "take". So those verbs are
        // trusted only alongside that class's own nouns, and anything else is declined.
        //
        // Where a family is DECLARED says nothing here. Every grouping is declared in Verbs.cs, for all
        // games at once; what ties "raise" to the basket is that the basket's handler reads RaiseVerbs.
        var sharedVerbs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var ownVerbs = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var (family, owner) in Repository.GetVerbOwners(gameName))
        {
            var words = family.Where(word => !word.Contains(' ')).ToArray();

            if (owner is null)
            {
                sharedVerbs.UnionWith(words);
                continue;
            }

            // The owning class and everything derived from it: a handler on an abstract base (every
            // access-card slot, every shuttle control cabin) answers for each concrete subclass.
            var itsNouns = nounOwners
                .Where(nounOwner => VerbFamilyUsage.IsOrDerivesFrom(nounOwner.Owner, owner))
                .SelectMany(nounOwner => nounOwner.Nouns)
                .Select(noun => noun.ToLowerInvariant())
                .ToArray();

            foreach (var word in words)
            {
                if (!ownVerbs.TryGetValue(word, out var answeredFor))
                    ownVerbs[word] = answeredFor = new HashSet<string>(StringComparer.Ordinal);

                answeredFor.UnionWith(itsNouns);
            }
        }

        return new Vocabulary(nouns, destinations, sharedVerbs, ownVerbs);
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
            if (!_vocab.SharedVerbs.Contains(verb) && !_vocab.OwnVerbs.ContainsKey(verb))
                return null; // not an imperative we recognise -> let the AI try
        }

        if (rest.Length == 0 || Verbs.DisembarkVerbs.Matches(verb))
            return null;

        // 3. Exploratory "look/peer under|through <object>" -> SimpleIntent with the preposition as adverb.
        if (rest.Length >= 2 && (rest[0] == "under" || rest[0] == "through") && Verbs.ExploratoryVerbs.Matches(verb))
        {
            return TryNoun(rest[1..], out var noun) && IsAnswered(verb, noun)
                ? new SimpleIntent { Verb = verb, Noun = noun, Adverb = rest[0], OriginalInput = original, Message = original }
                : null;
        }

        // 4. Single noun: the whole post-verb phrase must be an exact known noun (so a hidden second noun,
        //    or trailing words we don't understand, defers to the AI rather than being guessed). Tried BEFORE
        //    looking for a preposition to split on, because a noun may contain one: "metal bar, curved into
        //    a u-shape" is one object, not a bar and a u-shape.
        if (TryNoun(rest, out var single))
        {
            if (!IsAnswered(verb, single))
                return null;

            return Verbs.BoardVerbs.Matches(verb)
                ? new EnterSubLocationIntent { Noun = single, Message = original }
                : new SimpleIntent { Verb = verb, Noun = single, OriginalInput = original, Message = original };
        }

        // Getting into something takes one object. "enter the house through the window" is a way of MOVING,
        // which the AI resolves; as a two-noun "enter" it would be a command nothing answers.
        if (Verbs.BoardVerbs.Matches(verb))
            return null;

        // 5. Multi-noun: "verb <noun1> <preposition> <noun2>". Both sides must be known nouns, or we defer to
        //    the AI (so a mis-split can never turn into a wrong command). Every preposition is tried as the
        //    split point, not just the first, for the same reason as above: the first one may belong to a noun.
        for (var index = 1; index < rest.Length - 1; index++)
            if (ConnectingPrepositions.Contains(rest[index]) &&
                TryNoun(rest[..index], out var nounOne) && TryNoun(rest[(index + 1)..], out var nounTwo) &&
                IsAnswered(verb, nounOne, nounTwo))
                return new MultiNounIntent
                {
                    Verb = verb, NounOne = nounOne, NounTwo = nounTwo, Preposition = rest[index],
                    OriginalInput = original, Message = original
                };

        return null;
    }

    /// <summary>
    ///     Looks the phrase up as a noun the game declares, handing back the declared form.
    /// </summary>
    private bool TryNoun(string[] words, out string declared)
    {
        return _vocab.Nouns.TryGetValue(string.Join(' ', words), out declared!);
    }

    /// <summary>Lowercases, trims trailing sentence punctuation, collapses whitespace, and strips articles.</summary>
    private static string Normalize(string input)
    {
        var lowered = input.ToLowerInvariant().Trim().TrimEnd('?', '!', '.', ',', ';');

        // Runs of whitespace become one space BEFORE anything is matched. Splitting into words copes with a
        // doubled space on its own, but the travel prefixes ("go to ") and the article stripping below are
        // matched as text, so "go  to  the  kitchen" used to slip past both and be declined.
        lowered = string.Join(' ', lowered.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        lowered = lowered
            .Replace(" the ", " ")
            .Replace(" a ", " ")
            .Replace(" an ", " ");
        if (lowered.StartsWith("the ")) lowered = lowered[4..];
        if (lowered.StartsWith("a ")) lowered = lowered[2..];
        if (lowered.StartsWith("an ")) lowered = lowered[3..];
        return lowered.Trim();
    }

    /// <summary>
    ///     True when something answers <paramref name="verb" /> for these nouns: a verb the engine shares
    ///     across objects always, an object's own verb only when one of the nouns is that object's.
    /// </summary>
    private bool IsAnswered(string verb, params string[] declaredNouns)
    {
        return _vocab.SharedVerbs.Contains(verb) ||
               (_vocab.OwnVerbs.TryGetValue(verb, out var answeredFor) && declaredNouns.Any(answeredFor.Contains));
    }

    private readonly record struct Vocabulary(Dictionary<string, string> Nouns, string[] DestinationTerms,
        HashSet<string> SharedVerbs, Dictionary<string, HashSet<string>> OwnVerbs);
}
