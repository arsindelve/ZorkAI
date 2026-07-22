using System.Collections.Concurrent;
using System.Reflection;
using Model;
using Model.Intent;

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
/// AND the noun(s) against the known vocabulary, so it is never wrong, only sometimes silent. The known
/// verbs are derived from <see cref="Verbs" /> (single source of truth); multi-word verb phrases
/// ("turn on", "put on", "look at") are canonicalised to the engine's canonical verb.
/// </summary>
public class DeterministicParser
{
    private static readonly ConcurrentDictionary<string, Vocabulary> VocabCache = new();

    private readonly Vocabulary _vocab;

    // Verbs the engine handles that are NOT yet in the Verbs thesaurus. Verbs.cs is a relatively new,
    // still-incomplete construct: verbs like "read" and "tie" are handled by the game (via item processors
    // and, previously, the TestParser's JSON fixtures) but were never centralised. Surfacing these is the
    // point of migrating tests to this parser — each is a real command. TODO: fold these into Verbs.cs
    // proper (new families) so there is a single source of truth; kept here meanwhile.
    private static readonly string[] SupplementalVerbs =
    [
        "read", "tie", "untie", "fill", "pour", "dig", "type", "key", "punch", "set", "count", "use", "wave",
        "ring", "smell", "light", "wind", "kick", "deflate", "inflate", "lower", "raise", "extend", "lift",
        "oil", "lubricate", "salute", "eat", "play", "shoot", "empty", "remove", "spin", "spray", "water",
        "pump", "knock", "poke", "prod", "wear", "unlock", "lock", "flip", "board", "enter"
    ];

    // Every single-word verb the engine knows, harvested from the Verbs thesaurus (single source of truth)
    // plus the still-uncentralised supplement above. A synonym added to Verbs.cs is understood here
    // automatically. Multi-word entries (e.g. "look at") are handled by MultiWordVerbCanon instead.
    private static readonly HashSet<string> KnownVerbs = typeof(Verbs)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(f => f.FieldType == typeof(string[]))
        .SelectMany(f => (string[])f.GetValue(null)!)
        .Where(v => !v.Contains(' '))
        .Concat(SupplementalVerbs)
        .ToHashSet(StringComparer.OrdinalIgnoreCase);

    // Multi-word verb phrases -> the engine's canonical single verb. These MUST be canonicalised because the
    // second word would otherwise be read as a preposition/noun ("turn on lamp" -> verb "turn", "on lamp").
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
    private static readonly HashSet<string> ExploratoryVerbs = new(StringComparer.OrdinalIgnoreCase)
    {
        "look", "peer", "peek", "glance", "examine", "search", "inspect"
    };

    private static readonly string[] TravelPrefixes =
        ["go to ", "walk to ", "head to ", "travel to ", "go into ", "walk into ", "run to ", "proceed to "];

    public DeterministicParser(string gameName)
    {
        _vocab = VocabCache.GetOrAdd(gameName, Load);
    }

    private static Vocabulary Load(string gameName) =>
        new(new HashSet<string>(Repository.GetNouns(gameName), StringComparer.OrdinalIgnoreCase));

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
            return string.IsNullOrWhiteSpace(destination)
                ? null
                : new GoToDestinationIntent { Destination = destination, Message = original };
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
            if (!KnownVerbs.Contains(verb))
                return null; // not an imperative we recognise -> let the AI try
        }

        if (rest.Length == 0)
            return null;

        // 3. Exploratory "look/peer under|through <object>" -> SimpleIntent with the preposition as adverb.
        if (rest.Length >= 2 && (rest[0] == "under" || rest[0] == "through") && ExploratoryVerbs.Contains(verb))
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
        return _vocab.Nouns.Contains(single)
            ? new SimpleIntent { Verb = verb, Noun = single, OriginalInput = original, Message = original }
            : null;
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

    private readonly record struct Vocabulary(HashSet<string> Nouns);
}
