using System.Collections.Concurrent;
using Model.Intent;

namespace GameEngine;

/// <summary>
/// Layer 1 of the parser plan: a deterministic, vocabulary-driven first pass that resolves the common,
/// unambiguous command shapes instantly and reproducibly — with NO AI call — against the game's own
/// vocabulary (<see cref="Repository.GetNouns" /> / <see cref="Repository.GetContainers" /> plus a fixed
/// verb set). What it handles today:
///
///   * "go to &lt;place&gt;" / "walk into &lt;place&gt;"          -> GoToDestinationIntent   (#268)
///   * "put &lt;item&gt; in &lt;container&gt;"                    -> MultiNounIntent
///   * "&lt;verb&gt; under &lt;object&gt;" / "&lt;verb&gt; through &lt;object&gt;" -> SimpleIntent (adverb)
///   * "&lt;action-verb&gt; &lt;known object&gt;"                 -> SimpleIntent
///
/// It is intentionally CONSERVATIVE — the whole point is that it is *never wrong*, only sometimes silent.
/// It returns null for anything it isn't certain about so the caller falls back to the existing AI parser.
/// Two safety rules make it regression-proof:
///
///   1. It never handles take/drop/get/carry (those fan out to the separate multi-item take/drop AI, so a
///      deterministic short-circuit here would drop the multi-item behavior).
///   2. The generic "verb + object" path requires the ENTIRE post-verb phrase to be an exact known noun,
///      so a command with a tool/second noun ("attack troll with sword", "unlock door with key") never
///      matches — its remainder isn't a single known noun — and correctly falls back to the multi-noun AI.
///
/// Because it emits exactly the intents the AI emits for these clean shapes, promoting it changes latency,
/// cost, and determinism — not behavior.
/// </summary>
public class DeterministicParser
{
    private static readonly ConcurrentDictionary<string, Vocabulary> VocabCache = new();

    private readonly Vocabulary _vocab;

    // Action verbs safe for the generic "verb + exact known noun" path. Derived from the proven
    // TestParser verb list, minus take/drop/get/carry (multi-item AI) and the movement verbs (handled
    // earlier as directions/goto). Synonyms not listed here simply fall through to the AI.
    private static readonly HashSet<string> ActionVerbs = new(StringComparer.OrdinalIgnoreCase)
    {
        "open", "close", "shut", "examine", "inspect", "look", "eat", "press", "remove", "play", "shoot",
        "deactivate", "type", "key", "punch", "push", "pull", "burn", "set", "search", "empty", "wear",
        "drink", "use", "count", "touch", "read", "turn", "wave", "move", "ring", "activate", "smell",
        "throw", "light", "rub", "kiss", "wind", "kick", "deflate", "lower", "raise", "inflate", "unlock",
        "lock", "climb", "extend", "lift", "shake", "oil", "lubricate", "break", "smash", "squeeze",
        "attack", "kill", "salute", "peer", "peek", "flip", "board"
    };

    // Verbs that may lead a "verb PREPOSITION object" exploratory command ("look under rug", "peer through
    // crack", "go through window", "climb through hatch").
    private static readonly HashSet<string> PrepositionLeadVerbs = new(StringComparer.OrdinalIgnoreCase)
    {
        "look", "peer", "peek", "glance", "examine", "search", "go", "walk", "climb", "crawl", "move"
    };

    private static readonly string[] TravelPrefixes =
        ["go to ", "walk to ", "head to ", "travel to ", "go into ", "walk into ", "run to ", "proceed to "];

    public DeterministicParser(string gameName)
    {
        _vocab = VocabCache.GetOrAdd(gameName, Load);
    }

    private static Vocabulary Load(string gameName) =>
        new(
            new HashSet<string>(Repository.GetNouns(gameName), StringComparer.OrdinalIgnoreCase),
            new HashSet<string>(Repository.GetContainers(gameName), StringComparer.OrdinalIgnoreCase));

    /// <summary>
    /// Attempts a deterministic parse. Returns the intent when confident; null to signal "let the AI parse".
    /// </summary>
    public IntentBase? Parse(string? rawInput)
    {
        if (string.IsNullOrWhiteSpace(rawInput))
            return null;

        var original = rawInput.Trim();
        var input = Normalize(original);

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

        var verb = words[0];

        // 2. "put <item> in <container>" -> MultiNounIntent. Mirrors the proven TestParser ordering
        //    (NounOne = the item, NounTwo = the container).
        if (verb == "put" && words.Length >= 4)
        {
            var prepIndex = Array.FindIndex(words, w => w is "in" or "into" or "inside" or "on" or "onto");
            if (prepIndex > 1 && prepIndex < words.Length - 1)
            {
                var item = string.Join(' ', words[1..prepIndex]);
                var container = string.Join(' ', words[(prepIndex + 1)..]);
                if (_vocab.Nouns.Contains(item) && _vocab.Containers.Contains(container))
                    return new MultiNounIntent
                    {
                        Verb = "put",
                        NounOne = item,
                        NounTwo = container,
                        Preposition = "in",
                        OriginalInput = original,
                        Message = original
                    };
            }

            return null; // a "put" we can't confidently resolve -> let the AI try
        }

        // 3. "<verb> under/through <object>" -> exploratory look/go with the preposition as the adverb.
        if (words.Length >= 3 && (words[1] == "under" || words[1] == "through") &&
            PrepositionLeadVerbs.Contains(verb))
        {
            var noun = string.Join(' ', words[2..]);
            if (_vocab.Nouns.Contains(noun))
                return new SimpleIntent
                {
                    Verb = verb,
                    Noun = noun,
                    Adverb = words[1],
                    OriginalInput = original,
                    Message = original
                };

            return null;
        }

        // 4. Generic "<action-verb> <known object>". The remainder after the verb must be an EXACT known
        //    noun — so anything with a tool/second noun ("... with X") cannot match and falls back to the AI.
        if (ActionVerbs.Contains(verb))
        {
            var noun = string.Join(' ', words[1..]);
            if (_vocab.Nouns.Contains(noun))
                return new SimpleIntent
                {
                    Verb = verb,
                    Noun = noun,
                    OriginalInput = original,
                    Message = original
                };
        }

        return null;
    }

    /// <summary>Lowercases, trims, and strips leading articles so "Open the Mailbox" == "open mailbox".</summary>
    private static string Normalize(string input)
    {
        var lowered = input.ToLowerInvariant().Trim();
        // Strip articles anywhere they appear as whole words, matching the TestParser's "the " stripping.
        lowered = lowered
            .Replace(" the ", " ")
            .Replace(" a ", " ")
            .Replace(" an ", " ");
        if (lowered.StartsWith("the ")) lowered = lowered[4..];
        if (lowered.StartsWith("a ")) lowered = lowered[2..];
        if (lowered.StartsWith("an ")) lowered = lowered[3..];
        return lowered.Trim();
    }

    private readonly record struct Vocabulary(HashSet<string> Nouns, HashSet<string> Containers);
}
